using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JellyMario.UI
{
    public class BossUI : UIBase
    {
        [SerializeField] private Image bar;

        public void SetProgress(float value)
        {
            bar.fillAmount = Mathf.Clamp01(value);
        }

        public IEnumerator UpdateProgress(
            float startValue,
            float endValue,
            float duration)
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = timer / duration;

                bar.fillAmount =
                    Mathf.Lerp(
                        startValue,
                        endValue,
                        t);

                yield return null;
            }

            bar.fillAmount = endValue;
        }
    }
}