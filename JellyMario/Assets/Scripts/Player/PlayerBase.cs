using System.Collections;
using UnityEngine;

namespace JellyMario.Player
{
    // 플레이어의 상태
    public enum PlayerState
    {
        Idle,
        Move,
        Jump,
        Die
    }

    // 모든 플레이어의 부모 클래스
    public class PlayerBase : MonoBehaviour
    {
        [Header("Idle 설정")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private float idleFrameTime = 0.5f;

        [Header("Move 설정")]
        [SerializeField] private Sprite[] moveFrames;
        [SerializeField] private float moveFrameTime = 0.1f;

        [Header("Jump 설정")]
        [SerializeField] private Sprite[] jumpFrames;
        [SerializeField] private float jumpFrameTime = 0.1f;

        [Header("Die 설정")]
        [SerializeField] private Sprite[] dieFrames;
        [SerializeField] private float dieFrameTime = 0.5f;

        private SpriteRenderer _spriteRenderer;
        private Coroutine _animationCoroutine;

        public PlayerState CurrentState { get; private set; }

        // 플레이어 초기화
        protected virtual void Awake()
        {
            Initialize();

            CurrentState = PlayerState.Idle;
            SetAnimation(CurrentState);
        }

        protected virtual void Update()
        {
            HandleInput();
            HandleMovement();
        }

        protected virtual void Initialize()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected virtual void HandleInput()
        {
        }

        protected virtual void HandleMovement()
        {
        }

        protected virtual void ChangeState(PlayerState newState)
        {
            if (CurrentState == newState)
                return;

            CurrentState = newState;
            SetAnimation(CurrentState);
        }

        // 대기
        public virtual void Idle()
        {
            ChangeState(PlayerState.Idle);
        }

        // 이동
        public virtual void Move()
        {
            ChangeState(PlayerState.Move);
        }

        // 점프
        public virtual void Jump()
        {
            ChangeState(PlayerState.Jump);
        }

        // 사망
        public virtual void Die()
        {
            ChangeState(PlayerState.Die);
        }

        protected virtual void SetAnimation(PlayerState state)
        {
            Sprite[] selectedFrames;
            float selectedFrameTime;

            switch (state) 
            {
                case PlayerState.Idle:
                    selectedFrames = idleFrames;
                    selectedFrameTime = idleFrameTime;

                    break;

                case PlayerState.Move:
                    selectedFrames = moveFrames;
                    selectedFrameTime = moveFrameTime;

                    break;

                case PlayerState.Jump:
                    selectedFrames = jumpFrames;
                    selectedFrameTime = jumpFrameTime;
                    break;

                case PlayerState.Die:
                    selectedFrames = dieFrames;
                    selectedFrameTime = dieFrameTime;
                    break;

                default:
                    Debug.LogWarning($"등록되지 않은 애니메이션: {state}", this);

                    return;
            }

            if (selectedFrames == null || selectedFrames.Length == 0)
            {
                Debug.LogWarning($"{state} 이미지가 등록되지 않았습니다.", this);

                return;
            }

            if (_animationCoroutine != null) 
                StopCoroutine(_animationCoroutine);

            bool loop = state != PlayerState.Jump;

            _animationCoroutine = StartCoroutine(PlayAnimation(selectedFrames, selectedFrameTime, state, loop));
        }

        private IEnumerator PlayAnimation(Sprite[] frames, float frameTime, PlayerState state, bool loop)
        {
            WaitForSeconds wait = new WaitForSeconds(frameTime);

            do
            {
                foreach (Sprite frame in frames)
                {
                    if (frame != null)
                        _spriteRenderer.sprite = frame;

                    yield return wait;
                }
            }
            while (loop);

            _animationCoroutine = null;

            if (CurrentState == state)
                OnAnimationFinished(state);
        }

        protected virtual void OnAnimationFinished(PlayerState state)
        {
        }
    }
}