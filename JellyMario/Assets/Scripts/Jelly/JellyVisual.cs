using UnityEngine;

namespace JellyMario.Jelly
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class JellyVisual : MonoBehaviour
    {
        [Header("����")]
        [Tooltip("������ ������ �ڽ� ������Ʈ")]
        [SerializeField] private Transform visual;

        [Header("������ ����")]
        [SerializeField, Min(0f)] private float stiffness = 100f;
        [SerializeField, Min(0f)] private float damping = 12f;

        [Header("���� ����")]
        [SerializeField, Range(0f, 0.5f)]
        private float maxDeformation = 0.25f;
        [SerializeField, Min(0f)]
        private float impactResponse = 0.025f;
        private Vector3 _originalScale;

        // ���� ���� ũ�⿡�� �󸶳� �����ƴ��� ��Ÿ����.
        private Vector2 _deformation;
        // ������ ��� ���Ǵ� �ӵ���.
        private Vector2 _deformationVelocity;

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
                Debug.LogError("JellyVisual�� ������ Visual�� ��ϵ��� �ʾҽ��ϴ�.", this);

                enabled = false;
                return;
            }

            if (visual == transform)
            {
                Debug.LogError("Player ��Ʈ�� �ƴ� �ڽ� Visual�� ����ؾ� �մϴ�.", this);

                enabled = false;
                return;
            }

            _originalScale = visual.localScale;
        }

        // ������ ���¸� ���� ũ��� �����Ѵ�.
        private void UpdateSpring()
        {
            float deltaTime = Time.deltaTime;

            Vector2 acceleration = -_deformation * stiffness - _deformationVelocity * damping;

            _deformationVelocity += acceleration * deltaTime;
            _deformation += _deformationVelocity * deltaTime;

            _deformation.x = Mathf.Clamp(_deformation.x, -maxDeformation, maxDeformation);
            _deformation.y = Mathf.Clamp(_deformation.y, -maxDeformation, maxDeformation);
        }

        // ���� �������� �ڽ� Visual�� �����Ѵ�.
        private void ApplyDeformation()
        {
            float scaleX = Mathf.Max(0.5f, 1f + _deformation.x);
            float scaleY = Mathf.Max(0.5f, 1f + _deformation.y);

            visual.localScale = new Vector3(_originalScale.x * scaleX, _originalScale.y * scaleY, _originalScale.z);
        }

        // ������ �� ���η� �ø���.
        public void Stretch(float amount)
        {
            amount = ClampAmount(amount);

            AddDeformation(new Vector2(-amount * 0.5f, amount));
        }

        // ������ �� ���η� ������ ���η� ������.
        public void Squash(float amount)
        {
            amount = ClampAmount(amount);

            AddDeformation(new Vector2(amount, -amount));
        }

        // ������Ʈ�� ��Ȱ��ȭ�Ǹ� ���� ������� ������.
        private void AddDeformation(Vector2 amount)
        {
            _deformation += amount;

            // ������ �� �� �� �ⷷ�̵��� �ӵ��� �߰��Ѵ�.
            _deformationVelocity += amount * 3f;
        }

        private float ClampAmount(float amount)
        {
            return Mathf.Clamp(Mathf.Abs(amount), 0f, maxDeformation);
        }

        // ������Ʈ�� ��Ȱ��ȭ�Ǹ� ���� ������� ������.
        private void OnDisable()
        {
            if (visual != null)
                visual.localScale = _originalScale;

            _deformation = Vector2.zero;
            _deformationVelocity = Vector2.zero;
        }
    }
}