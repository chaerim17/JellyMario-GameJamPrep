using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            // TODO: DB에 선택 캐릭터 저장
            // ex) PlayerData.CharacterType = selectedCharacter;

            // TODO: DB에 닉네임 저장
            // ex) PlayerData.Nickname = nickname;

            // TODO: 타이머 시작
            // TimerManager.Instance.StartTimer();

            // TODO: 씬 담당자
            // 여기서 다음 씬 이동 호출 부탁드립니다.
            // ex) SceneManager.LoadScene("GameScene");

            Debug.Log($"닉네임: {nickname}");
            Debug.Log($"선택 캐릭터: {selectedCharacter}");
        }

        public void OnClickClose()
        {
            Hide();
        }
    }
}