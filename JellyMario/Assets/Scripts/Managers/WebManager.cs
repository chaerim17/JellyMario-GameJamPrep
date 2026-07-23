using System.Collections;
using JellyMario.Core;

namespace JellyMario.Managers
{
    // 웹(API)를 관리하는 매니저
    public class WebManager : Singleton<WebManager>
    {
        // 웹 매니저 초기화
        public void Initialize()
        {

        }

        // GET 요청을 보내는 공통 함수
        private IEnumerator SendGetRequest(string url)
        {
            yield break;
        }

        // POST 요청을 보내는 공통 함수
        private IEnumerator SendPostRequest(string url, string jsonData)
        {
            yield break;
        }

        // 랭킹 조회
        public void GetRanking()
        {

        }

        // 점수 등록
        public void SubmitScore()
        {

        }
    }
}