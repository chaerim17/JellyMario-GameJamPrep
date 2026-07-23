using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Managers
{
    // 플레이어 입력을 관리하는 매니저
    public class InputManager : Singleton<InputManager>
    {
        protected override void Initialize()
        {
            base.Initialize();
        }

        // 이동 입력 반환
        public virtual Vector2 GetMoveInput()
        {

            return Vector2.zero;
        }

        // 달리기 입력 반환
        public virtual bool GetRunInput()
        {

            return false;
        }

        // 점프 입력 반환
        public virtual bool GetJumpInput()
        {

            return false;
        }
    }
}