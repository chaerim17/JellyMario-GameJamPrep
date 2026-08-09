using TMPro;
using UnityEngine;
using UnityEngine.UI;
using JellyMario.Managers;
using UnityEngine.SceneManagement;

namespace JellyMario.UI
{
    public class LoginUI : UIBase
    {
        [Header("Character")]
        [SerializeField] private Button[] characterButtons;
        [SerializeField] private Outline[] outlines;

        [Header("Nickname")]
        [SerializeField] private TMP_InputField nicknameInput;

        private int selectedCharacter = -1;

        public override void Initialize()
        {
            base.Initialize();
            SelectCharacter(0);
        }

        // Test 이후 init 다른 곳에서 호출시 제거
        private void Start()
        {
            Initialize();
            Show();
        }

        public void SelectCharacter(int index)
        {
            selectedCharacter = index;

            for (int i = 0; i < outlines.Length; i++)
            {
                outlines[i].enabled = (i == index);
            }
        }

        public string GetNickname()
        {
            return nicknameInput.text;
        }

        public int GetSelectedCharacter()
        {
            return selectedCharacter;
        }

        public void OnClickStart()
        {
            //Debug.Log($"WebManager = {WebManager.Instance}");
            //Debug.Log("Start 버튼 눌림");

            if (selectedCharacter == -1)
            {
                Debug.LogWarning("캐릭터를 선택해주세요.");
                return;
            }

            string nickname = nicknameInput.text;

            if (string.IsNullOrWhiteSpace(nickname))
            {
                Debug.LogWarning("닉네임을 입력해주세요.");
                return;
            }

            // 플레이어 정보 저장
            PlayerPrefs.SetString("PlayerName", nickname);
            PlayerPrefs.SetInt("SelectedCharacter", selectedCharacter);

            WebManager.Instance.SubmitProfile(
                nickname,
                selectedCharacter
            );

            //Debug.Log($"WebManager = {WebManager.Instance}");
           
            // DB에 선택 캐릭터와 닉네임 전송
            WebManager.Instance.SubmitProfile(nickname, selectedCharacter);
            Debug.Log($"TimerManager = {TimerManager.Instance}");
            
            // 타이머 시작
            TimerManager.Instance.StartTimer();

            // 다음 씬 이동
            int currentIndex = SceneManager.GetActiveScene().buildIndex;

            if (currentIndex + 1 < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(currentIndex + 1);
            }

            Debug.Log($"닉네임: {nickname}");
            Debug.Log($"선택 캐릭터: {selectedCharacter}");
        }

        public void OnClickClose()
        {
            Hide();
        }
    }
}