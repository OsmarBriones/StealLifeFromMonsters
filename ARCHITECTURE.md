# StealLifeFromMonsters Architecture

This document describes the runtime structure, data flow, and design decisions for this mod.

---

## High-Level Concept

1. **Trigger / Hook:** Triggers whenever a player grabs an active monster (`EnemyRigidbody.physGrabObject.playerGrabbing`).
2. **Authority / Networking:** Strictly Host-authoritative (`SemiFunc.IsMasterClientOrSingleplayer()`). Damage to enemies is propagated via vanilla `EnemyHealth.Hurt()` / `HurtRPC`, and healing to players is propagated via `PlayerHealth.HealOther()` / `UpdateHealthRPC`. Clients do not need the mod installed.
3. **Outcome:** Transfers health from the grabbed monster to the player at a configurable cadence (default: 1.5s ticks, 10% monster health per tick) with native visual and audio feedback, strictly capped at player max health (100 HP) unless overheal is explicitly enabled.

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
  - Clamps amount to monster's remaining health (`healthCurrent`).
  - Calls `enemy.Health.Hurt(drainAmount, Vector3.zero)` to damage monster across network with full tick damage.
  - Computes absorbable heal amount (`Mathf.Min(drainAmount, missingPlayerHealth)`).
  - If `healAmount > 0`, calls `player.playerHealth.HealOther(healAmount, effect: true)`.
  - Always triggers `player.HealedOther()` if feedback is enabled, providing audio/visual cues even if player is at max health.
  - If `EnableFullHealthMoneyConversion` is enabled and player is at 100% health (or reaches 100% HP during the tick), converts excess unconverted drain into raw dollars (`drainToConvert * multiplier`). Accumulates raw dollars and awards 1 in-game currency unit per $1,000 gained (`accumulatedRawDollars / 1000`), accurately matching R.E.P.O.'s $K currency scale and preventing 1000x over-rewarding. Clamped against `MaxCurrencyCap` and `int.MaxValue`. Updates `ShopIncreaseUI` increment and triggers `CurrencyUI` momentary display (3.0s duration) if not already active or animating offscreen.

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
- **Safe Overheal Protection & Siphon:** Player healing is strictly clamped to the maximum health (`playerHealth.maxHealth`) by default, while allowing continued monster damage and siphon feedback when the player is at full health.
- **Zero Allocations:** Grab timer tracking uses a compact `Dictionary<long, float>` indexed by combined instance IDs with periodic garbage-free pruning.
