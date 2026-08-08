using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using JellyMario.Core;
using JellyMario.Network.Request;
using JellyMario.Network.Response;

using JellyMario.UI;

namespace JellyMario.Managers
{
    // 웹(API)를 관리하는 매니저 - Todo : ApiRoutes.cs로 이동 예정
    public class WebManager : Singleton<WebManager>
    {
        [SerializeField]
        private RankingUI rankingUI;

        // DB 주소와 Publishable Key
        private const string API_URL =
            "https://ifobygojapncuwpwfvwt.supabase.co/rest/v1/ranking";
        private const string PLAYER_API_URL =
            "https://ifobygojapncuwpwfvwt.supabase.co/rest/v1/player";

        private const string API_KEY =
            "sb_publishable_CeTXneXavNVq9EXdq4N9VQ_qvWSh2B1";

        protected override void Initialize()
        {
            base.Initialize();
        }

        // GET 요청을 보내는 공통 함수
        private IEnumerator SendGetRequest(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            request.SetRequestHeader("apikey", API_KEY);
            request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                OnRankingReceived(json);
                Debug.Log($"GET request succeeded: {json}");
            }
            else
            {
                Debug.LogError($"GET request failed: {request.error}");
            }
        }

        // POST 요청을 보내는 공통 함수
        private IEnumerator SendPostRequest(string url, string jsonData)
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

            UnityWebRequest request = new UnityWebRequest(url, "POST");

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", API_KEY);
            request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"POST request succeeded: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError(
                $"POST request failed\n" +
                $"Code: {request.responseCode}\n" +
                $"Error: {request.error}\n" +
                $"Response: {request.downloadHandler.text}"
                );
            }
        }

        ////Todo: SubmitScore로 변경 -> 실데이터 적용 테스트 시 삭제
        public void TestPostConnection()
        {
            SubmitScoreRequest requestData =
                new SubmitScoreRequest();

            requestData.playerName = "Chaerim";
            requestData.clearTime = 1500f;

            string jsonData =
                JsonUtility.ToJson(requestData);

            StartCoroutine(SendPostRequest(API_URL, jsonData));
            Debug.Log(jsonData);
        }

        // 랭킹 조회
        public void GetRanking()
        {
            string url =
                API_URL +
                "?select=*&order=clearTime.asc&limit=10";

            StartCoroutine(
                SendGetRequest(url)
            );
        }

        // 점수 등록
        public void SubmitScore(string playerName, float clearTime)
        {
            SubmitScoreRequest requestData = new SubmitScoreRequest();

            requestData.playerName = playerName;
            requestData.clearTime = clearTime;

            string jsonData = JsonUtility.ToJson(requestData);

            StartCoroutine(
                SendPostRequest(API_URL, jsonData)
            );

            Debug.Log(jsonData);
        }

        // 랭킹 조회 응답 처리
        private void OnRankingReceived(string json)
        {
            string wrappedJson =
                "{\"rankings\":" + json + "}";

            RankingResponse response =
                JsonUtility.FromJson<RankingResponse>(
                    wrappedJson
                );

            Debug.Log($"rankingUI = {rankingUI}");
            Debug.Log($"response = {response}");

            if (response != null)
            {
                Debug.Log($"rankings = {response.rankings}");
            }

            rankingUI.ShowRanking(response.rankings);
        }

        public void SubmitProfile(string playerName, int characterId)
        {
            SubmitProfileRequest requestData =
                new SubmitProfileRequest();

            requestData.playerName = playerName;
            requestData.characterId = characterId;

            string jsonData =
                JsonUtility.ToJson(requestData);

            StartCoroutine(
                SendPostRequest(
                    PLAYER_API_URL,
                    jsonData
                )
            );

            Debug.Log(jsonData);
        }
    }
}