using TMPro;
using UnityEngine;

namespace JellyMario.UI
{
    public class TimerUI : UIBase
    {
        [SerializeField] private TextMeshProUGUI timerText;

        public override void Initialize()
        {
            base.Initialize();
        }

        private void Update()
        {
            if (TimerManager.Instance == null) return;

            timerText.text =
                TimerManager.Instance.CurrentTime.ToString("00.00");
        }
    }
}