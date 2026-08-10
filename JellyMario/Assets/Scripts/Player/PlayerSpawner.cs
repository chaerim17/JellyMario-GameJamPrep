using UnityEngine;

namespace JellyMario.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerSpawner : MonoBehaviour
    {
        private const string SelectedCharacterKey = "SelectedCharacter";

        [Header("Spawn 설정")]
        [SerializeField] private Transform playerPoint;
        [SerializeField] private GameObject[] playerPrefabs;

        public GameObject SpawnedPlayer { get; private set; }

        private void Awake()
        {
            SpawnSelectedPlayer();
        }

        public void SpawnSelectedPlayer()
        {
            if (playerPoint == null)
                playerPoint = transform;

            if (playerPrefabs == null || playerPrefabs.Length == 0)
            {
                Debug.LogError("생성할 플레이어 프리팹이 등록되지 않았습니다.", this);
                return;
            }

            int selectedIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(SelectedCharacterKey, 0),
                0,
                playerPrefabs.Length - 1
            );

            GameObject selectedPrefab = playerPrefabs[selectedIndex];

            if (selectedPrefab == null)
            {
                Debug.LogError($"{selectedIndex}번 플레이어 프리팹이 비어 있습니다.", this);
                return;
            }

            RemoveScenePlayers();

            SpawnedPlayer = Instantiate(
                selectedPrefab,
                playerPoint.position,
                playerPoint.rotation
            );

            SpawnedPlayer.name = "Player";

            Debug.Log(
                $"선택 캐릭터 생성 완료: {selectedIndex} ({selectedPrefab.name})",
                SpawnedPlayer
            );
        }

        private static void RemoveScenePlayers()
        {
            PlayerController[] scenePlayers =
                FindObjectsByType<PlayerController>();

            foreach (PlayerController scenePlayer in scenePlayers)
            {
                scenePlayer.gameObject.SetActive(false);
                Destroy(scenePlayer.gameObject);
            }
        }
    }
}
