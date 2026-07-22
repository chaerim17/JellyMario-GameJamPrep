using JellyMario.Managers;

namespace JellyMario.Core
{
    // 모든 Manager에 접근하기 위한 클래스
    // 앞으로는 ManagersHub.Game, ManagersHub.Scene 형태로 사용한다.
    public static class ManagersHub
    {
        // GameManager 접근
        public static GameManager Game
        {
            get
            {
                return GameManager.Instance;
            }
        }

        // SceneManagerEx 접근
        public static SceneManagerEx Scene
        {
            get
            {
                return SceneManagerEx.Instance;
            }
        }
    }
}