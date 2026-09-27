# Tasks: Monster Life Drain

**Feature**: Monster Life Drain
**Branch**: `001-monster-life-drain`
**Spec**: [specs/001-monster-life-drain/spec.md](spec.md)
**Plan**: [specs/001-monster-life-drain/plan.md](plan.md)

## Phase 1: Setup & Configuration

- [x] **Task 1.1**: Create `Drain/DrainMode.cs` enum with `Percentage` and `Fixed` modes.
- [x] **Task 1.2**: Update `ConfigurationController.cs` with BepInEx config entries (`Enabled`, `DrainMode`, `DrainPercentage`, `DrainFixedAmount`, `TickIntervalSeconds`, `AllowOverheal`, `MaxHealthCap`).

## Phase 2: Core Domain Logic

- [x] **Task 2.1**: Implement `Drain/MonsterLifeDrainController.cs`:
  - Manage per-player/enemy grab timers.
  - Calculate percentage vs fixed damage amount.
  - Apply damage to monster via `enemy.Health.Hurt(amount, Vector3.zero)`.
  - Apply healing to player via `player.playerHealth.HealOther(amount, effect: true)`.
  - Trigger audio/visual cues with `player.HealedOther()`.
  - Handle overheal prevention and player death/tumble cancellations.

## Phase 3: Harmony Hooking

- [x] **Task 3.1**: Create `Patches/EnemyRigidbody_Update_Patch.cs` with a Harmony Postfix on `EnemyRigidbody.Update()`.
- [x] **Task 3.2**: Remove template dummy patch `Patches/ReloadOnLevelStart.cs` and add `Patches/RoundDirector_StartRoundLogic_Patch.cs` for cleanup.

## Phase 4: Compilation & Build Verification

- [x] **Task 4.1**: Compile the mod with `dotnet build -c Debug` and `dotnet build -c Release` to ensure 0 errors.

## Phase 5: Documentation & Git Finalization

- [x] **Task 5.1**: Update `ARCHITECTURE.md` with runtime data flow and hooks.
- [x] **Task 5.2**: Update `README.md` `## Features` and `## Configuration` sections.
- [x] **Task 5.3**: Commit all implemented files with conventional commit message and push to GitHub.
