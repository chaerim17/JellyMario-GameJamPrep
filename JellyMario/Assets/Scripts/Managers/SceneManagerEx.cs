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

        // 씬을 변경하는 함수
        public void LoadScene(string sceneName)
        {
            // sceneName과 같은 이름의 씬으로 이동
            SceneManager.LoadScene(sceneName);

            Debug.Log($"씬 이동 : {sceneName}");
        }

        // 현재 씬 이름을 반환하는 함수
        public string GetCurrentScene()
        {
            return SceneManager.GetActiveScene().name;
        }
    }
}