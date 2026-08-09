using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JellyMario.Enemy
{
    public enum EnemyState
    {
        Idle,
        Move,
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

        [Header("Die 설정")]
        [SerializeField] private Sprite[] dieFrames;
        [SerializeField] private float dieFrameTime = 0.5f;

        private SpriteRenderer _spriteRenderer;
        private Coroutine _animationCoroutine;
        private bool _hasInitializedState;
        private bool _restartAnimationOnEnable;

        protected SpriteRenderer CachedSpriteRenderer => _spriteRenderer;

        public EnemyState CurrentState { get; private set; }

        protected Vector2[] CacheWaypointLocalPositions(
            Transform[] movePoints)
        {
            List<Vector2> positions = new List<Vector2>();

            if (movePoints == null)
                return positions.ToArray();

            foreach (Transform movePoint in movePoints)
            {
                if (movePoint == null)
                    continue;

                positions.Add(movePoint.localPosition);
            }

            return positions.ToArray();
        }

        protected Vector2 WaypointLocalToWorld(Vector2 localPosition)
        {
            if (transform.parent == null)
                return localPosition;

            return transform.parent.TransformPoint(localPosition);
        }

        // 적 초기화
        protected virtual void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            IgnoreOtherEnemyCollisions();

            ChangeState(EnemyState.Idle);
        }

        private void IgnoreOtherEnemyCollisions()
        {
            Collider2D[] ownColliders = GetComponentsInChildren<Collider2D>(true);
            EnemyBase[] enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsInactive.Include);

            foreach (EnemyBase otherEnemy in enemies)
            {
                if (otherEnemy == null || otherEnemy == this)
                    continue;

                Collider2D[] otherColliders =
                    otherEnemy.GetComponentsInChildren<Collider2D>(true);

                foreach (Collider2D ownCollider in ownColliders)
                {
                    foreach (Collider2D otherCollider in otherColliders)
                    {
                        if (ownCollider != null && otherCollider != null)
                            Physics2D.IgnoreCollision(ownCollider, otherCollider, true);
                    }
                }
            }
        }

        protected virtual void OnEnable()
        {
            if (_restartAnimationOnEnable && _animationCoroutine == null)
                SetAnimation(CurrentState);

            _restartAnimationOnEnable = false;
        }

        protected virtual void OnDisable()
        {
            _restartAnimationOnEnable = _hasInitializedState;
            StopCurrentAnimation();
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

        // 사망
        public virtual void Die()
        {
            ChangeState(EnemyState.Die);
        }

        // 상태 변경
        protected void ChangeState(EnemyState newState)
        {
            if (_hasInitializedState && CurrentState == newState)
                return;

            CurrentState = newState;
            _hasInitializedState = true;
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

                case EnemyState.Die:
                    selectedFrames = dieFrames;
                    selectedFrameTime = dieFrameTime;
                    break;

                default:
                    Debug.LogWarning($"등록되지 않은 애니메이션: {state}", this);

                    return;
            }

            StopCurrentAnimation();

            if (selectedFrames == null || selectedFrames.Length == 0) 
            {
                Debug.LogWarning($"{state} 이미지가 등록되지 않았습니다.", this);

                return;
            }

            if (_spriteRenderer == null)
            {
                Debug.LogWarning("Enemy animation requires a SpriteRenderer.", this);
                return;
            }

            _animationCoroutine = StartCoroutine(PlayAnimation(selectedFrames, selectedFrameTime));
        }

        private void StopCurrentAnimation()
        {
            if (_animationCoroutine == null)
                return;

            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }

        private IEnumerator PlayAnimation(Sprite[] frames, float frameTime)
        {
            WaitForSeconds wait = new WaitForSeconds(Mathf.Max(frameTime, 0.01f));

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
