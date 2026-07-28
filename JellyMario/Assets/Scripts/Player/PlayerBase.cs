using UnityEngine;

namespace JellyMario.Player
{
    // 모든 플레이어의 부모 클래스
    public class PlayerBase : MonoBehaviour
    {
        // 플레이어 초기화
        protected virtual void Initialize()
        {

        }

        // 대기
        public virtual void Idle()
        {

        }

        // 이동
        public virtual void Move()
        {

        }

        // 달리기
        public virtual void Run()
        {

        }

        // 점프
        public virtual void Jump()
        {

        }

        // 피격
        public virtual void Hit()
        {

        }

        // 사망
        public virtual void Die()
        {

        }

        // 애니메이션 변경
        protected virtual void SetAnimation(string animationName)
        {

        }

        protected virtual void Awake()
        {
            Initialize();
        }
    }
}