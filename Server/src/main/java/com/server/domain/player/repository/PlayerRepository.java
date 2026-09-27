package com.server.domain.player.repository;

import com.server.domain.player.entity.Player;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface PlayerRepository extends JpaRepository<Player, Long> {
	Optional<Player> findByNickname(String nickname);
}
