using UnityEngine;

namespace JellyMario.Jelly
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class JellySurfaceFollower2D : MonoBehaviour
    {
        [Header("바닥 판정")]
        [Tooltip("접촉 법선의 Y값이 이 값보다 클 때 파동 바닥으로 판단합니다.")]
        [SerializeField, Range(-1f, 1f)]
        private float minimumGroundNormalY = 0.5f;

        [Header("이동 설정")]
        [Tooltip("파동과 충돌체 사이에서 튀지 않도록 바닥 법선 방향의 상대 속도를 제거합니다.")]
        [SerializeField]
        private bool cancelGroundNormalVelocity = true;

        private Rigidbody2D _rigidbody;
        private JellySurfaceWave _surfaceWave;
        private Vector2 _contactPoint;
        private Vector2 _contactNormal = Vector2.up;
        private bool _followingEnabled = true;

        public bool IsFollowingSurface =>
            _followingEnabled && _surfaceWave != null;

        public JellySurfaceWave CurrentSurface => _surfaceWave;

        public bool IsSurfaceCollision(Collision2D collision)
        {
            return FindSurfaceWave(collision) != null;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!IsFollowingSurface)
                return;

            Vector2 surfaceDelta =
                _surfaceWave.GetSurfaceDeltaAtWorldPoint(_contactPoint);

            _rigidbody.position += surfaceDelta;

            if (cancelGroundNormalVelocity)
                RemoveGroundNormalVelocity();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            UpdateSurfaceContact(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            UpdateSurfaceContact(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            JellySurfaceWave surfaceWave = FindSurfaceWave(collision);

            if (surfaceWave == _surfaceWave)
                ClearSurface();
        }

        public void SetFollowingEnabled(bool enabled)
        {
            _followingEnabled = enabled;

            if (!enabled)
                ClearSurface();
        }

        public void ClearSurface()
        {
            _surfaceWave = null;
            _contactPoint = Vector2.zero;
            _contactNormal = Vector2.up;
        }

        private void UpdateSurfaceContact(Collision2D collision)
        {
            if (!_followingEnabled)
                return;

            JellySurfaceWave surfaceWave = FindSurfaceWave(collision);

            if (surfaceWave == null)
                return;

            bool foundGroundContact = false;
            ContactPoint2D bestContact = default;
            float bestNormalY = minimumGroundNormalY;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y <= bestNormalY)
                    continue;

                bestContact = contact;
                bestNormalY = contact.normal.y;
                foundGroundContact = true;
            }

            if (!foundGroundContact)
                return;

            _surfaceWave = surfaceWave;
            _contactPoint = bestContact.point;
            _contactNormal = bestContact.normal.normalized;
        }

        private static JellySurfaceWave FindSurfaceWave(Collision2D collision)
        {
            if (collision == null)
                return null;

            return collision.gameObject.GetComponentInParent<JellySurfaceWave>();
        }

        private void RemoveGroundNormalVelocity()
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            float normalSpeed = Vector2.Dot(velocity, _contactNormal);

            velocity -= _contactNormal * normalSpeed;
            _rigidbody.linearVelocity = velocity;
        }

        private void OnDisable()
        {
            ClearSurface();
        }
    }
}
