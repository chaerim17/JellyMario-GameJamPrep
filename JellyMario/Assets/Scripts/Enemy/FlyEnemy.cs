//벌
using UnityEngine;

namespace JellyMario.Enemy
{
    public class FlyEnemy : EnemyBase
    {
        [SerializeField] private float moveSpeed = 2f;

        // 1 = 왼쪽, -1 = 오른쪽
        [SerializeField] private int direction = 1;

        private SpriteRenderer _spriteRenderer;

        protected override void Awake()
        {
            base.Awake();

            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = direction < 0;
            }

            Move();
        }

        protected override void HandleMovement()
        {
            transform.Translate(
                Vector2.left *
                (direction * moveSpeed * Time.deltaTime));
        }
    }
}