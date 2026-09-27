package com.server.global.websocket;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.data.redis.listener.ChannelTopic;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.CloseStatus;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;

import java.io.IOException;
import java.util.Set;
import java.util.concurrent.CopyOnWriteArraySet;

@Slf4j
@Component
@RequiredArgsConstructor
public class GameWebSocketHandler extends TextWebSocketHandler {

	private final Set<WebSocketSession> sessions = new CopyOnWriteArraySet<>();
	private final StringRedisTemplate redisTemplate;
	private final ChannelTopic topic;

	@Override
	public void afterConnectionEstablished(WebSocketSession session) {
		sessions.add(session);
		log.info("[WebSocket 연결] 세션 ID: {} (현재 동접: {}명)", session.getId(), sessions.size());
	}

	@Override
	protected  void handleTextMessage(WebSocketSession session, TextMessage message) {
		String payload = message.getPayload();
		log.info("[WebSocket 수신] 세션 {}: {}", session.getId(), payload);

		redisTemplate.convertAndSend(topic.getTopic(), payload);
	}

	@Override
	public void afterConnectionClosed(WebSocketSession session, CloseStatus status) {
		sessions.remove(session);
		log.info("[WebSocket 종료] 세션 ID: {} (현재 동접: {}명)", session.getId(), sessions.size());
	}

	public void broadcast(String message) {
		for (WebSocketSession session : sessions) {
			if (session.isOpen()) {
				try {
					session.sendMessage(new TextMessage(message));
				} catch (IOException e) {
					log.error("[WebSocket 전송 실패] 세션 ID: {}", session.getId(), e);
				}
			}
		}
	}
}
