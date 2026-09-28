# Changelog

## [Unreleased]
- Changed default value of `AllowDrainWhileStunned` to `true` so health drain continues during stuns and tumbles by default.
- Changed default value of `EnableFullHealthMoneyConversion` to `true` to enable full health currency conversion by default.

## [1.2.0] - 2026-09-27
- Converted `DrainPercentage` configuration entry to an integer type (`int`, default `10`, range 1 - 100).
- Lowered `TickIntervalSeconds` maximum allowed value from 10.0s to 3.0s (`AcceptableValueRange<float>(0.2f, 3.0f)`).
- Fixed full health money conversion scale: converted raw dollar earnings (`drained HP * multiplier`) into the game's native $K currency units (`$1K = 1 unit`) via dollar accumulation, ensuring a 500 HP enemy with a 10x multiplier yields $5K ($5,000) instead of an erroneous 5,000K ($5,000,000).
- Momentarily display the total run currency HUD indicator (`CurrencyUI`) when gaining money from monster life drain, with debounce logic ensuring it waits until the indicator completely disappears before showing it again.
- Added optional full health money conversion mechanic (`EnableFullHealthMoneyConversion`, default `false`) allowing players at 100% HP to convert excess stolen monster health into immediate run currency.
- Added `FullHealthMoneyMultiplier` (default `10`, range 0 - 300) to scale currency earned per stolen HP.
- Added `MaxCurrencyCap` (default `999999`) and integer overflow safety checks to prevent breaking run currency or exceeding game limits.

## [1.1.0] - 2026-09-27
- Changed `DrainFixedAmount` default value to 25 and expanded allowed range to 0 - 500 HP.
- Added lower bound of 0 to `MaxHealthCap` (0 - 1000) to prevent negative health configurations.
- Expanded `TickIntervalSeconds` maximum range up to 10.0 seconds.
- Added `AllowDrainWhileStunned` configuration option (default `false`) allowing health drain to continue while stunned or tumbling if grabbing the monster.
- Simplified `## Issues & Bug Reports` section in documentation.

## [1.0.0] - 2026-09-27
- Initial release!
- Grab monsters to drain health directly into your player health pool at natural transfer cadence (1.0s ticks).
- Monsters always receive full tick damage regardless of player health deficit.
- Offensively siphon monsters even when already at full health.
- Native audiovisual feedback with screen pulse and heartbeat cues.
- Fully configurable: percentage or fixed drain mode, tick interval, and max health cap.
