using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using JellyMario.Core;
using JellyMario.Network.Request;

namespace JellyMario.Managers
{
    // 웹(API)를 관리하는 매니저
    public class WebManager : Singleton<WebManager>
    {
        protected override void Initialize()
        {
            Debug.Log("WebManager Initialize");
            TestConnection();
        }

        // GET 요청을 보내는 공통 함수
        private IEnumerator SendGetRequest(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
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

        // Todo: Test가 아닌 실제 API 주소로 변경 필요
        public void TestConnection()
        {
            StartCoroutine(
                SendGetRequest(
                    "https://jsonplaceholder.typicode.com/todos/1"
                )
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
                    $"Error: {request.error}"
                );
            }
        }
        //Todo: Test가 아닌 실제 API 주소로 변경 필요 (SendPostRequest())
        public void TestPostConnection()
        {
            SubmitScoreRequest requestData =
                new SubmitScoreRequest();

            requestData.playerName = "Chaerim";
            requestData.score = 1500;

            string jsonData =
                JsonUtility.ToJson(requestData);

            //StartCoroutine(SendPostRequest(ApiRoutes.SubmitScore,jsonData));
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