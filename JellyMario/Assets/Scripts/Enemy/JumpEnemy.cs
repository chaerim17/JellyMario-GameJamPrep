//개구리
using UnityEngine;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpEnemy : EnemyBase
    {
        [SerializeField] private float jumpPower = 11f;
        [SerializeField] private float jumpDelay = 2f;

        [SerializeField] private Transform groundCheck;
        [SerializeField] private float checkRadius = 0.1f;

        [SerializeField] private float movePower = 3f;
        [SerializeField] private int direction = 1;

        private Rigidbody2D _rigidbody;
        private float _timer;
        private SpriteRenderer _spriteRenderer;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = direction < 0;
            }
        }

        protected override void Update()
        {
            base.Update();

            _timer += Time.deltaTime;

            if (_timer >= jumpDelay && IsGrounded())
            {
                _timer = 0f;

                _rigidbody.linearVelocity =
                    new Vector2(movePower * direction, jumpPower);
            }

            if (IsGrounded())
                Idle();
            else
                Move();
        }

        private bool IsGrounded()
        {
            return Physics2D.OverlapCircle(
                groundCheck.position,
                checkRadius,
                LayerMask.GetMask("Default"));
        }
    }
}