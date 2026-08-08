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

        [Tooltip("하나의 파동이 유지되는 시간. 마지막 25% 구간에서 자연스럽게 사라집니다.")]
        [SerializeField, Min(0.01f)] private float visualWaveDuration = 3f;

        [Tooltip("동시에 유지할 충돌 파동 개수")]
        [SerializeField, Range(1, ShaderWaveSlotCount)]
        private int concurrentWaveLimit = ShaderWaveSlotCount;

        [Tooltip("같은 충돌의 접촉점을 서로 다른 지점으로 판단하는 최소 거리")]
        [SerializeField, Min(0f)] private float minimumContactSeparation = 0.75f;

        [Header("공통 파동 설정")]
        [SerializeField, Min(0f)] private float impactFrequency = 1.2f;
        [SerializeField, Min(0f)] private float impactSpeed = 1.5f;
        [SerializeField, Min(0f)] private float impactFalloff = 0.25f;
        [SerializeField, Min(0f)] private float impactDecay = 0.6f;

        [Tooltip("화면과 Collider에 함께 적용되는 파동 높이 배율")]
        [SerializeField, Min(0f)] private float visualWaveHeight = 1.6f;

        [Tooltip("여러 파동이 합쳐졌을 때 화면과 Collider의 최대 이동 거리")]
        [SerializeField, Min(0f)] private float maxCombinedVisualOffset = 1.25f;

        [Header("파동 Collider")]
        [Tooltip("화면의 파동과 같은 공식으로 Collider 윤곽을 움직입니다.")]
        [SerializeField] private bool animateTileCollider = true;

        [SerializeField] private Tilemap surfaceTilemap;
        [SerializeField] private TilemapCollider2D tilemapCollider;

        private MaterialPropertyBlock _propertyBlock;

        private readonly VisualWaveSlot[] _visualWaveSlots =
            new VisualWaveSlot[ShaderWaveSlotCount];

        private readonly List<ImpactCooldownRecord> _impactCooldownRecords = new();
        private readonly List<DeformableColliderPath> _colliderPaths = new();

        private CompositeCollider2D _sourceCompositeCollider;
        private Rigidbody2D _sourceRigidbody;
        private GameObject _runtimeColliderRoot;
        private bool _runtimeColliderDeformed;
        private bool _createdCompositeCollider;
        private bool _createdRigidbody;
        private bool _originalTilemapColliderEnabled;
        private bool _originalCompositeColliderEnabled;
        private Collider2D.CompositeOperation _originalCompositeOperation;
        private CompositeCollider2D.GeometryType _originalGeometryType;
        private CompositeCollider2D.GenerationType _originalGenerationType;

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
        private static readonly int WaveDurationId = Shader.PropertyToID("_WaveDuration");

        private sealed class VisualWaveSlot
        {
            public bool Active;
            public Vector2 LocalContactPoint;
            public Vector2 LocalNormal = Vector2.up;
            public float StartTime;
            public float EndTime;
            public float Strength;
        }

        private sealed class ImpactCooldownRecord
        {
            public GameObject SourceObject;
            public Vector2 WorldPoint;
            public float EndTime;
        }

        private sealed class DeformableColliderPath
        {
            public EdgeCollider2D Collider;
            public Vector2[] OriginalPoints;
            public Vector2[] DeformedPoints;
        }

        private void Awake()
        {
            if (!EnsureRuntimeState())
            {
                Debug.LogError("출렁이게 할 Surface Renderer가 등록되지 않았습니다.", this);
                enabled = false;
                return;
            }

            ApplyWaveSettings();
            ClearAllVisualWaves();

            if (animateTileCollider && !InitializeRuntimeWaveCollider())
            {
                Debug.LogWarning(
                    "파동 Collider를 만들 수 없어 비주얼 파동만 사용합니다. " +
                    "TilemapCollider2D와 Rigidbody2D 설정을 확인해 주세요.",
                    this);
                animateTileCollider = false;
            }
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

            _propertyBlock ??= new MaterialPropertyBlock();

            for (int index = 0; index < ShaderWaveSlotCount; index++)
                _visualWaveSlots[index] ??= new VisualWaveSlot();

            return true;
        }

        private void FixedUpdate()
        {
            if (!EnsureRuntimeState())
                return;

            UpdateVisualWaves();
            UpdateRuntimeWaveCollider();
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

                if (record.SourceObject == sourceObject &&
                    Vector2.Distance(record.WorldPoint, worldPoint) < separation)
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
                SourceObject = sourceObject,
                WorldPoint = worldPoint,
                EndTime = Time.time + impactCooldown
            });
        }

        public void PlayRipple(Vector2 worldContactPoint, Vector2 worldContactNormal, float collisionSpeed)
        {
            if (!EnsureRuntimeState())
                return;

            float strength = Mathf.Clamp(
                Mathf.Abs(collisionSpeed) * impactResponse,
                0f,
                maxImpactStrength);

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
            slot.LocalContactPoint = new Vector2(localPoint3D.x, localPoint3D.y);
            slot.LocalNormal = localNormal;
            slot.StartTime = Time.time;
            slot.EndTime = Time.time + visualWaveDuration;
            slot.Strength = strength;

            surfaceRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(
                ImpactDataIds[slotIndex],
                new Vector4(localPoint3D.x, localPoint3D.y, slot.StartTime, strength));
            _propertyBlock.SetVector(
                ImpactNormalIds[slotIndex],
                new Vector4(localNormal.x, localNormal.y, 0f, 0f));
            surfaceRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void PlayRipple(Vector2 worldContactPoint, float collisionSpeed)
        {
            PlayRipple(worldContactPoint, Vector2.up, collisionSpeed);
        }

        public Vector2 GetSurfaceDeltaAtWorldPoint(Vector2 worldPoint)
        {
            if (!HasActiveWave())
                return Vector2.zero;

            Vector3 localPoint3D = surfaceRenderer.transform.InverseTransformPoint(worldPoint);
            Vector2 localPoint = new Vector2(localPoint3D.x, localPoint3D.y);
            float sampleInterval = Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            float currentTime = Time.time;

            VisualWaveSlot contactWave = FindClosestActiveWaveSlot(localPoint, currentTime);

            if (contactWave == null)
                return Vector2.zero;

            Vector2 currentOffset = EvaluateLocalWaveOffset(
                contactWave,
                localPoint,
                currentTime);
            Vector2 previousOffset = EvaluateLocalWaveOffset(
                contactWave,
                localPoint,
                currentTime - sampleInterval);

            Vector3 worldDelta = surfaceRenderer.transform.TransformVector(
                currentOffset - previousOffset);

            return new Vector2(worldDelta.x, worldDelta.y);
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
                slot.Strength = 0f;
                ClearVisualWaveSlot(index);
                propertyChanged = true;
            }

            if (propertyChanged)
                surfaceRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void ClearVisualWaveSlot(int index)
        {
            _propertyBlock.SetVector(
                ImpactDataIds[index],
                new Vector4(0f, 0f, -1000f, 0f));
            _propertyBlock.SetVector(
                ImpactNormalIds[index],
                new Vector4(0f, 1f, 0f, 0f));
        }

        private void ClearAllVisualWaves()
        {
            surfaceRenderer.GetPropertyBlock(_propertyBlock);

            for (int index = 0; index < ShaderWaveSlotCount; index++)
            {
                _visualWaveSlots[index].Active = false;
                _visualWaveSlots[index].Strength = 0f;
                ClearVisualWaveSlot(index);
            }

            surfaceRenderer.SetPropertyBlock(_propertyBlock);
        }

        private Vector2 EvaluateLocalWaveOffset(Vector2 localPoint, float sampleTime)
        {
            Vector2 combinedOffset = Vector2.zero;

            foreach (VisualWaveSlot slot in _visualWaveSlots)
                combinedOffset += EvaluateLocalWaveOffset(slot, localPoint, sampleTime);

            return Vector2.ClampMagnitude(combinedOffset, maxCombinedVisualOffset);
        }

        private VisualWaveSlot FindClosestActiveWaveSlot(Vector2 localPoint, float sampleTime)
        {
            VisualWaveSlot closestSlot = null;
            float closestDistanceSquared = float.PositiveInfinity;

            foreach (VisualWaveSlot slot in _visualWaveSlots)
            {
                if (!slot.Active || slot.Strength <= 0.00001f || sampleTime < slot.StartTime)
                    continue;

                float elapsed = sampleTime - slot.StartTime;

                if (CalculateWaveEnvelope(elapsed) <= 0f)
                    continue;

                float distanceSquared = (localPoint - slot.LocalContactPoint).sqrMagnitude;

                if (distanceSquared >= closestDistanceSquared)
                    continue;

                closestSlot = slot;
                closestDistanceSquared = distanceSquared;
            }

            return closestSlot;
        }

        private Vector2 EvaluateLocalWaveOffset(VisualWaveSlot slot, Vector2 localPoint, float sampleTime)
        {
            if (!slot.Active || slot.Strength <= 0.00001f || sampleTime < slot.StartTime)
                return Vector2.zero;

            float elapsed = Mathf.Max(sampleTime - slot.StartTime, 0f);
            float waveEnvelope = CalculateWaveEnvelope(elapsed);

            if (waveEnvelope <= 0f)
                return Vector2.zero;

            float falloff = Mathf.Max(impactFalloff, 0.0001f);
            Vector2 impactNormal = slot.LocalNormal.normalized;
            Vector2 delta = localPoint - slot.LocalContactPoint;
            Vector2 tangent = new Vector2(-impactNormal.y, impactNormal.x);

            float surfaceDistance = Mathf.Abs(Vector2.Dot(delta, tangent));
            float depthDistance = Mathf.Abs(Vector2.Dot(delta, impactNormal));

            float spatialFade = Mathf.Exp(
                -surfaceDistance * surfaceDistance * falloff * falloff * 0.35f);
            float depthFade = Mathf.Exp(-depthDistance * (falloff + 1f));
            float timeFade = Mathf.Exp(
                -elapsed * Mathf.Max(impactDecay, 0.0001f));

            float phase = surfaceDistance * impactFrequency - elapsed * impactSpeed;
            float ripple = -Mathf.Cos(phase) * 0.8f;
            float dent = -0.3f * Mathf.Exp(
                -surfaceDistance * surfaceDistance * Mathf.Max(impactFalloff + 1f, 0.0001f)
                - elapsed * Mathf.Max(impactDecay + 2f, 0.0001f));

            float offset =
                (ripple * spatialFade * depthFade * timeFade + dent)
                * slot.Strength
                * visualWaveHeight
                * waveEnvelope;

            return impactNormal * offset;
        }

        private float CalculateWaveEnvelope(float elapsed)
        {
            float duration = Mathf.Max(visualWaveDuration, 0.01f);

            if (elapsed >= duration)
                return 0f;

            float attackDuration = Mathf.Max(Mathf.Min(duration * 0.1f, 0.1f), 0.0001f);
            float attackProgress = Mathf.Clamp01(elapsed / attackDuration);
            float attackFade = attackProgress * attackProgress * (3f - 2f * attackProgress);

            float fadeStart = duration * 0.75f;
            float fadeProgress = Mathf.Clamp01(
                (elapsed - fadeStart) / Mathf.Max(duration - fadeStart, 0.0001f));
            float smoothProgress = fadeProgress * fadeProgress * (3f - 2f * fadeProgress);

            return attackFade * (1f - smoothProgress);
        }

        private bool HasActiveWave()
        {
            foreach (VisualWaveSlot slot in _visualWaveSlots)
            {
                if (slot.Active)
                    return true;
            }

            return false;
        }

        private bool InitializeRuntimeWaveCollider()
        {
            if (surfaceTilemap == null || tilemapCollider == null)
                return false;

            _originalTilemapColliderEnabled = tilemapCollider.enabled;
            _originalCompositeOperation = tilemapCollider.compositeOperation;
            _sourceRigidbody = GetComponent<Rigidbody2D>();
            _sourceCompositeCollider = GetComponent<CompositeCollider2D>();

            if (_sourceCompositeCollider == null)
            {
                bool hadRigidbody = _sourceRigidbody != null;
                _sourceCompositeCollider = gameObject.AddComponent<CompositeCollider2D>();
                _createdCompositeCollider = true;
                _sourceRigidbody = GetComponent<Rigidbody2D>();
                _createdRigidbody = !hadRigidbody && _sourceRigidbody != null;
            }

            if (_sourceRigidbody == null)
            {
                DestroyRuntimeWaveCollider();
                return false;
            }

            if (_createdRigidbody)
                _sourceRigidbody.bodyType = RigidbodyType2D.Static;

            _originalCompositeColliderEnabled = _sourceCompositeCollider.enabled;
            _originalGeometryType = _sourceCompositeCollider.geometryType;
            _originalGenerationType = _sourceCompositeCollider.generationType;

            tilemapCollider.enabled = true;
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            _sourceCompositeCollider.enabled = true;
            _sourceCompositeCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;
            _sourceCompositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;

            if (tilemapCollider.hasTilemapChanges)
                tilemapCollider.ProcessTilemapChanges();

            _sourceCompositeCollider.GenerateGeometry();

            if (_sourceCompositeCollider.pathCount == 0)
            {
                DestroyRuntimeWaveCollider();
                return false;
            }

            _runtimeColliderRoot = new GameObject("Runtime Wave Collider");
            _runtimeColliderRoot.layer = gameObject.layer;
            _runtimeColliderRoot.transform.SetParent(transform, false);

            for (int pathIndex = 0; pathIndex < _sourceCompositeCollider.pathCount; pathIndex++)
            {
                int pointCount = _sourceCompositeCollider.GetPathPointCount(pathIndex);

                if (pointCount < 2)
                    continue;

                Vector2[] sourcePoints = new Vector2[pointCount];
                _sourceCompositeCollider.GetPath(pathIndex, sourcePoints);

                bool alreadyClosed = Vector2.SqrMagnitude(
                    sourcePoints[0] - sourcePoints[pointCount - 1]) < 0.000001f;
                int runtimePointCount = alreadyClosed ? pointCount : pointCount + 1;
                Vector2[] closedPoints = new Vector2[runtimePointCount];

                for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
                    closedPoints[pointIndex] = sourcePoints[pointIndex];

                if (!alreadyClosed)
                    closedPoints[runtimePointCount - 1] = sourcePoints[0];

                EdgeCollider2D edgeCollider = _runtimeColliderRoot.AddComponent<EdgeCollider2D>();
                edgeCollider.sharedMaterial = _sourceCompositeCollider.sharedMaterial;
                edgeCollider.isTrigger = _sourceCompositeCollider.isTrigger;
                edgeCollider.usedByEffector = _sourceCompositeCollider.usedByEffector;
                edgeCollider.edgeRadius = _sourceCompositeCollider.edgeRadius;
                edgeCollider.points = closedPoints;

                _colliderPaths.Add(new DeformableColliderPath
                {
                    Collider = edgeCollider,
                    OriginalPoints = closedPoints,
                    DeformedPoints = new Vector2[runtimePointCount]
                });
            }

            if (_colliderPaths.Count == 0)
            {
                DestroyRuntimeWaveCollider();
                return false;
            }

            tilemapCollider.enabled = false;
            _sourceCompositeCollider.enabled = false;

            return true;
        }

        private void UpdateRuntimeWaveCollider()
        {
            if (!animateTileCollider || _runtimeColliderRoot == null)
                return;

            bool hasActiveWave = HasActiveWave();

            if (!hasActiveWave && !_runtimeColliderDeformed)
                return;

            float sampleTime = Time.time;

            foreach (DeformableColliderPath path in _colliderPaths)
            {
                for (int index = 0; index < path.OriginalPoints.Length; index++)
                {
                    Vector2 originalPoint = path.OriginalPoints[index];
                    path.DeformedPoints[index] = hasActiveWave
                        ? originalPoint + EvaluateLocalWaveOffset(originalPoint, sampleTime)
                        : originalPoint;
                }

                path.Collider.points = path.DeformedPoints;
            }

            _runtimeColliderDeformed = hasActiveWave;
        }

        private void RestoreSourceColliders()
        {
            if (tilemapCollider != null)
            {
                tilemapCollider.compositeOperation = _originalCompositeOperation;
                tilemapCollider.enabled = _originalTilemapColliderEnabled;
            }

            if (_sourceCompositeCollider != null && !_createdCompositeCollider)
            {
                _sourceCompositeCollider.geometryType = _originalGeometryType;
                _sourceCompositeCollider.generationType = _originalGenerationType;
                _sourceCompositeCollider.enabled = _originalCompositeColliderEnabled;

                if (_sourceCompositeCollider.enabled)
                    _sourceCompositeCollider.GenerateGeometry();
            }
        }

        private void DestroyRuntimeWaveCollider()
        {
            RestoreSourceColliders();

            if (_runtimeColliderRoot != null)
                Destroy(_runtimeColliderRoot);

            if (_createdCompositeCollider && _sourceCompositeCollider != null)
                Destroy(_sourceCompositeCollider);

            if (_createdRigidbody && _sourceRigidbody != null)
                Destroy(_sourceRigidbody);

            _runtimeColliderRoot = null;
            _sourceCompositeCollider = null;
            _sourceRigidbody = null;
            _createdCompositeCollider = false;
            _createdRigidbody = false;
            _runtimeColliderDeformed = false;
            _colliderPaths.Clear();
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
            _propertyBlock.SetFloat(WaveDurationId, visualWaveDuration);
            surfaceRenderer.SetPropertyBlock(_propertyBlock);
        }

        private bool CanReact(int objectLayer)
        {
            int objectLayerMask = 1 << objectLayer;
            return (reactingLayers.value & objectLayerMask) != 0;
        }

        private void OnDisable()
        {
            DestroyRuntimeWaveCollider();

            if (surfaceRenderer != null && _propertyBlock != null)
                ClearAllVisualWaves();
        }
    }
}
