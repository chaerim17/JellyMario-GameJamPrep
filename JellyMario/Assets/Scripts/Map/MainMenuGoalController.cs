using UnityEngine;

public class MainMenuGoalController : MonoBehaviour
{
    [SerializeField] private GameObject loginPanel;

    private void Start()
    {
        loginPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        loginPanel.SetActive(true);
    }
}