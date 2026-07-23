using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Managers
{
    // 게임 UI를 관리하는 매니저
    public class UIManager : Singleton<UIManager>
    {
        // UIManager 초기화
        protected override void Initialize()
        {
            base.Initialize();
        }

        // UI 표시
        public virtual void ShowUI(string uiName)
        {

        }

        // UI 숨기기
        public virtual void HideUI(string uiName)
        {

        }

        // 모든 UI 닫기
        public virtual void CloseAllUI()
        {

        }
    }
}