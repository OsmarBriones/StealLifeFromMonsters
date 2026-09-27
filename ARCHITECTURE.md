# StealLifeFromMonsters Architecture

This document describes the runtime structure, data flow, and design decisions for this mod.

---

## High-Level Concept

1. **Trigger / Hook:** Triggers whenever a player grabs an active monster (`EnemyRigidbody.physGrabObject.playerGrabbing`).
2. **Authority / Networking:** Strictly Host-authoritative (`SemiFunc.IsMasterClientOrSingleplayer()`). Damage to enemies is propagated via vanilla `EnemyHealth.Hurt()` / `HurtRPC`, and healing to players is propagated via `PlayerHealth.HealOther()` / `UpdateHealthRPC`. Clients do not need the mod installed.
3. **Outcome:** Transfers health from the grabbed monster to the player at a configurable cadence (default: 1.0s ticks, 10% monster health per tick) with native visual and audio feedback, strictly capped at player max health (100 HP) unless overheal is explicitly enabled.

---

## Main Data Flow

### 1. Grab Detection & Tick Processing
- **Hook:** `Patches/EnemyRigidbody_Update_Patch.cs` (`HarmonyPostfix` on `EnemyRigidbody.Update`).
- **Authority Check:** `SemiFunc.IsMasterClientOrSingleplayer()`.
- **Target Evaluation:** `EnemyRigidbody.enemy` (verifies monster is alive and has `EnemyHealth`).
- **Grabber Loop:** Evaluates all `PhysGrabber` entries in `physGrabObject.playerGrabbing`.
- **Timing:** `MonsterLifeDrainController` tracks elapsed hold time per `(enemyId, playerId)` pair.
- **Drain Execution:** When `currentTimer >= ConfigurationController.TickIntervalSeconds.Value`:
  - Computes drain amount (`Percentage` vs `Fixed`).
  - Clamps amount to monster's remaining health and player's missing health to the cap.
  - Calls `enemy.Health.Hurt(drainAmount, Vector3.zero)` to damage monster across network.
  - Calls `player.playerHealth.HealOther(drainAmount, effect: true)` and `player.HealedOther()` to heal player and play native feedback.

### 2. Level Lifecycle & Cleanup
- **Hook:** `Patches/RoundDirector_StartRoundLogic_Patch.cs` (`HarmonyPostfix` on `RoundDirector.StartRoundLogic`).
- Resets all cached interaction timers via `MonsterLifeDrainController.Reset()` on round start.

---

## Component Layout

```text
StealLifeFromMonsters/
├── StealLifeFromMonstersPlugin.cs      # BaseUnityPlugin entry point, registers Harmony patches & config
├── ConfigurationController.cs          # BepInEx ConfigFile wrapper
├── Drain/
│   ├── DrainMode.cs                    # Calculation enum: Percentage, Fixed
│   └── MonsterLifeDrainController.cs   # Core logic: grab timers, health math, damage & heal dispatch
└── Patches/
    ├── EnemyRigidbody_Update_Patch.cs  # Postfix on EnemyRigidbody.Update
    └── RoundDirector_StartRoundLogic_Patch.cs # Postfix on RoundDirector.StartRoundLogic
```

---

## Key Design Decisions & Invariants

- **Host-Only Compatibility:** Because all health changes use standard vanilla RPC methods, connected clients run pure vanilla code and receive full health synchronization and visual feedback automatically.
- **Safe Overheal Protection:** Life drain is strictly clamped to the player's maximum health (`playerHealth.maxHealth`) by default to prevent wasting monster life and infinite drain exploits.
- **Zero Allocations:** Grab timer tracking uses a compact `Dictionary<long, float>` indexed by combined instance IDs with periodic garbage-free pruning.
