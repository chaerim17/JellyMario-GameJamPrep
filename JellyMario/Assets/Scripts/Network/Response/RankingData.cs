namespace JellyMario.Network.Response
{
    // 랭킹 한 명의 정보
    [System.Serializable]
    public class RankingData
    {
        // 랭킹 ID
        public long id;

        // 플레이어 이름
        public string playerName;

        // 클리어 시간
        public float clearTime;

        // 점수 등록 시간
        public string createdAt;
    }
}