using UnityEngine;

namespace JellyMario.Jelly
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class JellyVisual : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("충돌 각도로 회전하며 변형을 적용할 축입니다. 비어 있으면 자동으로 생성합니다.")]
        [SerializeField] private Transform deformationPivot;

        [Tooltip("실제로 변형할 SpriteRenderer가 들어 있는 Transform")]
        [SerializeField] private Transform visual;

        [Header("스프링 설정")]
        [SerializeField, Min(0f)] private float stiffness = 100f;
        [SerializeField, Min(0f)] private float damping = 12f;

        [Header("변형 설정")]
        [SerializeField, Range(0f, 0.5f)]
        private float maxDeformation = 0.25f;

        [SerializeField, Min(0f)]
        private float impactResponse = 0.025f;

        [Tooltip("압축 뒤 반동으로 늘어날 때의 크기입니다.")]
        [SerializeField, Range(1f, 3f)]
        private float stretchMultiplier = 1.5f;

        [Tooltip("눌릴 때 양옆으로 부풀리는 정도입니다.")]
        [SerializeField, Range(0f, 2f)]
        private float sideExpansion = 1.15f;

        [Header("충돌면 고정")]
        [Tooltip("캐릭터 비주얼 크기의 절반입니다.")]
        [SerializeField] private Vector2 visualHalfSize = new Vector2(0.5f, 0.5f);

        [SerializeField, Range(0f, 1f)]
        private float anchorStrength = 1f;

        // 양수면 충돌 방향으로 눌리고, 음수면 그 방향으로 늘어난다.
        private float _deformation;
        private float _deformationVelocity;
        private Vector2 _localImpactNormal = Vector2.up;

        private Vector3 _originalPivotPosition;
        private Vector3 _originalPivotScale;
        private Quaternion _originalPivotRotation;
        private Quaternion _originalVisualRotation;
        private Quaternion _originalCombinedRotation;

        private void Awake()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            UpdateSpring();
            ApplyDeformation();
        }

        private void Initialize()
        {
            if (visual == null)
            {
                Debug.LogError(
                    "JellyVisual에 변형할 Visual이 등록되지 않았습니다.",
                    this
                );

                enabled = false;
                return;
            }

            CreateDeformationPivotIfNeeded();

            if (deformationPivot == null || visual.parent != deformationPivot)
            {
                Debug.LogError(
                    "Visual은 DeformationPivot의 자식이어야 합니다.",
                    this
                );

                enabled = false;
                return;
            }

            _originalPivotPosition = deformationPivot.localPosition;
            _originalPivotScale = deformationPivot.localScale;
            _originalPivotRotation = deformationPivot.localRotation;
            _originalVisualRotation = visual.localRotation;
            _originalCombinedRotation =
                _originalPivotRotation * _originalVisualRotation;
        }

        private void CreateDeformationPivotIfNeeded()
        {
            if (deformationPivot != null)
                return;

            Transform originalParent = visual.parent;

            if (originalParent == null)
            {
                Debug.LogError(
                    "Visual에는 부모 Transform이 필요합니다.",
                    this
                );

                return;
            }

            GameObject pivotObject = new GameObject("DeformationPivot");
            deformationPivot = pivotObject.transform;
            deformationPivot.SetParent(originalParent, false);

            // Pivot을 Visual의 중심에 놓은 뒤 Visual의 월드 모습을 유지한다.
            deformationPivot.localPosition = visual.localPosition;
            deformationPivot.localRotation = Quaternion.identity;
            deformationPivot.localScale = Vector3.one;
            deformationPivot.SetSiblingIndex(visual.GetSiblingIndex());

            visual.SetParent(deformationPivot, true);
        }

        private void UpdateSpring()
        {
            float acceleration =
                -_deformation * stiffness
                - _deformationVelocity * damping;

            _deformationVelocity += acceleration * Time.deltaTime;
            _deformation += _deformationVelocity * Time.deltaTime;

            _deformation = Mathf.Clamp(
                _deformation,
                -maxDeformation,
                maxDeformation
            );
        }

        private void ApplyDeformation()
        {
            if (deformationPivot == null || visual == null)
                return;

            if (_localImpactNormal.sqrMagnitude < 0.001f)
                _localImpactNormal = Vector2.up;

            // Pivot의 로컬 Y축을 충돌 법선과 일치시킨다.
            float impactAngle =
                Mathf.Atan2(_localImpactNormal.y, _localImpactNormal.x)
                * Mathf.Rad2Deg
                - 90f;

            deformationPivot.localRotation =
                Quaternion.Euler(0f, 0f, impactAngle);

            // 압축 뒤 스프링이 반대쪽으로 넘어가면 늘어나는 반동을 더 크게 보여준다.
            float displayedDeformation = _deformation < 0f
                ? _deformation * stretchMultiplier
                : _deformation;

            // 충돌 축으로는 눌리고 그와 수직인 축으로는 부풀린다.
            float tangentScale = Mathf.Max(
                0.5f,
                1f + displayedDeformation * sideExpansion
            );

            float normalScale = Mathf.Max(
                0.5f,
                1f - displayedDeformation
            );

            deformationPivot.localScale = new Vector3(
                _originalPivotScale.x * tangentScale,
                _originalPivotScale.y * normalScale,
                _originalPivotScale.z
            );

            // 변형축만 회전하고 스프라이트의 보이는 회전은 유지한다.
            visual.localRotation =
                Quaternion.Inverse(deformationPivot.localRotation)
                * _originalCombinedRotation;

            float projectedHalfSize =
                Mathf.Abs(_localImpactNormal.x) * visualHalfSize.x
                + Mathf.Abs(_localImpactNormal.y) * visualHalfSize.y;

            float compression = Mathf.Max(0f, _deformation);

            // 닿은 면 쪽으로 중심을 이동시켜 충돌 지점을 고정한다.
            Vector2 anchorOffset =
                -_localImpactNormal
                * compression
                * projectedHalfSize
                * anchorStrength;

            deformationPivot.localPosition =
                _originalPivotPosition + (Vector3)anchorOffset;
        }

        public void ReactToImpact(Vector2 worldNormal, float force)
        {
            if (deformationPivot == null)
                return;

            Transform deformationSpace = deformationPivot.parent;
            Vector3 localNormal3D = deformationSpace != null
                ? deformationSpace.InverseTransformDirection(worldNormal)
                : worldNormal;

            _localImpactNormal = new Vector2(
                localNormal3D.x,
                localNormal3D.y
            ).normalized;

            float amount = Mathf.Clamp(
                Mathf.Abs(force) * impactResponse,
                0f,
                maxDeformation
            );

            AddDeformation(amount);
        }

        // 캐릭터의 머리 방향으로 늘어난다.
        public void Stretch(float amount)
        {
            _localImpactNormal = Vector2.up;
            AddDeformation(-ClampAmount(amount));
        }

        // 캐릭터의 머리-발 축으로 눌린다.
        public void Squash(float amount)
        {
            _localImpactNormal = Vector2.up;
            AddDeformation(ClampAmount(amount));
        }

        private void AddDeformation(float amount)
        {
            _deformation += amount;
            _deformationVelocity += amount * 3f;
        }

        private float ClampAmount(float amount)
        {
            return Mathf.Clamp(
                Mathf.Abs(amount),
                0f,
                maxDeformation
            );
        }

        private void OnDisable()
        {
            if (deformationPivot != null)
            {
                deformationPivot.localPosition = _originalPivotPosition;
                deformationPivot.localScale = _originalPivotScale;
                deformationPivot.localRotation = _originalPivotRotation;
            }

            if (visual != null)
                visual.localRotation = _originalVisualRotation;

            _deformation = 0f;
            _deformationVelocity = 0f;
        }
    }
}
