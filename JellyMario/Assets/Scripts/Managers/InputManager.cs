using UnityEngine;
using UnityEngine.InputSystem;
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
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
                return Vector2.zero;

            // 좌우 이동 입력 처리
            float horizontal = 0f;

            if (keyboard.leftArrowKey.isPressed)
                horizontal -= 1f;
            if (keyboard.rightArrowKey.isPressed)
                horizontal += 1f;

            return new Vector2(horizontal, 0f);
        }

        // 점프 입력 반환
        public virtual bool GetJumpInput()
        {
            Keyboard keyboard = Keyboard.current;

            // 점프 입력은 위쪽 화살표 키를 눌렀을 때 반환
            return keyboard != null &&
                   keyboard.upArrowKey.wasPressedThisFrame;
        }
    }
}