# StealLifeFromMonsters

Grab monsters and drain their health to heal yourself, using the player health transfer cadence. Fully configurable. Only Host, clients don't need it.

Only the host needs to have this mod installed — other players see the effects automatically without installing anything.

## Features

- **Monster Life Drain**: Grab any active monster and hold the grab interaction to drain its life force directly into your own health pool.
- **Natural Transfer Cadence**: Uses the exact rhythm and timing of the player-to-player health transfer mechanic (1-second pulses).
- **Consistent Monster Damage & Full Health Siphon**: Monsters always receive the full drain damage per tick regardless of your current HP. If your health is already full, you can continue draining to damage the monster without gaining excess HP (unless Overheal is enabled).
- **Native Audiovisual Feedback**: Triggers full visual screen pulse and audio heartbeat cues on every healing tick.
- **Host-Only Multiplayer**: Only the host needs to install the mod. Health changes and damage synchronize across all connected players automatically.
- **Fully Configurable**: Switch between percentage-based or fixed HP drain, adjust tick rates, and customize health caps.

## Requirements
- [BepInEx Pack for R.E.P.O.](https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/)

## Installation
1. Install the latest [BepInEx Pack](https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/).
2. Place `StealLifeFromMonsters.dll` into your `BepInEx/plugins` folder (or install via r2modman / Thunderstore Mod Manager).
3. Launch the game once — the configuration file will be generated automatically inside `BepInEx/config`.

## Configuration
All settings are controlled through the generated file: `com.osmar.StealLifeFromMonsters.cfg` located in `BepInEx/config`.

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | `true` | Enable or disable the mod. |
| `DrainMode` | `Percentage` | Calculation mode: `Percentage` (scales with monster health) or `Fixed` (flat HP). |
| `DrainPercentage` | `10.0` | Percentage of monster's max health to drain per tick (1 - 100%). |
| `DrainFixedAmount` | `10` | Flat HP drained per tick when using `Fixed` mode (1 - 100). |
| `TickIntervalSeconds` | `1.0` | Seconds between each drain tick (0.2 - 5.0 seconds). |
| `AllowOverheal` | `false` | When true, allows draining past normal max health up to `MaxHealthCap`. |
| `MaxHealthCap` | `100` | Maximum health allowed when draining. |
| `EnableAudioVisualFeedback` | `true` | Plays native heal sound and screen pulse effects while draining. |

## Issues & Bug Reports
The official way to report issues, suggest improvements, or submit feedback is by opening an issue on the official GitHub repository:
👉 [GitHub Issues](https://github.com/OsmarBriones/StealLifeFromMonsters/issues)

## Credits
Developed by **Osmar Briones**
