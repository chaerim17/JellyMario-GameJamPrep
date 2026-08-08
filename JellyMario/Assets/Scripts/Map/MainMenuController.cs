using UnityEngine;
using JellyMario.Managers;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject rankingPanel;

    private void Start()
    {
        loginPanel.SetActive(false);
        rankingPanel.SetActive(false);
    }

    // 골인 지점
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        loginPanel.SetActive(true);
    }

    // 랭킹 버튼
    public void OnClickRankingButton()
    {
        rankingPanel.SetActive(true);

        WebManager.Instance.GetRanking();
    }

    // 랭킹 닫기
    public void OnClickCloseRanking()
    {
        rankingPanel.SetActive(false);
    }
}