//튀어나오는 장애물
using UnityEngine;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpawnEnemy : EnemyBase
    {
        [SerializeField, Min(0f)] private float riseHeight = 1f;
        [SerializeField, Min(0f)] private float moveSpeed = 1f;
        [SerializeField, Min(0f)] private float waitTime = 1.5f;

        private Rigidbody2D _rigidbody;
        private Vector2 _hiddenPosition;
        private Vector2 _shownPosition;
        private float _waitTimer;
        private bool _movingUp = true;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.gravityScale = 0f;

            _hiddenPosition = _rigidbody.position;
            _shownPosition = _hiddenPosition + Vector2.up * riseHeight;
            _waitTimer = waitTime;
        }

        private void FixedUpdate()
        {
            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.fixedDeltaTime;
                _rigidbody.linearVelocity = Vector2.zero;

                return;
            }

            Vector2 target = _movingUp ? _shownPosition : _hiddenPosition;
            Vector2 nextPosition = Vector2.MoveTowards(_rigidbody.position, target, moveSpeed * Time.fixedDeltaTime);

            _rigidbody.MovePosition(nextPosition);

            if ((nextPosition - target).sqrMagnitude > 0.0001f)
                return;

            _movingUp = !_movingUp;
            _waitTimer = waitTime;
        }
    }
}