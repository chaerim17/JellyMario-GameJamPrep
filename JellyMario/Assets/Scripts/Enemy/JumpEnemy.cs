//개구리
using JellyMario.Jelly;
using UnityEngine;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(JellySurfaceFollower2D))]
    public class JumpEnemy : EnemyBase
    {
        [SerializeField] private float jumpPower = 11f;
        [SerializeField] private float jumpDelay = 0f;

        [Tooltip("Parent-local X positions used as jump patrol points.")]
        [SerializeField] private float[] movePointXs;
        [SerializeField, Min(0.01f)] private float pointArrivalDistance = 0.35f;

        [SerializeField] private float movePower = 3f;
        [SerializeField] private int direction = 1;

        private Rigidbody2D _rigidbody;
        private JellySurfaceFollower2D _surfaceFollower;
        private int _currentPointIndex;
        private int _pointStep;
        private int _jumpTargetIndex = -1;
        private float _jumpStartTargetDeltaX;
        private float _groundedTimer;
        private float _airGravityScale;
        private bool _wasGrounded;
        private bool _isAirborne = true;
        private readonly ContactPoint2D[] _contactBuffer = new ContactPoint2D[8];

        private bool HasWaypointPath => movePointXs != null && movePointXs.Length > 0;

        protected override void Awake()
        {
            base.Awake();

            CacheComponents();
            _airGravityScale = Mathf.Max(_rigidbody.gravityScale, 0f);

            _pointStep = direction < 0 ? -1 : 1;
            _currentPointIndex = _pointStep > 0
                ? 0 : Mathf.Max((movePointXs?.Length ?? 0) - 1, 0);

            UpdateFacingDirection(GetHorizontalJumpPower());
        }

        protected override void Update()
        {
            base.Update();
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null || _surfaceFollower == null)
                CacheComponents();

            if (_rigidbody == null || _surfaceFollower == null)
                return;

            RecoverLandingFromCurrentContacts();

            // 파동으로 움직이는 EdgeCollider는 접촉이 순간적으로 끊길 수 있다.
            // 실제 착지 이후에는 다음 점프 전까지 착지 상태를 유지한다.
            bool isGrounded = !_isAirborne;

            if (isGrounded)
            {
                _rigidbody.linearVelocity = Vector2.zero;

                if (!_wasGrounded)
                {
                    _groundedTimer = 0f;
                    UpdateWaypointAfterLanding();
                }

                _groundedTimer += Time.fixedDeltaTime;

                if (_groundedTimer >= jumpDelay)
                {
                    _groundedTimer = 0f;
                    _wasGrounded = false;

                    UpdateJumpDirection();
                    float horizontalJumpPower = GetHorizontalJumpPower();

                    _isAirborne = true;
                    _surfaceFollower?.SetFollowingEnabled(false);
                    _rigidbody.gravityScale = _airGravityScale;
                    _rigidbody.linearVelocity = new Vector2(horizontalJumpPower, jumpPower);

                    UpdateFacingDirection(horizontalJumpPower);

                    Move();
                    return;
                }

                Idle();
            }
            else
            {
                Move();
            }

            _wasGrounded = isGrounded;
        }

        private void UpdateJumpDirection()
        {
            if (!HasWaypointPath)
                return;

            float targetX = GetWaypointWorldX(_currentPointIndex);
            float deltaX = targetX - _rigidbody.position.x;

            if (Mathf.Abs(deltaX) <= pointArrivalDistance)
            {
                SelectNextPoint();
                targetX = GetWaypointWorldX(_currentPointIndex);
                deltaX = targetX - _rigidbody.position.x;
            }

            direction = deltaX < 0f ? -1 : 1;
            _jumpTargetIndex = _currentPointIndex;
            _jumpStartTargetDeltaX = deltaX;

        }

        private void UpdateWaypointAfterLanding()
        {
            if (!HasWaypointPath || _jumpTargetIndex != _currentPointIndex)
                return;

            float targetX = GetWaypointWorldX(_currentPointIndex);
            float currentDeltaX = targetX - _rigidbody.position.x;
            bool reachedTarget = Mathf.Abs(currentDeltaX) <= pointArrivalDistance;
            bool crossedTarget = 
                Mathf.Abs(_jumpStartTargetDeltaX) > pointArrivalDistance &&
                Mathf.Sign(currentDeltaX) != Mathf.Sign(_jumpStartTargetDeltaX);

            if (reachedTarget || crossedTarget)
                SelectNextPoint();

            _jumpTargetIndex = -1;
        }

        private void SelectNextPoint()
        {
            if (movePointXs.Length <= 1)
                return;

            _currentPointIndex = (_currentPointIndex + _pointStep + movePointXs.Length) % movePointXs.Length;
        }

        private float GetWaypointWorldX(int index)
        {
            float localX = movePointXs[index];

            if (transform.parent == null)
                return localX;

            Vector3 localPosition = transform.localPosition;
            localPosition.x = localX;

            return transform.parent.TransformPoint(localPosition).x;
        }

        private float GetHorizontalJumpPower()
        {
            return HasWaypointPath
                ? Mathf.Abs(movePower) * direction : movePower * direction;
        }

        private void UpdateFacingDirection(float horizontalVelocity)
        {
            if (CachedSpriteRenderer != null && Mathf.Abs(horizontalVelocity) > 0.001f)
                CachedSpriteRenderer.flipX = horizontalVelocity > 0f;
        }

        private void CacheComponents()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _surfaceFollower = GetComponent<JellySurfaceFollower2D>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            RegisterGroundContact(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            RegisterGroundContact(collision);
        }

        private void RegisterGroundContact(Collision2D collision)
        {
            if (_surfaceFollower == null || !_surfaceFollower.IsSurfaceCollision(collision))
                return;

            // 상승 중에는 표면 추적을 다시 켜지 않는다. Rigidbody의
            // 점프 속도와 Gravity만 사용하고, 하강 중 착지할 때 다시 붙는다.
            if (_isAirborne && _rigidbody.linearVelocity.y > 0f)
                return;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y <= 0.5f)
                    continue;

                if (_surfaceFollower.TryStartFollowing(collision))
                    CompleteLanding();

                return;
            }
        }

        private void RecoverLandingFromCurrentContacts()
        {
            if (!_isAirborne || _rigidbody.linearVelocity.y > 0f)
                return;

            int contactCount = _rigidbody.GetContacts(_contactBuffer);

            for (int i = 0; i < contactCount; i++)
            {
                ContactPoint2D contact = _contactBuffer[i];

                if (contact.normal.y <= 0.5f)
                    continue;

                bool startedFollowing = 
                    _surfaceFollower.TryStartFollowing(contact.collider, contact.point, contact.normal) ||
                    _surfaceFollower.TryStartFollowing(contact.otherCollider, contact.point, contact.normal);

                if (!startedFollowing)
                    continue;

                CompleteLanding();
                return;
            }
        }

        private void CompleteLanding()
        {
            _isAirborne = false;
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        protected override void OnDisable()
        {
            _isAirborne = true;

            if (_rigidbody != null)
                _rigidbody.gravityScale = _airGravityScale;

            base.OnDisable();
        }
    }
}