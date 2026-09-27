using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using NativeWebSocket;

public class ChatNetworkTester : MonoBehaviour
{
    private const string ServerUrl = "ws://localhost:8080/ws/game";

    [Header("플레이어 설정")]
    public string senderName = "Player_A"; // 인스펙터에서 창마다 다르게 설정 가능

    private WebSocket websocket;

    // 스프링 부트의 ChatMessageDto와 필드명을 동일하게 맞춥니다.
    [Serializable]
    public class ChatMessage
    {
        public string sender;
        public string message;
        public string type;
    }

    async void Start()
    {
        websocket = new WebSocket(ServerUrl);

        // 1. 연결 성공 이벤트
        websocket.OnOpen += () =>
        {
            Debug.Log($"<color=cyan>[WebSocket 연결 성공]</color> {ServerUrl}");
            // 접속 알림 전송
            SendMessageToServer("ENTER", $"{senderName}님이 접속하셨습니다.");
        };

        // 2. 에러 이벤트
        websocket.OnError += (e) =>
        {
            Debug.LogError($"[WebSocket 에러] {e}");
        };

        // 3. 연결 종료 이벤트
        websocket.OnClose += (e) =>
        {
            Debug.LogWarning($"[WebSocket 연결 종료] Code: {e}");
        };

        // 4. 메시지 수신 이벤트 (서버가 브로드캐스팅한 패킷 도착)
        websocket.OnMessage += (bytes) =>
        {
            string json = Encoding.UTF8.GetString(bytes);
            ChatMessage chat = JsonUtility.FromJson<ChatMessage>(json);

            Debug.Log($"<color=yellow>[실시간 수신]</color> [{chat.sender}] : {chat.message} (타입: {chat.type})");
        };

        // 서버 연결 시도
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        // 웹소켓 메시지 큐 디스패치 (메인 스레드 동기화 처리)
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
#endif

        // 신규 Input System: 스페이스바를 누르면 테스트 메시지 전송
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            string testText = $"안녕하세요! 현재 시각 프레임: {Time.frameCount}";
            SendMessageToServer("CHAT", testText);
        }
    }

    // 서버로 메시지 전송 함수
    public async void SendMessageToServer(string type, string content)
    {
        if (websocket.State == WebSocketState.Open)
        {
            ChatMessage chat = new ChatMessage
            {
                sender = this.senderName,
                message = content,
                type = type
            };

            string json = JsonUtility.ToJson(chat);
            await websocket.SendText(json);
            Debug.Log($"<color=white>[전송 완료]</color> {content}");
        }
        else
        {
            Debug.LogWarning("[전송 실패] 웹소켓이 아직 연결되지 않았습니다.");
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
}