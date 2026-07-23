using JellyMario.Managers;

namespace JellyMario.Core
{
    // 모든 Manager에 접근하기 위한 클래스
    // ManagersHub.Game, ManagersHub.Scene 형태로 사용한다.
    public static class ManagersHub
    {
        // GameManager 접근
        public static GameManager Game => GameManager.Instance;

        // SceneManagerEx 접근
        public static SceneManagerEx Scene => SceneManagerEx.Instance;

        // WebManager 접근
        public static WebManager Web => WebManager.Instance;

        // ResourceManager 접근
        public static ResourceManager Resource => ResourceManager.Instance;

        // PlayerManager 접근
        public static PlayerManager Player => PlayerManager.Instance;

        // InputManager 접근
        public static InputManager Input => InputManager.Instance;

        // UIManager 접근
        public static UIManager UI => UIManager.Instance;

        // SoundManager 접근
        public static SoundManager Sound => SoundManager.Instance;
    }
}