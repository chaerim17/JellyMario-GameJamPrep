using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Player
{
    // 플레이어를 제어하는 기본 클래스
    public class PlayerController : PlayerBase
    {
        protected override void Initialize()
        {
            base.Initialize();

            ManagersHub.Player.RegisterPlayer(this);

            // 플레이어 초기화
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
            // 애니메이션 변경
        }
    }
}