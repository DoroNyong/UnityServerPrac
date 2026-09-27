package com.server.global.websocket;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

@Slf4j
@Service
@RequiredArgsConstructor
public class RedisSubscriber {

	private final GameWebSocketHandler gameWebSocketHandler;

	public void sendMessage(String publishMessage) {
		log.info("[Redis 수신] 브로드캐스팅 메시지: {}", publishMessage);

		gameWebSocketHandler.broadcast(publishMessage);
	}
}
