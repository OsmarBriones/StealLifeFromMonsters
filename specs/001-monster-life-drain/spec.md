# Feature Specification: Monster Life Drain

**Feature Branch**: `001-monster-life-drain`

**Created**: 2026-09-27

**Status**: Draft

**Input**: User description: "Grab monsters and drain their health to heal yourself, using the player health transfer cadence. Fully configurable. Only Host, clients don't need it."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Grab Monster to Drain Life (Priority: P1)

As an injured player, I want to grab a monster and hold the grab interaction to drain a portion of its life and heal myself, so that I can sustain my health during dangerous encounters.

**Why this priority**: This is the core mechanic of the mod. Without it, the mod provides no gameplay functionality.

**Independent Test**: Can be tested by injuring a player, approaching an active monster, holding the grab button on the monster, and verifying player HP increases while monster HP decreases at the native player-to-player health transfer cadence.

**Acceptance Scenarios**:

1. **Given** an injured player (< 100 HP) and an alive monster, **When** the player grabs the monster and maintains the grab hold, **Then** player health increases and monster health decreases by 10% of the monster's health per tick, using the native transfer cadence.
2. **Given** a player draining a monster, **When** the player releases the grab or moves out of range, **Then** the health transfer immediately stops.
3. **Given** a player draining a monster, **When** the monster's health reaches 0, **Then** the monster dies and the transfer stops.

---

### User Story 2 - Health Capping, Overheal Prevention and Full-Health Siphon (Priority: P2)

As a player at full health, I want to be able to continue grabbing and draining monsters to deal damage to them without gaining excess health, while ensuring my own health does not exceed maximum health (100 HP) unless overheal is explicitly enabled.

**Why this priority**: Allows offensive siphon utility against monsters even when at full HP, while respecting the vanilla maximum health boundary.

**Independent Test**: Grab a monster when player HP is 100 or when player reaches 100 during drain, and verify monster continues losing full tick health while player HP remains at 100.

**Acceptance Scenarios**:

1. **Given** a player at 100 HP, **When** the player grabs and holds a monster, **Then** the monster takes full tick damage (e.g., 20 HP on Bella) and audiovisual feedback plays, while player HP stays capped at 100 HP.
2. **Given** a player at 95 HP draining 10 HP from a monster, **When** the transfer tick occurs, **Then** the monster takes the full 10 HP damage and the player health is clamped at 100 HP (+5 HP absorbed).

---

### User Story 3 - Configurable Drain Mode and Values (Priority: P3)

As a server host, I want to configure whether life drain calculates via percentage or fixed amount, and customize the drain values, so that I can balance the mod for my playgroup.

**Why this priority**: Enables balance tuning for different difficulty levels and player preferences.

**Independent Test**: Change `DrainMode` to `Fixed` with `DrainFixedAmount = 15` in `com.osmar.StealLifeFromMonsters.cfg`, reload the game, and verify player receives exactly 15 HP per tick.

**Acceptance Scenarios**:

1. **Given** `DrainMode = Percentage` and `DrainPercentage = 10.0`, **When** draining a 200 HP monster, **Then** 20 HP is drained per tick.
2. **Given** `DrainMode = Fixed` and `DrainFixedAmount = 5.0`, **When** draining any monster, **Then** 5 HP is drained per tick.

---

### User Story 4 - Native Audiovisual Feedback (Priority: P4)

As a player, I want to see the health transfer beam and hear the transfer heartbeat/pulse audio while draining a monster, so that the action feels satisfying and integrated with the game's aesthetic.

**Why this priority**: Critical for game feel and player clarity during chaotic monster encounters.

**Independent Test**: Initiate monster life drain and verify the native green/red health transfer beam renders and the transfer audio loops during the hold.

**Acceptance Scenarios**:

1. **Given** active life drain on a monster, **When** the transfer is occurring, **Then** the native health transfer visual effect renders between player and monster.
2. **Given** active life drain, **When** transfer ticks fire, **Then** native heartbeat/health transfer audio pulses play.

---

## Functional Requirements

- **FR-001**: The mod MUST operate host-side (`Only Host, clients don't need it.`), detecting grabs and synchronizing health changes across all connected players via vanilla RPCs.
- **FR-002**: The drain interaction MUST trigger when holding the grab button on an alive monster.
- **FR-003**: The drain tick rate MUST match the native player-to-player health transfer interval.
- **FR-004**: Life drain MUST NOT heal the player beyond 100 HP (no overheal).
- **FR-005**: If the player is downed, dead, or loses grip, the transfer MUST terminate immediately.
- **FR-006**: BepInEx configuration MUST expose `Enabled`, `DrainMode` (Percentage/Fixed), `DrainPercentage`, `DrainFixedAmount`, and `EnableVisualFeedback`.

## Key Entities

- **`MonsterLifeDrainController`**: Manages active drain sessions per player, calculates ticks, and applies damage/healing.
- **`ConfigurationController`**: Binds and exposes mod configuration settings.
- **`SemiFunc / PlayerAvatar / Enemy`**: Game engine classes for health state, grab detection, and RPC synchronization.

## Success Criteria

1. Player can replenish missing health from any alive monster by holding the grab interaction.
2. Draining stops reliably upon reaching 100 HP, releasing grab, or monster death.
3. Native health transfer beam and audio loop accurately during the drain.
4. Host installation functions completely without clients needing the mod installed.
