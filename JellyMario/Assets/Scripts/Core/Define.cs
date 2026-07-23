namespace JellyMario.Core
{
    // 게임의 현재 상태
    public enum GameState
    {
        Title,
        Playing,
        Pause,
        Result
    }

    // 씬 종류
    public enum SceneType
    {
        Init,
        MainMenu,
        Tutorial,
        Easy,
        Hard
    }
}