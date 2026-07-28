using System.Collections;
using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Player
{
    // 플레이어를 제어하는 기본 클래스
    public class PlayerController : PlayerBase
    {
        private const string IdleAnimation = "Idle";

        [Header("Idle 설정")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private float idleFrameTime = 0.5f;

        private SpriteRenderer _spriteRenderer;
        private Coroutine _animationCoroutine;
        private string _currentAnimationName;

        protected override void Initialize()
        {
            base.Initialize();

            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (ManagersHub.Player !=null)
                ManagersHub.Player.RegisterPlayer(this);

            // 플레이어 초기화
            Idle();
        }
        
        public override void Idle()
        {
            SetAnimation(IdleAnimation);
        }

        public override void Move()
        {
            // 이동 구현
        }

        public override void Run()
        {
            // 달리기 구현
        }

        public override void Jump()
        {
            // 점프 구현
        }

        public override void Hit()
        {
            // 피격 구현
        }

        public override void Die()
        {
            // 사망 구현
        }

        protected override void SetAnimation(string animationName)
        {
            // 현재 실행 중인 Animation 중복 실행 방지
            if (_currentAnimationName == animationName && _animationCoroutine != null)
                return;

            if (animationName != IdleAnimation) {
                Debug.LogWarning($"등록되지 않은 애니메이션: {animationName}");

                return;
            }

            if (_spriteRenderer == null) {
                Debug.LogWarning("SpriteRenderer를 찾을 수 없습니다.");

                return;
            }

            if (idleFrames == null || idleFrames.Length == 0) {
                Debug.LogWarning("Idle 이미지가 등록되지 않았습니다.");

                return;
            }

            if (_animationCoroutine != null) {
                StopCoroutine(_animationCoroutine);
            }

            _currentAnimationName = animationName;

            _animationCoroutine =
                StartCoroutine(PlayIdleAnimation());
        }

        private IEnumerator PlayIdleAnimation()
        {
            float frameTime = Mathf.Max(0.01f, idleFrameTime);

            WaitForSeconds wait = new WaitForSeconds(frameTime);

            while (true) {
                foreach (Sprite frame in idleFrames) {
                    if (frame != null) {
                        _spriteRenderer.sprite = frame;
                    }

                    yield return wait;
                }
            }
        }
    }
}