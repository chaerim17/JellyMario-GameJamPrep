namespace JellyMario.Network.Request
{
    // 점수 등록 요청 데이터
    [System.Serializable]
    public class SubmitScoreRequest
    {
        // 플레이어 이름
        public string playerName;

        // 플레이어 점수
        public int score;
    }
}