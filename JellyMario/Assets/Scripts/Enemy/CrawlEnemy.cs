//달팽이
using UnityEngine;

namespace JellyMario.Enemy
{
    public class CrawlEnemy : EnemyBase
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 5f;

        private Vector3 _startPosition;
        private int _direction = -1;

        protected override void Awake()
        {
            base.Awake();

            _startPosition = transform.position;

            Move();
        }

        protected override void HandleMovement()
        {
            transform.Translate(
                Vector2.right * (_direction * moveSpeed * Time.deltaTime));

            float distance =
                transform.position.x - _startPosition.x;

            if (Mathf.Abs(distance) >= moveRange)
            {
                _direction *= -1;

                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
        }
    }
}