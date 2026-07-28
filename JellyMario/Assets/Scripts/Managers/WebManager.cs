using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using JellyMario.Core;
using JellyMario.Network.Request;
using JellyMario.Network.Response;

namespace JellyMario.Managers
{
    // 웹(API)를 관리하는 매니저
    public class WebManager : Singleton<WebManager>
    {
        // DB 주소와 Publishable Key
        private const string API_URL =
            "https://ifobygojapncuwpwfvwt.supabase.co/rest/v1/ranking";

        private const string API_KEY =
            "sb_publishable_CeTXneXavNVq9EXdq4N9VQ_qvWSh2B1";

        protected override void Initialize()
        {
            Debug.Log("WebManager Initialize");
            TestConnection();
            TestPostConnection();
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
                Debug.Log($"GET request succeeded: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"GET request failed: {request.error}");
            }
        }

        public void TestConnection()
        {
            SendGetRequest(
                API_URL + "?select=*"
                );
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
        //Todo: Test가 아닌 실제 데이터 값 전달 함수로 변경 예정
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

        }

        // 점수 등록
        public void SubmitScore()
        {

        }
    }
}