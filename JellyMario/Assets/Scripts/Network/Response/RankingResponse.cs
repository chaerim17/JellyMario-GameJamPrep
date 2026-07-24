namespace JellyMario.Network.Response
{
    // 랭킹 조회 응답 데이터
    [System.Serializable]
    public class RankingResponse
    {
        // 랭킹 목록
        public RankingData[] rankings;
    }
}