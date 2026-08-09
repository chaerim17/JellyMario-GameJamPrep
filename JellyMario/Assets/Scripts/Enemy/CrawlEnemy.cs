//달팽이
using JellyMario.Jelly;
using UnityEngine;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CrawlEnemy : EnemyBase
    {
        [SerializeField] private Transform[] movePoints;
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField, Min(0f)] private float moveRange = 5f;
        [SerializeField, Min(0.001f)] private float pointArrivalDistance = 0.1f;
        [SerializeField, Min(0f)] private float slopeRotationSpeed = 360f;
        [SerializeField, Min(0.1f)] private float groundProbeDistance = 1f;

        private Rigidbody2D _rigidbody;
        private Vector2[] _pathPositions;
        private float _startX;
        private int _direction = -1;
        private int _currentPointIndex;
        private int _pointStep = 1;
        private Vector2 _surfaceNormal = Vector2.up;
        private float _lastSurfaceContactTime = float.NegativeInfinity;

        private bool HasWaypointPath => _pathPositions != null && _pathPositions.Length > 0;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.angularVelocity = 0f;
            _startX = _rigidbody.position.x;
            _pathPositions = CacheWaypointLocalPositions(movePoints);
            _currentPointIndex = 0;

            UpdateFacingDirection();
            Move();
        }

        private void FixedUpdate()
        {
            if (HasWaypointPath)
                UpdateWaypointDirection();
            else
            {
                float distance = _rigidbody.position.x - _startX;

                if (moveRange > 0f && ((_direction < 0 && distance <= -moveRange) || (_direction > 0 && distance >= moveRange)))
                {
                    _direction *= -1;
                    UpdateFacingDirection();
                }
            }

            bool isOnSurface = TryGetSurfaceNormal(out Vector2 detectedNormal);

            if (isOnSurface)
            {
                _surfaceNormal = detectedNormal;
                _lastSurfaceContactTime = Time.time;
            }
            else
            {
                isOnSurface = Time.time - _lastSurfaceContactTime
                    <= Time.fixedDeltaTime * 2.5f;
            }

            Vector2 velocity = _rigidbody.linearVelocity;
            float targetRotation = 0f;

            if (isOnSurface)
            {
                Vector2 surfaceTangent = new Vector2(_surfaceNormal.y, -_surfaceNormal.x).normalized;

                velocity = surfaceTangent * (_direction * moveSpeed);
                targetRotation = Mathf.Atan2(_surfaceNormal.y, _surfaceNormal.x) * Mathf.Rad2Deg - 90f;
            }
            else
            {
                velocity.x = _direction * moveSpeed;
            }

            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = 0f;
            float nextRotation = Mathf.MoveTowardsAngle(_rigidbody.rotation, targetRotation, slopeRotationSpeed * Time.fixedDeltaTime);

            _rigidbody.MoveRotation(nextRotation);
        }

        private void UpdateWaypointDirection()
        {
            Vector2 target = WaypointLocalToWorld(_pathPositions[_currentPointIndex]);
            float deltaX = target.x - _rigidbody.position.x;

            if (Mathf.Abs(deltaX) <= pointArrivalDistance)
            {
                SelectNextPoint();
                target = WaypointLocalToWorld(_pathPositions[_currentPointIndex]);
                deltaX = target.x - _rigidbody.position.x;
            }

            int nextDirection = deltaX < 0f ? -1 : 1;

            if (nextDirection == _direction)
                return;

            _direction = nextDirection;
            UpdateFacingDirection();
        }

        private void SelectNextPoint()
        {
            if (_pathPositions.Length <= 1)
                return;

            _currentPointIndex = (_currentPointIndex + _pointStep + _pathPositions.Length) % _pathPositions.Length;
        }

        private bool TryGetSurfaceNormal(out Vector2 surfaceNormal)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(_rigidbody.position, Vector2.down, groundProbeDistance);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                    continue;

                JellySurfaceWave surface = hit.collider.GetComponentInParent<JellySurfaceWave>();

                if (surface == null || hit.normal.y <= 0.1f)
                    continue;

                surfaceNormal = hit.normal.normalized;
                return true;
            }

            surfaceNormal = Vector2.up;
            return false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            IgnoreOtherEnemyCollision(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (IgnoreOtherEnemyCollision(collision))
                return;

            JellySurfaceWave surface = collision.gameObject.GetComponentInParent<JellySurfaceWave>();

            if (surface == null)
                return;

            float bestUpwardNormal = 0.1f;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y <= bestUpwardNormal)
                    continue;

                bestUpwardNormal = contact.normal.y;
                _surfaceNormal = contact.normal.normalized;
                _lastSurfaceContactTime = Time.time;
            }
        }

        private bool IgnoreOtherEnemyCollision(Collision2D collision)
        {
            EnemyBase otherEnemy = collision.gameObject.GetComponentInParent<EnemyBase>();

            if (otherEnemy == null || otherEnemy == this)
                return false;

            Collider2D[] ownColliders = GetComponentsInChildren<Collider2D>();
            Collider2D[] otherColliders = otherEnemy.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D ownCollider in ownColliders)
            {
                foreach (Collider2D otherCollider in otherColliders)
                    Physics2D.IgnoreCollision(ownCollider, otherCollider, true);
            }

            return true;
        }

        private void UpdateFacingDirection()
        {
            if (CachedSpriteRenderer != null)
                CachedSpriteRenderer.flipX = _direction > 0;
        }
    }
}