# Implementation Plan: Monster Life Drain

**Branch**: `001-monster-life-drain` | **Date**: 2026-09-27 | **Spec**: [specs/001-monster-life-drain/spec.md](spec.md)

**Input**: Feature specification from `specs/001-monster-life-drain/spec.md`

## Summary

Implement grab-based monster life drain mechanics for R.E.P.O.
When a player grabs an active monster and holds the grab interaction, the mod drains health from the monster and transfers it to the player at the native player-to-player health transfer cadence (1.0s ticks).
Execution is strictly Host-authoritative (`SemiFunc.IsMasterClientOrSingleplayer()`), synchronizing monster health damage and player healing through native game RPCs (`EnemyHealth.Hurt()` and `PlayerHealth.HealOther()`).

## Technical Context

**Language/Version**: C# 12 / `.NET Framework 4.8` (`net48`)
**Primary Dependencies**:
- BepInEx 5.4.x / HarmonyX 2.12.x
- Unity 2022.3.x assemblies (`UnityEngine.CoreModule`, `UnityEngine.PhysicsModule`)
- `Assembly-CSharp.dll` (Game binary)
- `PhotonUnityNetworking` (Multiplayer RPCs)
**Target Platform**: Windows 64-bit / R.E.P.O.
**Project Type**: BepInEx Game Mod (`.dll`)
**Performance Goals**: Zero GC allocations per frame; dictionary lookup only on active grabs; timer-based cadence.
**Constraints**:
- Host-only authority: `SemiFunc.IsMasterClientOrSingleplayer()` for all health mutations and RPC dispatches.
- Modern C# standard: PascalCase properties, camelCase locals, zero Hungarian/underscore prefixes, `internal` class scoping.
- Zero compiler warnings.

## Constitution Check

- [x] **I. RepoKit Alignment**: Complies with `REPO_MODS_WORKSPACE.md` and `REPO_MODS_METHODOLOGY.md`.
- [x] **II. Host-Only Authority**: Uses `SemiFunc.IsMasterClientOrSingleplayer()` for game-state mutations.
- [x] **III. Modern C# & Zero Legacy Prefixes**: Clean naming, no leading underscores or static prefixes.
- [x] **IV. Shared Code Hygiene**: Leverages engine vanilla methods (`EnemyHealth.Hurt`, `PlayerHealth.HealOther`), no redundant code.
- [x] **V. 3-Tier Testing**: Tier 1/2 contract verification and Tier 3 in-game smoke testing via r2modman Debug profile.

## Project Structure

### Documentation (this feature)

```text
specs/001-monster-life-drain/
├── spec.md              # Requirements and user scenarios
├── plan.md              # This technical plan
└── tasks.md             # Ordered task breakdown
```

### Source Code

```text
StealLifeFromMonsters/
├── StealLifeFromMonsters.csproj        # Targets net48, references game libs
├── StealLifeFromMonstersPlugin.cs      # BepInPlugin entry point, loads Harmony patches & config
├── ConfigurationController.cs          # BepInEx ConfigFile wrapper for drain mode, rates, caps
├── Drain/
│   ├── DrainMode.cs                    # Enum: Percentage, Fixed
│   └── MonsterLifeDrainController.cs   # Core logic: grab tracking, timers, damage & healing
└── Patches/
    └── EnemyRigidbody_Update_Patch.cs  # Postfix hook on EnemyRigidbody.Update to detect player grab
```

## Architecture & Data Flow

```
Player holds Grab on Monster
          │
          ▼
EnemyRigidbody.Update()
          │ (Harmony Postfix)
          ▼
MonsterLifeDrainController.ProcessDrain(enemyRb)
          │
          ├── IsMasterClient? ── NO ──> Return
          ├── Grabbers active? ── NO ──> Reset timer & Return
          ├── Player health < MaxHealth? ── NO (and not overheal) ──> Return
          ├── Timer >= TickInterval (1.0s)? ── NO ──> Timer += Time.deltaTime
          │
          ▼ YES (Tick fired)
Calculate drain amount (10% of monster max health or fixed amount)
Clamp amount <= monster.Health.healthCurrent
          │
          ├──> enemy.Health.Hurt(amount, Vector3.zero) ──> HurtRPC to all clients
          │
          └──> player.playerHealth.HealOther(amount, effect: true) ──> UpdateHealthRPC to all clients
               player.HealedOther() ──> Audio & green screen pulse trigger
```

## Harmony Patch Target

- **Class**: `EnemyRigidbody`
- **Method**: `Update()`
- **Patch Type**: `Postfix`
- **Rationale**: `EnemyRigidbody` owns `physGrabObject.playerGrabbing` (list of `PhysGrabber` holding the monster) and already runs host-only checks (`SemiFunc.IsMasterClientOrSingleplayer()`) in this loop.
