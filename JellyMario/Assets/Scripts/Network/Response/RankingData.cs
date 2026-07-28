namespace JellyMario.Network.Response
{
    // 랭킹 한 명의 정보
    [System.Serializable]
    public class RankingData
    {
        // 플레이어 이름
        public string playerName;

        // 클리어 시간
        public float clearTime;
    }
}