using UnityEngine;
using TMPro;

using JellyMario.Network.Response;

namespace JellyMario.UI
{
    public class RankingUI : UIBase
    {
        [SerializeField]
        private TMP_Text[] rankTexts;

        [SerializeField]
        private TMP_Text[] nameTexts;

        [SerializeField]
        private TMP_Text[] timeTexts;

        // 랭킹 표시
        public void ShowRanking(RankingData[] rankings)
        {
            for (int i = 0; i < rankTexts.Length; i++)
            {
                if (i < rankings.Length)
                {
                    
                    nameTexts[i].text = rankings[i].playerName;
                    timeTexts[i].text = $"{rankings[i].clearTime:F2}";
                    rankTexts[i].text = (i + 1).ToString();
                }
                else
                {
                    rankTexts[i].text = string.Empty;
                    nameTexts[i].text = string.Empty;
                    timeTexts[i].text = string.Empty;
                }
            }
            gameObject.SetActive(true);
        }

        // 닫기
        public void OnClickClose()
        {
            gameObject.SetActive(false);
        }
    }
}