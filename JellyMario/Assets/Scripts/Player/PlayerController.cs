using JellyMario.Core;
using JellyMario.Effects;
using JellyMario.Jelly;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace JellyMario.Player
{
    // 플레이어 기본 컴포넌트
    [RequireComponent(
        typeof(Rigidbody2D),
        typeof(CapsuleCollider2D),
        typeof(JellySurfaceFollower2D)
    )]
    // 플레이어를 제어하는 기본 클래스
    public class PlayerController : PlayerBase
    {
        [Header("Move 설정")]
        [SerializeField] private float moveSpeed = 30f;
        [SerializeField] private float rotationAcceleration = 720f;

        [Header("Jump 설정")]
        [SerializeField] private float jumpPower = 5f;
        [SerializeField] private Transform jumpDirection;

        [Header("Jelly 설정")]
        [SerializeField] private JellyVisual jellyVisual;
        [SerializeField] private JellySurfaceFollower2D jellySurfaceFollower;
        [SerializeField] private float jumpStretch = 0.1f;

        [Header("Die 설정")]
        [SerializeField] private PixelShatterEffect pixelShatterEffect;
        [SerializeField, Min(0f)] private float deathDelay = 0.8f;

        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private Vector2 _moveInput;
        private bool _isDead;

        // 플레이어 초기화
        protected override void Initialize()
        {
            base.Initialize();

            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            if (jumpDirection == null)
                jumpDirection = transform;

            if (jellyVisual == null)
                jellyVisual = GetComponent<JellyVisual>();

            if (jellySurfaceFollower == null)
                jellySurfaceFollower = GetComponent<JellySurfaceFollower2D>();

            if (jellySurfaceFollower == null)
                jellySurfaceFollower = gameObject.AddComponent<JellySurfaceFollower2D>();

            if (pixelShatterEffect == null)
                pixelShatterEffect = GetComponent<PixelShatterEffect>();

            if (ManagersHub.Player != null)
                ManagersHub.Player.RegisterPlayer(this);
        }

        // 플레이어 입력 처리
        protected override void HandleInput()
        {
            if (_isDead)
                return;

            if (ManagersHub.Input == null)
            {
                _moveInput = Vector2.zero;

                return;
            }

            _moveInput = ManagersHub.Input.GetMoveInput();

            if (ManagersHub.Input.GetJumpInput())
                Jump();
        }

        // 플레이어 이동 처리
        protected override void HandleMovement()
        {
            if (_isDead)
                return;

            // Jump 애니메이션 중에도 회전 조작은 유지하되,
            // 애니메이션 상태는 Move로 즉시 바꾸지 않는다.
            if (CurrentState == PlayerState.Jump)
            {
                UpdateRotation();

                return;
            }

            if (Mathf.Abs(_moveInput.x) <= 0.01f)
            {
                UpdateRotation();
                Idle();
                return;
            }

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
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            float targetAngularVelocity = Mathf.Abs(_moveInput.x) <= 0.01f
                ? 0f
                : -_moveInput.x * moveSpeed;

            _rigidbody.angularVelocity = Mathf.MoveTowards(_rigidbody.angularVelocity, targetAngularVelocity, rotationAcceleration * Time.deltaTime);
        }

        protected override void OnAnimationFinished(PlayerState state)
        {
            base.OnAnimationFinished(state);

            if (state != PlayerState.Jump)
                return;

            if (Mathf.Abs(_moveInput.x) <= 0.01f)
                Idle();
            else
                Move();
        }

        // 점프
        public override void Jump()
        {
            if (_isDead)
                return;

            base.Jump();

            jellySurfaceFollower?.SetFollowingEnabled(false);

            Vector2 direction = jumpDirection.up.normalized;
            _rigidbody.linearVelocity = direction * jumpPower;

            jellyVisual?.Stretch(jumpStretch);

            ManagersHub.Sound?.PlayJumpSFX();
        }

        // 충돌 처리
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isDead)
                return;

            if (IsDeathLayer(collision.gameObject))
            {
                Die();
                return;
            }

            if (collision.contactCount == 0)
                return;

            ContactPoint2D strongestContact = collision.GetContact(0);
            float strongestImpactSpeed = 0f;

            // 접촉점이 여러 개라면 충격이 가장 강한 지점을 찾는다.
            foreach (ContactPoint2D contact in collision.contacts)
            {
                float impactSpeed = Mathf.Abs(Vector2.Dot(collision.relativeVelocity, contact.normal));

                if (impactSpeed > strongestImpactSpeed)
                {
                    strongestImpactSpeed = impactSpeed;
                    strongestContact = contact;
                }
            }

            // 벽, 바닥, 천장 등 모든 충돌에 젤리 반응
            jellyVisual?.ReactToImpact(strongestContact.normal, strongestImpactSpeed);

            bool isWaveSurface =
                jellySurfaceFollower?.IsSurfaceCollision(collision) == true;
            bool isGroundLayer =
                collision.gameObject.layer == LayerMask.NameToLayer("Ground");

            // Ground 레이어이거나 파동 표면인 경우 착지로 처리한다.
            if (!isGroundLayer && !isWaveSurface)
                return;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 플레이어 아래쪽에서 발생한 충돌
                if (contact.normal.y > 0.5f)
                {
                    jellySurfaceFollower?.SetFollowingEnabled(true);

                    if (Mathf.Abs(_moveInput.x) <= 0.01f)
                        Idle();
                    else
                        Move();

                    break;
                }
            }
        }

        private static bool IsDeathLayer(GameObject target)
        {
            if (target == null)
                return false;

            int layer = target.layer;

            return layer == LayerMask.NameToLayer("Monster") ||
                   layer == LayerMask.NameToLayer("Hazard") ||
                   layer == LayerMask.NameToLayer("DeathZone");
        }

        // 트리거 처리
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isDead)
                return;

            if (IsDeathLayer(other.gameObject))
            {
                Die();

                return;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Goal Flag"))
            {
                if (SceneManager.GetActiveScene().name != "MainMenu")
                    StageClear();
            }
        }

        public override void Die()
        {
            if (_isDead)
                return;

            _isDead = true;
            base.Die();

            _moveInput = Vector2.zero;
            jellySurfaceFollower?.SetFollowingEnabled(false);

            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
            _rigidbody.simulated = false;

            if (_collider != null)
                _collider.enabled = false;

            // 사운드가 없어도 사망 연출과 재시작이 진행되도록 먼저 예약한다.
            StartCoroutine(PlayDeathSequence());
            ManagersHub.Sound?.PlayDeathSFX();

            Debug.Log("Player Dead");
        }

        private IEnumerator PlayDeathSequence()
        {
            // Die 상태의 Hit 스프라이트가 적용된 다음 조각을 생성한다.
            yield return null;

            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            float waitTime = deathDelay;

            if (pixelShatterEffect != null && pixelShatterEffect.Play(spriteRenderer))
                waitTime = Mathf.Max(waitTime, pixelShatterEffect.Duration);

            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void StageClear()
        {
            Debug.Log("Stage Clear");

            // 클리어 처리
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            int nextScene = currentScene + 1;

            // 다음 씬이 존재하면 이동
            if (nextScene < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(nextScene);
        }
    }
}
