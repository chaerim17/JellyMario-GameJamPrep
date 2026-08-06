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

        [SerializeField] private float movePower = -3f;

        private Rigidbody2D _rigidbody;
        private float _timer;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();
        }

        protected override void Update()
        {
            base.Update();

            _timer += Time.deltaTime;

            if (_timer >= jumpDelay && IsGrounded())
            {
                _timer = 0f;

                _rigidbody.linearVelocity =
                    new Vector2(movePower, jumpPower);
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