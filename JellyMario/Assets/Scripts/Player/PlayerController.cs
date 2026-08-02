using JellyMario.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JellyMario.Player
{
    // 플레이어 기본 컴포넌트
    [RequireComponent(
        typeof(Rigidbody2D),
        typeof(CapsuleCollider2D)
    )]
    // 플레이어를 제어하는 기본 클래스
    public class PlayerController : PlayerBase
    {
        [Header("Move 설정")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Jump 설정")]
        [SerializeField] private float jumpPower = 10f;
        [SerializeField] private float jumpDuration = 0.5f;

        private Rigidbody2D _rigidbody;
        private bool _isGrounded;
        private Vector2 _moveInput;

        // 플레이어 초기화
        protected override void Initialize()
        {
            base.Initialize();

            _rigidbody = GetComponent<Rigidbody2D>();

            if (ManagersHub.Player != null)
                ManagersHub.Player.RegisterPlayer(this);

            _isGrounded = true;
        }

        // 플레이어 입력 처리
        protected override void HandleInput()
        {
            if (ManagersHub.Input == null)
            {
                _moveInput = Vector2.zero;

                return;
            }

            _moveInput = ManagersHub.Input.GetMoveInput();

            if (_isGrounded && ManagersHub.Input.GetJumpInput())
                Jump();
        }

        // 플레이어 이동 처리
        protected override void HandleMovement()
        {
            if (!_isGrounded)
                return;
            else if (Mathf.Abs(_moveInput.x) <= 0.01f)
            {
                Idle();
                return;
            }
            else
                Move();
        }

        public override void Idle()
        {
            base.Idle();
        }

        // 이동
        public override void Move()
        {
            base.Move();

            _rigidbody.linearVelocity = new Vector2(_moveInput.x * moveSpeed, _rigidbody.linearVelocity.y);
        }

        // 점프
        public override void Jump()
        {
            base.Jump();

            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, jumpPower);
        }

        // 충돌 처리
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Ground"))
                return;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 플레이어의 발밑에서 발생한 충돌인지 검사
                if (contact.normal.y > 0.5f)
                {
                    _isGrounded = true;

                    if (Mathf.Abs(_moveInput.x) <= 0.01f)
                        Idle();
                    else
                        Move();

                    break;
                }
            }
        }

        // 트리거 처리
        private void OnTriggerEnter2D(Collider2D other)
        {
            int layer = other.gameObject.layer;

            if (layer == LayerMask.NameToLayer("Hazard") ||
                layer == LayerMask.NameToLayer("DeathZone"))
            {
                Die();
                return;
            }

            if (layer == LayerMask.NameToLayer("Goal Flag"))
            {
                StageClear();
            }
        }
        public override void Die()
        {
            base.Die();

            Debug.Log("Player Dead");

            // TODO : 사망 처리
            // 현재 씬 다시 시작
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void StageClear()
        {
            Debug.Log("Stage Clear");

            // TODO : 클리어 처리
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            int nextScene = currentScene + 1;

            // 다음 씬이 존재하면 이동
            if (nextScene < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextScene);
            }
        }
    }
}