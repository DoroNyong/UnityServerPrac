using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkTester : MonoBehaviour
{
    // 로컬 Spring Boot 주소
    private const string ServerUrl = "http://localhost:8080/api/test";

    [Serializable]
    public class PlayerRequest
    {
        public string playerName;
    }

    [Serializable]
    public class PingResponse
    {
        public string message;
        public string timestamp;
    }

    [Serializable]
    public class EchoResponse
    {
        public string status;
        public string reply;
    }

    void Start()
    {
        StartCoroutine(SendPing());
        StartCoroutine(SendEcho("UnityUser1"));
    }

    // 1. GET 테스트
    IEnumerator SendPing()
    {
        using (UnityWebRequest req = UnityWebRequest.Get($"{ServerUrl}/ping"))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Ping 실패] {req.error}");
            }
            else
            {
                PingResponse res = JsonUtility.FromJson<PingResponse>(req.downloadHandler.text);
                Debug.Log($"[Ping 성공] 메시지: {res.message} / 시각: {res.timestamp}");
            }
        }
    }

    // 2. POST 테스트
    IEnumerator SendEcho(string name)
    {
        PlayerRequest data = new PlayerRequest { playerName = name };
        string json = JsonUtility.ToJson(data);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest($"{ServerUrl}/echo", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Echo 실패] {req.error}");
            }
            else
            {
                EchoResponse res = JsonUtility.FromJson<EchoResponse>(req.downloadHandler.text);
                Debug.Log($"[Echo 성공] 상태: {res.status} | {res.reply}");
            }
        }
    }
}