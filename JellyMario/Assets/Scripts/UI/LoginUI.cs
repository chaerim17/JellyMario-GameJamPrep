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

        private int selectedCharacter;

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

        public void OnClickClose()
        {
            Hide();
        }
    }
}