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

            float currentTime = TimerManager.Instance.CurrentTime;

            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}