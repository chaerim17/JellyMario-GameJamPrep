using UnityEngine;

namespace JellyMario.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        // UI 초기화
        public virtual void Initialize()
        {
        }

        // UI 표시
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        // UI 숨김
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}