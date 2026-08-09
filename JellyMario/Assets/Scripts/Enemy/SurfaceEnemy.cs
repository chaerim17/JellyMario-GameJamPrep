//표면 굴러다니는 적
using System.Collections.Generic;
using JellyMario.Jelly;
using UnityEngine;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SurfaceEnemy : EnemyBase
    {
        [SerializeField] private Transform[] movePoints;
        [SerializeField, Min(0f)] private float moveSpeed = 3f;
        [SerializeField] private int direction = 1;
        [SerializeField, Min(0.001f)] private float arrivalDistance = 0.02f;

        private Rigidbody2D _rigidbody;
        private Vector2[] _pathPositions;
        private int _currentIndex;
        private int _indexStep;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;

            CachePathPositions();
            _indexStep = direction < 0 ? -1 : 1;
            _currentIndex = _indexStep > 0
                ? 0 : Mathf.Max(_pathPositions.Length - 1, 0);

            if (_pathPositions.Length == 0)
            {
                Debug.LogWarning("RollingEnemy에 이동 Point가 연결되어 있지 않습니다.", this);
                Idle();
                return;
            }

            if (HasArrived(_rigidbody.position, CurrentWorldTarget))
                SelectNextPoint();

            Move();
        }

        protected override void HandleMovement()
        {
        }

        private void FixedUpdate()
        {
            if (_pathPositions == null || _pathPositions.Length == 0)
                return;

            Vector2 targetPosition = CurrentWorldTarget;
            Vector2 nextPosition = Vector2.MoveTowards(_rigidbody.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

            _rigidbody.MovePosition(nextPosition);

            if (HasArrived(nextPosition, targetPosition))
                SelectNextPoint();
        }

        private Vector2 CurrentWorldTarget
        {
            get
            {
                Vector2 localTarget = _pathPositions[_currentIndex];

                if (transform.parent == null)
                    return localTarget;

                return transform.parent.TransformPoint(localTarget);
            }
        }

        private void SelectNextPoint()
        {
            if (_pathPositions.Length <= 1)
                return;

            _currentIndex = (_currentIndex + _indexStep + _pathPositions.Length) % _pathPositions.Length;
        }

        private bool HasArrived(Vector2 position, Vector2 target)
        {
            return (position - target).sqrMagnitude <= arrivalDistance * arrivalDistance;
        }

        private void CachePathPositions()
        {
            List<Transform> validPoints = new List<Transform>();

            if (movePoints != null)
            {
                foreach (Transform movePoint in movePoints)
                {
                    if (movePoint != null && !validPoints.Contains(movePoint))
                        validPoints.Add(movePoint);
                }
            }

            if (validPoints.Count == 0)
            {
                Transform[] children = GetComponentsInChildren<Transform>(true);

                foreach (Transform child in children)
                {
                    if (child == transform || !child.name.StartsWith("Point"))
                        continue;

                    validPoints.Add(child);
                }
            }

            _pathPositions = new Vector2[validPoints.Count];

            for (int index = 0; index < validPoints.Count; index++)
            {
                // Point와 RollingEnemy의 Inspector에 표시되는 로컬 좌표를 맞춘다.
                _pathPositions[index] = validPoints[index].localPosition;
            }
        }

        private void Start()
        {
            Collider2D[] ownColliders = GetComponentsInChildren<Collider2D>(true);
            JellySurfaceWave[] groundSurfaces = Object.FindObjectsByType<JellySurfaceWave>(FindObjectsInactive.Include);

            foreach (JellySurfaceWave groundSurface in groundSurfaces)
            {
                if (groundSurface == null)
                    continue;

                Collider2D[] groundColliders = groundSurface.GetComponentsInChildren<Collider2D>(true);

                foreach (Collider2D ownCollider in ownColliders)
                {
                    foreach (Collider2D groundCollider in groundColliders)
                    {
                        if (ownCollider == null || groundCollider == null)
                            continue;

                        Physics2D.IgnoreCollision(ownCollider, groundCollider, true);
                    }
                }
            }
        }
    }
}