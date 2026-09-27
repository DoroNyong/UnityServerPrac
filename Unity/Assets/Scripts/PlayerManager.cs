using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private const string BaseUrl = "http://localhost:8080/api/players";

    [Header("테스트용 플레이어 데이터")]
    public string nickname = "GyeongsanHero";
    public int level = 1;
    public int gold = 100;

    // 스프링 DTO와 변수명을 정확히 일치시켜야 합니다.
    [Serializable]
    public class PlayerSaveRequest
    {
        public string nickname;
        public int level;
        public int gold;
    }

    [Serializable]
    public class PlayerResponse
    {
        public long id;
        public string nickname;
        public int level;
        public int gold;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // S 키를 누르면 현재 인스펙터 값으로 서버에 저장/갱신
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            SavePlayerData();
        }

        // L 키를 누르면 닉네임 기준으로 서버에서 데이터 불러오기
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            LoadPlayerData();
        }

        // G 키를 누르면 로컬 골드 +50 (저장 테스트용)
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            gold += 50;
            Debug.Log($"[로컬] 골드 획득! 현재 골드: {gold} (S키를 눌러 저장하세요)");
        }
    }

    // ==========================================
    // 1. 데이터 저장/갱신 (POST /api/players/save)
    // ==========================================
    public void SavePlayerData()
    {
        StartCoroutine(CoSavePlayer(nickname, level, gold));
    }

    private IEnumerator CoSavePlayer(string name, int lv, int g)
    {
        PlayerSaveRequest reqData = new PlayerSaveRequest
        {
            nickname = name,
            level = lv,
            gold = g
        };

        string json = JsonUtility.ToJson(reqData);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest($"{BaseUrl}/save", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[저장 실패] HTTP {req.responseCode}: {req.error}\n응답: {req.downloadHandler.text}");
            }
            else
            {
                PlayerResponse res = JsonUtility.FromJson<PlayerResponse>(req.downloadHandler.text);
                Debug.Log($"<color=cyan>[저장 성공]</color> DB ID: {res.id} | 닉네임: {res.nickname} | 레벨: {res.level} | 골드: {res.gold}");
            }
        }
    }

    // ==========================================
    // 2. 데이터 불러오기 (GET /api/players/{nickname})
    // ==========================================
    public void LoadPlayerData()
    {
        StartCoroutine(CoLoadPlayer(nickname));
    }

    private IEnumerator CoLoadPlayer(string name)
    {
        string encodedName = UnityWebRequest.EscapeURL(name);

        using (UnityWebRequest req = UnityWebRequest.Get($"{BaseUrl}/{encodedName}"))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                if (req.responseCode == 404)
                {
                    Debug.LogWarning($"[불러오기] '{name}' 플레이어 정보를 DB에서 찾을 수 없습니다.");
                }
                else
                {
                    Debug.LogError($"[불러오기 실패] {req.error}");
                }
            }
            else
            {
                PlayerResponse res = JsonUtility.FromJson<PlayerResponse>(req.downloadHandler.text);

                // 불러온 데이터를 유니티 인스펙터 변수에 반영
                this.level = res.level;
                this.gold = res.gold;

                Debug.Log($"<color=green>[불러오기 성공]</color> DB ID: {res.id} | 닉네임: {res.nickname} | 레벨: {res.level} | 골드: {res.gold}");
            }
        }
    }
}