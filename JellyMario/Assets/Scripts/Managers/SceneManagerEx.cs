using UnityEngine;
using UnityEngine.SceneManagement;    // Unity의 SceneManager를 사용하기 위해 추가
using JellyMario.Core;

namespace JellyMario.Managers
{
    // 씬(Scene)을 관리하는 클래스
    // Singleton을 상속받기 때문에
    // SceneManagerEx.Instance 로 접근할 수 있다.
    public class SceneManagerEx : Singleton<SceneManagerEx>
    {
        // 게임이 시작될 때 가장 먼저 실행된다.
        protected override void Awake()
        {
            // Singleton의 Awake를 먼저 실행
            base.Awake();

            Debug.Log("SceneManagerEx Awake 실행");
        }

        // 게임 시작 후 첫 씬 설정
        private void Start()
        {
            if (GetCurrentScene() == SceneType.Init)
            {
                LoadScene(SceneType.MainMenu);
            }
        }

        // 씬을 변경하는 함수
        public void LoadScene(SceneType sceneType)
        {
            SceneManager.LoadScene(sceneType.ToString());

            Debug.Log($"씬 이동 : {sceneType}");
        }

        // 현재 씬 이름을 반환하는 함수
        public SceneType GetCurrentScene()
        {
            return (SceneType)System.Enum.Parse(
                typeof(SceneType),
                SceneManager.GetActiveScene().name);
        }
    }
}