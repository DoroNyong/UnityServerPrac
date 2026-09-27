package com.server.domain.player.controller;

import com.server.domain.player.dto.PlayerDto;
import com.server.domain.player.entity.Player;
import com.server.domain.player.repository.PlayerRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@Slf4j
@RestController
@RequestMapping("/api/players")
@RequiredArgsConstructor
public class PlayerController {

	private final PlayerRepository playerRepository;

	@PostMapping("/save")
	public ResponseEntity<PlayerDto.Response> savePlayer(@RequestBody PlayerDto.SaveRequest req) {
		log.info("[플레이어 저장/갱신 요청] 닉네임: {}, 레벨: {}, 골드: {}", req.nickname(), req.level(), req.gold());

		Player player = playerRepository.findByNickname(req.nickname())
				.map(p -> {
					p.updateScore(req.level(), req.gold());
					return p;
				})
				.orElseGet(req::toEntity);

		Player saved = playerRepository.save(player);
		return ResponseEntity.ok(PlayerDto.Response.from(saved));
	}

	@GetMapping("/{nickname}")
	public ResponseEntity<PlayerDto.Response> getPlayer(@PathVariable String nickname) {
		log.info("[플레이어 조회 요청] 닉네임: {}", nickname);

		return playerRepository.findByNickname(nickname)
				.map(p -> ResponseEntity.ok(PlayerDto.Response.from(p)))
				.orElse(ResponseEntity.notFound().build());
	}
}
