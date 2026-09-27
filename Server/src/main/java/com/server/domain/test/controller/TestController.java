package com.server.domain.test.controller;

import com.server.domain.test.dto.EchoResponse;
import com.server.domain.test.dto.PingResponse;
import com.server.domain.test.dto.PlayerRequest;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;

@RestController
@RequestMapping("/api/test")
public class TestController {

	// 1. 단순 Ping
	@GetMapping("/ping")
	public ResponseEntity<PingResponse> ping() {
		System.out.println("[Spring Boot] 유니티로부터 Ping 요청 도착!");
		PingResponse response = new PingResponse("Spring Boot 연결 성공!", LocalDateTime.now().toString());
		return ResponseEntity.ok(response);
	}

	// 2. 데이터 Echo
	@PostMapping("/echo")
	public ResponseEntity<EchoResponse> echo(@RequestBody PlayerRequest request) {
		System.out.println("[Spring Boot] 플레이어 접속 데이터 수신: " + request.playerName());
		EchoResponse response = new EchoResponse("SUCCESS", "환영합니다, " + request.playerName() + "님!");
		return ResponseEntity.ok(response);
	}
}
