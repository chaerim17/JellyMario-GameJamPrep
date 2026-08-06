//보스 패턴 슬라임
using UnityEngine;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossSlimeEnemy : EnemyBase
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int direction = -1;

        //발사 정도 조절
        [SerializeField] private Vector2 launchForce = new Vector2(-3f, 6f);

        private Rigidbody2D _rigidbody;
        private bool _isGrounded;
        private SpriteRenderer _spriteRenderer;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();

            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = direction > 0;
            }
        }

        private void Start()
        {
            _rigidbody.linearVelocity =
                new Vector2(
                    Mathf.Abs(launchForce.x) * direction,
                    launchForce.y);
        }

        protected override void HandleMovement()
        {
            if (!_isGrounded)
                return;

            _rigidbody.linearVelocity =
                new Vector2(
                    moveSpeed * direction,
                    _rigidbody.linearVelocity.y);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                _isGrounded = true;
                Move();
            }
        }
    }
}