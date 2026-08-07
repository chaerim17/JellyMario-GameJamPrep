using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JellyMario.Jelly
{
    [DisallowMultipleComponent]
    public sealed class JellySurfaceWave : MonoBehaviour
    {
        private const int ShaderWaveSlotCount = 4;

        [Header("참조")]
        [Tooltip("출렁임 Shader가 적용된 표면 Renderer")]
        [SerializeField] private Renderer surfaceRenderer;

        [Header("반응할 레이어")]
        [SerializeField] private LayerMask reactingLayers = ~0;

        [Header("충돌 설정")]
        [SerializeField, Min(0f)] private float minimumImpactSpeed = 0.8f;
        [SerializeField, Min(0f)] private float impactResponse = 0.08f;
        [SerializeField, Min(0f)] private float maxImpactStrength = 0.5f;

        [Tooltip("같은 물체의 중복 충돌을 무시하는 시간")]
        [SerializeField, Min(0f)] private float impactCooldown = 0.4f;

        [Tooltip("하나의 파동이 유지되는 시간")]
        [SerializeField, Min(0.01f)] private float visualWaveDuration = 3f;

        [Tooltip("동시에 유지할 충돌 파동 개수")]
        [SerializeField, Range(1, ShaderWaveSlotCount)]
        private int concurrentWaveLimit = ShaderWaveSlotCount;

        [Tooltip("같은 충돌의 접촉점을 서로 다른 지점으로 판단하는 최소 거리")]
        [SerializeField, Min(0f)] private float minimumContactSeparation = 0.75f;

        [Header("비주얼 파동 설정")]
        [SerializeField, Min(0f)] private float impactFrequency = 1.2f;
        [SerializeField, Min(0f)] private float impactSpeed = 1.5f;
        [SerializeField, Min(0f)] private float impactFalloff = 0.25f;
        [SerializeField, Min(0f)] private float impactDecay = 0.6f;

        [Tooltip("Shader에서 보여주는 파동 높이 배율")]
        [SerializeField, Min(0f)] private float visualWaveHeight = 1.6f;

        [Tooltip("여러 비주얼 파동이 합쳐졌을 때의 최대 이동 거리")]
        [SerializeField, Min(0f)] private float maxCombinedVisualOffset = 1.25f;

        [Header("타일 Collider 파동")]
        [Tooltip("Tilemap 타일과 Collider를 파동처럼 함께 움직입니다.")]
        [SerializeField] private bool animateTileCollider = true;

        [SerializeField] private Tilemap surfaceTilemap;
        [SerializeField] private TilemapCollider2D tilemapCollider;

        [SerializeField, Min(0f)] private float colliderWaveRadius = 6f;
        [SerializeField, Min(0f)] private float colliderWaveHeight = 0.12f;
        [SerializeField, Min(0.01f)] private float colliderWaveTravelSpeed = 2.5f;
        [SerializeField, Min(0f)] private float colliderWaveFrequency = 2.8f;
        [SerializeField, Min(0f)] private float colliderWaveDecay = 1f;
        [SerializeField, Min(0f)] private float colliderDistanceFalloff = 0.18f;

        [Tooltip("여러 Collider 파동이 합쳐졌을 때의 최대 이동 거리")]
        [SerializeField, Min(0f)] private float maxCombinedColliderOffset = 0.35f;

        private MaterialPropertyBlock _propertyBlock;

        private readonly VisualWaveSlot[] _visualWaveSlots =
            new VisualWaveSlot[ShaderWaveSlotCount];

        private readonly List<ColliderWave> _colliderWaves = new();
        private readonly Dictionary<Vector3Int, TileWaveCell> _waveCells = new();
        private readonly List<ImpactCooldownRecord> _impactCooldownRecords = new();

        private static readonly int[] ImpactDataIds =
        {
            Shader.PropertyToID("_ImpactData0"),
            Shader.PropertyToID("_ImpactData1"),
            Shader.PropertyToID("_ImpactData2"),
            Shader.PropertyToID("_ImpactData3")
        };

        private static readonly int[] ImpactNormalIds =
        {
            Shader.PropertyToID("_ImpactNormal0"),
            Shader.PropertyToID("_ImpactNormal1"),
            Shader.PropertyToID("_ImpactNormal2"),
            Shader.PropertyToID("_ImpactNormal3")
        };

        private static readonly int ImpactFrequencyId = Shader.PropertyToID("_ImpactFrequency");
        private static readonly int ImpactSpeedId = Shader.PropertyToID("_ImpactSpeed");
        private static readonly int ImpactFalloffId = Shader.PropertyToID("_ImpactFalloff");
        private static readonly int ImpactDecayId = Shader.PropertyToID("_ImpactDecay");
        private static readonly int WaveHeightId = Shader.PropertyToID("_WaveHeightMultiplier");
        private static readonly int MaxCombinedWaveId = Shader.PropertyToID("_MaxCombinedWaveOffset");

        private sealed class VisualWaveSlot
        {
            public bool Active;
            public float EndTime;
        }

        private sealed class ColliderWave
        {
            public Vector2 LocalContactPoint;
            public Vector2 LocalNormal;
            public float StartTime;
            public float EndTime;
        }

        private sealed class ImpactCooldownRecord
        {
            public GameObject SourceObject;
            public Vector2 WorldPoint;
            public float EndTime;
        }

        private sealed class TileWaveCell
        {
            public Matrix4x4 OriginalTransform;
            public TileFlags OriginalFlags;
            public Vector2 LocalCenter;
        }

        private void Awake()
        {
            if (!EnsureRuntimeState())
            {
                Debug.LogError("출렁이게 할 Surface Renderer가 등록되지 않았습니다.", this);

                enabled = false;
                return;
            }

            if (animateTileCollider && (surfaceTilemap == null || tilemapCollider == null))
            {
                Debug.LogWarning("Tilemap과 TilemapCollider2D가 없어 비주얼 파동만 사용합니다.", this);

                animateTileCollider = false;
            }

            ApplyWaveSettings();
            ClearAllVisualWaves();
        }

        private void OnEnable()
        {
            // 플레이 중 스크립트가 다시 컴파일되면 Awake가 다시 호출되지 않은 채
            // 직렬화되지 않는 MaterialPropertyBlock만 사라질 수 있다.
            if (EnsureRuntimeState())
                ApplyWaveSettings();
        }

        private bool EnsureRuntimeState()
        {
            if (surfaceRenderer == null)
                surfaceRenderer = GetComponent<Renderer>();

            if (surfaceTilemap == null)
                surfaceTilemap = GetComponent<Tilemap>();

            if (tilemapCollider == null)
                tilemapCollider = GetComponent<TilemapCollider2D>();

            if (surfaceRenderer == null)
                return false;

            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            for (int index = 0; index < ShaderWaveSlotCount; index++)
            {
                if (_visualWaveSlots[index] == null)
                    _visualWaveSlots[index] = new VisualWaveSlot();
            }

            return true;
        }

        private void FixedUpdate()
        {
            if (!EnsureRuntimeState())
                return;

            UpdateVisualWaves();
            UpdateTileColliderWaves();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!CanReact(collision.gameObject.layer) || collision.contactCount == 0)
                return;
            
            int collisionWaveLimit = Mathf.Clamp(concurrentWaveLimit, 1, ShaderWaveSlotCount);

            Vector2[] acceptedPoints = new Vector2[ShaderWaveSlotCount];
            int acceptedCount = 0;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                float contactSpeed = Mathf.Abs(Vector2.Dot(collision.relativeVelocity, contact.normal));

                if (contactSpeed < minimumImpactSpeed)
                    continue;

                if (IsImpactCoolingDown(collision.gameObject, contact.point))
                    continue;

                bool overlapsAcceptedPoint = false;

                for (int index = 0; index < acceptedCount; index++)
                {
                    if (Vector2.Distance(acceptedPoints[index], contact.point) < minimumContactSeparation)
                    {
                        overlapsAcceptedPoint = true;

                        break;
                    }
                }

                if (overlapsAcceptedPoint)
                    continue;

                PlayRipple(contact.point, contact.normal, contactSpeed);
                RememberImpact(collision.gameObject, contact.point);

                acceptedPoints[acceptedCount] = contact.point;
                acceptedCount++;

                if (acceptedCount >= collisionWaveLimit)
                    break;
            }

        }

        private bool IsImpactCoolingDown(GameObject sourceObject, Vector2 worldPoint)
        {
            float separation = Mathf.Max(minimumContactSeparation, 0.05f);

            for (int index = _impactCooldownRecords.Count - 1; index >= 0; index--)
            {
                ImpactCooldownRecord record = _impactCooldownRecords[index];

                if (Time.time >= record.EndTime || record.SourceObject == null)
                {
                    _impactCooldownRecords.RemoveAt(index);

                    continue;
                }

                if (record.SourceObject == sourceObject && Vector2.Distance(record.WorldPoint, worldPoint) < separation)
                    return true;
            }

            return false;
        }

        private void RememberImpact(GameObject sourceObject, Vector2 worldPoint)
        {
            if (impactCooldown <= 0f)
                return;

            _impactCooldownRecords.Add(new ImpactCooldownRecord
                {
                    SourceObject = sourceObject, WorldPoint = worldPoint, EndTime = Time.time + impactCooldown
                });
        }

        public void PlayRipple(Vector2 worldContactPoint, Vector2 worldContactNormal, float collisionSpeed)
        {
            if (!EnsureRuntimeState())
                return;

            float strength = Mathf.Clamp(Mathf.Abs(collisionSpeed) * impactResponse, 0f, maxImpactStrength);

            if (strength <= 0f)
                return;

            Vector3 localPoint3D = surfaceRenderer.transform.InverseTransformPoint(worldContactPoint);
            Vector3 localNormal3D = surfaceRenderer.transform.InverseTransformDirection(worldContactNormal);
            Vector2 localNormal = new Vector2(localNormal3D.x, localNormal3D.y).normalized;

            if (localNormal.sqrMagnitude < 0.001f)
                localNormal = Vector2.up;

            int slotIndex = FindVisualWaveSlot();
            VisualWaveSlot slot = _visualWaveSlots[slotIndex];
            slot.Active = true;
            slot.EndTime = Time.time + visualWaveDuration;

            surfaceRenderer.GetPropertyBlock(_propertyBlock);

            _propertyBlock.SetVector(ImpactDataIds[slotIndex], new Vector4(localPoint3D.x, localPoint3D.y, Time.time, strength));
            _propertyBlock.SetVector(ImpactNormalIds[slotIndex], new Vector4(localNormal.x, localNormal.y, 0f, 0f));

            surfaceRenderer.SetPropertyBlock(_propertyBlock);

            StartTileColliderWave(worldContactPoint, worldContactNormal);
        }

        public void PlayRipple(Vector2 worldContactPoint, float collisionSpeed)
        {
            PlayRipple(worldContactPoint, Vector2.up, collisionSpeed);
        }

        private int FindVisualWaveSlot()
        {
            int limit = Mathf.Clamp(concurrentWaveLimit, 1, ShaderWaveSlotCount);

            int oldestSlot = 0;
            float oldestEndTime = float.PositiveInfinity;

            for (int index = 0; index < limit; index++)
            {
                VisualWaveSlot slot = _visualWaveSlots[index];

                if (!slot.Active)
                    return index;

                if (slot.EndTime < oldestEndTime)
                {
                    oldestEndTime = slot.EndTime;
                    oldestSlot = index;
                }
            }

            return oldestSlot;
        }

        private void UpdateVisualWaves()
        {
            bool propertyChanged = false;

            surfaceRenderer.GetPropertyBlock(_propertyBlock);

            for (int index = 0; index < ShaderWaveSlotCount; index++)
            {
                VisualWaveSlot slot = _visualWaveSlots[index];

                if (!slot.Active || Time.time < slot.EndTime)
                    continue;

                slot.Active = false;
                ClearVisualWaveSlot(index);
                propertyChanged = true;
            }

            if (propertyChanged)
                surfaceRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void ClearVisualWaveSlot(int index)
        {
            _propertyBlock.SetVector(ImpactDataIds[index], new Vector4(0f, 0f, -1000f, 0f));
            _propertyBlock.SetVector(ImpactNormalIds[index], new Vector4(0f, 1f, 0f, 0f));
        }

        private void ClearAllVisualWaves()
        {
            surfaceRenderer.GetPropertyBlock(_propertyBlock);

            for (int index = 0; index < ShaderWaveSlotCount; index++)
            {
                _visualWaveSlots[index].Active = false;
                ClearVisualWaveSlot(index);
            }

            surfaceRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void StartTileColliderWave(Vector2 worldContactPoint, Vector2 worldContactNormal)
        {
            if (!animateTileCollider || surfaceTilemap == null)
                return;

            Vector3 localPoint3D = surfaceTilemap.transform.InverseTransformPoint(worldContactPoint);
            Vector3 localNormal3D = surfaceTilemap.transform.InverseTransformDirection(worldContactNormal);
            Vector2 localNormal = new Vector2(localNormal3D.x, localNormal3D.y).normalized;

            if (localNormal.sqrMagnitude < 0.001f)
                localNormal = Vector2.up;

            float travelDuration = colliderWaveRadius / Mathf.Max(colliderWaveTravelSpeed, 0.01f);
            float decayDuration = colliderWaveDecay > 0f
                ? 5f / colliderWaveDecay
                : 3f;
            int colliderWaveLimit = Mathf.Clamp(concurrentWaveLimit, 1, ShaderWaveSlotCount);

            if (_colliderWaves.Count >= colliderWaveLimit)
                _colliderWaves.RemoveAt(0);

            _colliderWaves.Add(new ColliderWave
            {
                LocalContactPoint = new Vector2(localPoint3D.x, localPoint3D.y),
                LocalNormal = localNormal,
                StartTime = Time.time,
                EndTime = Time.time + travelDuration + decayDuration
            });

            RegisterWaveCells(localPoint3D);
        }

        private void RegisterWaveCells(Vector3 localContactPoint)
        {
            Vector3 cellSize = surfaceTilemap.layoutGrid.cellSize;
            float smallestCellSize = Mathf.Max(0.001f, Mathf.Min(Mathf.Abs(cellSize.x), Mathf.Abs(cellSize.y)));

            int cellRadius = Mathf.CeilToInt(colliderWaveRadius / smallestCellSize);

            Vector3Int centerCell = surfaceTilemap.LocalToCell(localContactPoint);

            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                for (int x = -cellRadius; x <= cellRadius; x++)
                {
                    Vector3Int cell = centerCell + new Vector3Int(x, y, 0);
                    if (!surfaceTilemap.HasTile(cell) || _waveCells.ContainsKey(cell))
                        continue;

                    Vector3 cellCenter = surfaceTilemap.GetCellCenterLocal(cell);
                    if (Vector2.Distance(cellCenter, localContactPoint) > colliderWaveRadius)
                        continue;

                    TileFlags originalFlags = surfaceTilemap.GetTileFlags(cell);

                    _waveCells[cell] = new TileWaveCell
                    {
                        OriginalTransform = surfaceTilemap.GetTransformMatrix(cell),
                        OriginalFlags = originalFlags,
                        LocalCenter = new Vector2(cellCenter.x, cellCenter.y)
                    };

                    surfaceTilemap.SetTileFlags(cell, originalFlags & ~TileFlags.LockTransform);
                }
            }
        }

        private void UpdateTileColliderWaves()
        {
            if (_colliderWaves.Count == 0 || surfaceTilemap == null)
                return;

            for (int index = _colliderWaves.Count - 1; index >= 0; index--)
            {
                if (Time.time >= _colliderWaves[index].EndTime)
                    _colliderWaves.RemoveAt(index);
            }

            if (_colliderWaves.Count == 0)
            {
                ResetTileColliderWaves();

                return;
            }

            foreach (KeyValuePair<Vector3Int, TileWaveCell> pair in _waveCells)
            {
                Vector2 combinedOffset = Vector2.zero;
                TileWaveCell cell = pair.Value;

                foreach (ColliderWave wave in _colliderWaves)
                {
                    float distance = Vector2.Distance(cell.LocalCenter, wave.LocalContactPoint);
                    if (distance > colliderWaveRadius)
                        continue;

                    float delayedTime = Time.time - wave.StartTime - distance / Mathf.Max(colliderWaveTravelSpeed, 0.01f);
                    if (delayedTime < 0f)
                        continue;

                    float timeEnvelope = Mathf.Exp(-delayedTime * colliderWaveDecay);
                    float distanceEnvelope = Mathf.Exp(-distance * colliderDistanceFalloff);
                    float waveOffset = Mathf.Sin(delayedTime * colliderWaveFrequency) * colliderWaveHeight * timeEnvelope * distanceEnvelope;

                    combinedOffset += wave.LocalNormal * waveOffset;
                }

                combinedOffset = Vector2.ClampMagnitude(combinedOffset, maxCombinedColliderOffset);

                Matrix4x4 waveTransform = Matrix4x4.Translate(combinedOffset) * cell.OriginalTransform;

                surfaceTilemap.SetTransformMatrix(pair.Key, waveTransform);
            }

            ProcessTilemapColliderChanges();
        }

        private void ResetTileColliderWaves()
        {
            if (surfaceTilemap == null || _waveCells.Count == 0)
                return;

            foreach (KeyValuePair<Vector3Int, TileWaveCell> pair in _waveCells)
            {
                surfaceTilemap.SetTransformMatrix(pair.Key, pair.Value.OriginalTransform);
                surfaceTilemap.SetTileFlags(pair.Key, pair.Value.OriginalFlags);
            }

            _waveCells.Clear();
            _colliderWaves.Clear();
            ProcessTilemapColliderChanges();
        }

        private void ProcessTilemapColliderChanges()
        {
            if (tilemapCollider != null && tilemapCollider.hasTilemapChanges)
                tilemapCollider.ProcessTilemapChanges();
        }

        private void ApplyWaveSettings()
        {
            surfaceRenderer.GetPropertyBlock(_propertyBlock);

            _propertyBlock.SetFloat(ImpactFrequencyId, impactFrequency);
            _propertyBlock.SetFloat(ImpactSpeedId, impactSpeed);
            _propertyBlock.SetFloat(ImpactFalloffId, impactFalloff);
            _propertyBlock.SetFloat(ImpactDecayId, impactDecay);
            _propertyBlock.SetFloat(WaveHeightId, visualWaveHeight);
            _propertyBlock.SetFloat(MaxCombinedWaveId, maxCombinedVisualOffset);

            surfaceRenderer.SetPropertyBlock(_propertyBlock);
        }

        private bool CanReact(int objectLayer)
        {
            int objectLayerMask = 1 << objectLayer;
            return (reactingLayers.value & objectLayerMask) != 0;
        }

        private void OnDisable()
        {
            ResetTileColliderWaves();

            if (surfaceRenderer != null && _propertyBlock != null)
                ClearAllVisualWaves();
        }
    }
}