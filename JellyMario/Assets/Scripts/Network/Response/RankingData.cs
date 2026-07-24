namespace JellyMario.Network.Response
{
    // 랭킹 한 명의 정보
    [System.Serializable]
    public class RankingData
    {
        // 플레이어 이름
        public string playerName;

        // 플레이어 점수
        public int score;
    }
}