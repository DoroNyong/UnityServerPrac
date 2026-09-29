using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// 회원가입/로그인 HTTP 통신 담당 (UI에 의존하지 않음)
public class AuthNetworkManager : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:8080";
    [SerializeField] private int timeoutSeconds = 10;

    private const string SignupPath = "/api/members/signup";
    private const string LoginPath = "/api/members/login";

    // 로그인 성공 후 다른 시스템에서 참조할 수 있도록 보관
    public AuthResponse CurrentMember { get; private set; }

    public void Signup(string username, string password, Action<AuthResult> onDone)
    {
        StartCoroutine(Post(SignupPath, username, password, onDone));
    }

    public void Login(string username, string password, Action<AuthResult> onDone)
    {
        StartCoroutine(Post(LoginPath, username, password, result =>
        {
            if (result.success) CurrentMember = result.data;
            onDone?.Invoke(result);
        }));
    }

    private IEnumerator Post(string path, string username, string password, Action<AuthResult> onDone)
    {
        string json = JsonUtility.ToJson(new AuthRequest { username = username, password = password });
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(baseUrl + path, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            onDone?.Invoke(BuildResult(req));
        }
    }

    private static AuthResult BuildResult(UnityWebRequest req)
    {
        var result = new AuthResult { responseCode = req.responseCode };
        string text = req.downloadHandler != null ? req.downloadHandler.text : null;

        switch (req.result)
        {
            case UnityWebRequest.Result.Success:
                result.success = true;
                result.data = SafeParse<AuthResponse>(text);
                result.message = result.data != null && !string.IsNullOrEmpty(result.data.message)
                    ? result.data.message
                    : "요청이 성공했습니다.";
                break;

            case UnityWebRequest.Result.ProtocolError: // 4xx, 5xx
                ErrorResponse err = SafeParse<ErrorResponse>(text);
                string detail = err != null && !string.IsNullOrEmpty(err.message) ? err.message
                              : err != null && !string.IsNullOrEmpty(err.error) ? err.error
                              : !string.IsNullOrEmpty(text) ? text
                              : req.error;
                result.message = $"[{req.responseCode}] {detail}";
                break;

            default: // ConnectionError, DataProcessingError
                result.message = $"서버에 연결할 수 없습니다. ({req.error})";
                break;
        }

        return result;
    }

    private static T SafeParse<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonUtility.FromJson<T>(json); }
        catch (Exception) { return null; } // HTML/일반 텍스트 응답 등
    }
}
