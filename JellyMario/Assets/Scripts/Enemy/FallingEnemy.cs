// 밟으면 떨어지는 땅
using JellyMario.Player;
using UnityEngine;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingEnemy : EnemyBase
    {
        [SerializeField, Min(0f)] private float fallSpeed = 3f;
        [SerializeField, Min(0f)] private float fallDelay = 0.2f;

        private Rigidbody2D _rigidbody;
        private Collider2D[] _colliders;
        private bool _isActivated;
        private float _fallTimer;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();
            _colliders = GetComponents<Collider2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (!_isActivated)
                return;

            if (_fallTimer < fallDelay)
            {
                _fallTimer += Time.fixedDeltaTime;
                return;
            }

            Vector2 nextPosition = _rigidbody.position + Vector2.down * (fallSpeed * Time.fixedDeltaTime);

            _rigidbody.MovePosition(nextPosition);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryActivateFromPlayer(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryActivateFromPlayer(collision);
        }

        private void TryActivateFromPlayer(Collision2D collision)
        {
            if (_isActivated || collision == null)
                return;

            PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();

            if (player == null || player.transform.position.y <= transform.position.y)
                return;

            float topY = GetTopY();

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.point.y < topY - 0.12f)
                    continue;

                _isActivated = true;
                _fallTimer = 0f;
                Move();

                return;
            }
        }

        private float GetTopY()
        {
            float topY = transform.position.y;

            foreach (Collider2D bodyCollider in _colliders)
            {
                if (bodyCollider == null || !bodyCollider.enabled || bodyCollider.isTrigger)
                    continue;

                topY = Mathf.Max(topY, bodyCollider.bounds.max.y);
            }

            return topY;
        }
    }
}