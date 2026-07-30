using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Player
{
    // 플레이어를 제어하는 기본 클래스
    public class PlayerController : PlayerBase
    {
        [Header("Move 설정")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Jump 설정")]
        [SerializeField] private float jumpPower = 10f;
        [SerializeField] private float jumpDuration = 0.5f;

        private bool _isJumping;
        private Vector2 _moveInput;

        protected override void Initialize()
        {
            base.Initialize();

            if (ManagersHub.Player != null)
                ManagersHub.Player.RegisterPlayer(this);
        }

        protected override void HandleInput()
        {
            if (ManagersHub.Input == null) {
                _moveInput = Vector2.zero;

                return;
            }

            _moveInput = ManagersHub.Input.GetMoveInput();

            if (!_isJumping && ManagersHub.Input.GetJumpInput())
                _isJumping = true;
        }

        protected override void HandleMovement()
        {
            if (_isJumping)
            {
                Jump();
                return;
            }
            else if (Mathf.Abs(_moveInput.x) <= 0.01f)
            {
                Idle();
                return;
            }
            else
                Move();
        }

        public override void Move()
        {
            base.Move();

            float distance = _moveInput.x * moveSpeed * Time.deltaTime;
            transform.position += Vector3.right * distance;
        }

        public override void Jump()
        {
            base.Jump();

            if (transform.position.y < 0.1f)
                transform.position += Vector3.up * jumpPower;
            else
            {
                transform.position -= Vector3.up * jumpPower;

                _isJumping = false;
            }
        }
    }
}