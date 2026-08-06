using JellyMario.Player;
using System.Collections;
using UnityEngine;

namespace JellyMario.Enemy
{
    public enum EnemyState
    {
        Idle,
        Move,
        Hit,
        Die
    }

    // 모든 적의 부모 클래스
    public class EnemyBase : MonoBehaviour
    {
        [Header("Idle 설정")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private float idleFrameTime = 0.5f;

        [Header("Move 설정")]
        [SerializeField] private Sprite[] moveFrames;
        [SerializeField] private float moveFrameTime = 0.5f;

        [Header("Hit 설정")]
        [SerializeField] private Sprite[] hitFrames;
        [SerializeField] private float hitFrameTime = 0.5f;

        [Header("Die 설정")]
        [SerializeField] private Sprite[] dieFrames;
        [SerializeField] private float dieFrameTime = 0.5f;

        private SpriteRenderer _spriteRenderer;
        private Coroutine _animationCoroutine;

        public EnemyState CurrentState { get; private set; }

        // 적 초기화
        protected virtual void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            CurrentState = EnemyState.Idle;
            ChangeState(CurrentState);
        }

        // 프레임마다 호출되는 업데이트 메서드
        protected virtual void Update()
        {
            HandleMovement();
        }

        // 이동 처리
        protected virtual void HandleMovement()
        {
        }

        // 대기
        public virtual void Idle()
        {
            ChangeState(EnemyState.Idle);
        }

        // 이동
        public virtual void Move()
        {
            ChangeState(EnemyState.Move);
        }

        // 피격
        public virtual void Hit()
        {
            ChangeState(EnemyState.Hit);
        }
        // 사망
        public virtual void Die()
        {
            ChangeState(EnemyState.Die);
        }

        // 상태 변경
        protected void ChangeState(EnemyState newState)
        {
            if (CurrentState == newState)
                return;

            CurrentState = newState;
            SetAnimation(newState);
        }

        // 애니메이션
        private void SetAnimation(EnemyState state)
        {
            Sprite[] selectedFrames;
            float selectedFrameTime;

            switch (state) {
                case EnemyState.Idle:
                    selectedFrames = idleFrames;
                    selectedFrameTime = idleFrameTime;
                    break;

                case EnemyState.Move:
                    selectedFrames = moveFrames;
                    selectedFrameTime = moveFrameTime;
                    break;

                case EnemyState.Hit:
                    selectedFrames = hitFrames;
                    selectedFrameTime = hitFrameTime;
                    break;

                case EnemyState.Die:
                    selectedFrames = dieFrames;
                    selectedFrameTime = dieFrameTime;
                    break;

                default:
                    Debug.LogWarning($"등록되지 않은 애니메이션: {state}", this);

                    return;
            }

            if (selectedFrames == null || selectedFrames.Length == 0) {
                Debug.LogWarning($"{state} 이미지가 등록되지 않았습니다.", this);

                return;
            }

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(PlayAnimation(selectedFrames, selectedFrameTime));
        }
        private IEnumerator PlayAnimation(Sprite[] frames, float frameTime)
        {
            WaitForSeconds wait = new WaitForSeconds(frameTime);

            do {
                foreach (Sprite frame in frames) {
                    if (frame != null && _spriteRenderer != null)
                        _spriteRenderer.sprite = frame;

                    yield return wait;
                }
            } while (true);
        }
    }
}
