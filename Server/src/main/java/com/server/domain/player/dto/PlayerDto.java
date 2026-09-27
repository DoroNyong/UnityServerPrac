package com.server.domain.player.dto;

import com.server.domain.player.entity.Player;

public class PlayerDto {

	public record SaveRequest(String nickname, int level, int gold) {
		public Player toEntity() {
			return Player.builder()
					.nickname(nickname)
					.level(level)
					.gold(gold)
					.build();
		}
	}

	public record Response(Long id, String nickname, int level, int gold) {
		public static Response from(Player player) {
			return new Response(
					player.getId(),
					player.getNickname(),
					player.getLevel(),
					player.getGold()
			);
		}
	}
}
