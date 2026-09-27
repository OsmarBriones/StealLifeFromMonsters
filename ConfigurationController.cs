using BepInEx.Configuration;
using StealLifeFromMonsters.Drain;

namespace StealLifeFromMonsters
{
	internal static class ConfigurationController
	{
		private static ConfigFile? configFile;

		internal static ConfigEntry<bool> Enabled { get; private set; } = null!;
		internal static ConfigEntry<DrainMode> DrainMode { get; private set; } = null!;
		internal static ConfigEntry<float> DrainPercentage { get; private set; } = null!;
		internal static ConfigEntry<int> DrainFixedAmount { get; private set; } = null!;
		internal static ConfigEntry<float> TickIntervalSeconds { get; private set; } = null!;
		internal static ConfigEntry<bool> AllowOverheal { get; private set; } = null!;
		internal static ConfigEntry<int> MaxHealthCap { get; private set; } = null!;
		internal static ConfigEntry<bool> AllowDrainWhileStunned { get; private set; } = null!;
		internal static ConfigEntry<bool> EnableAudioVisualFeedback { get; private set; } = null!;
		internal static ConfigEntry<bool> EnableFullHealthMoneyConversion { get; private set; } = null!;
		internal static ConfigEntry<int> FullHealthMoneyMultiplier { get; private set; } = null!;
		internal static ConfigEntry<int> MaxCurrencyCap { get; private set; } = null!;

		internal static void Initialize(ConfigFile config)
		{
			configFile = config;

			Enabled = configFile.Bind(
				"General",
				nameof(Enabled),
				true,
				"Enable or disable the StealLifeFromMonsters mod."
			);

			DrainMode = configFile.Bind(
				"Mechanics",
				nameof(DrainMode),
				StealLifeFromMonsters.Drain.DrainMode.Percentage,
				"Calculation mode for life drain: Percentage of monster health or Fixed health amount."
			);

			DrainPercentage = configFile.Bind(
				"Mechanics",
				nameof(DrainPercentage),
				10f,
				new ConfigDescription(
					"Percentage of monster's max health to drain per tick (when DrainMode is Percentage).",
					new AcceptableValueRange<float>(1f, 100f)
				)
			);

			DrainFixedAmount = configFile.Bind(
				"Mechanics",
				nameof(DrainFixedAmount),
				25,
				new ConfigDescription(
					"Fixed amount of HP to drain per tick (when DrainMode is Fixed).",
					new AcceptableValueRange<int>(0, 500)
				)
			);

			TickIntervalSeconds = configFile.Bind(
				"Mechanics",
				nameof(TickIntervalSeconds),
				1.0f,
				new ConfigDescription(
					"Seconds between each health drain tick (matches native player transfer interval by default).",
					new AcceptableValueRange<float>(0.2f, 10.0f)
				)
			);

			AllowOverheal = configFile.Bind(
				"Mechanics",
				nameof(AllowOverheal),
				false,
				"If true, allows players to drain health beyond their normal max health up to MaxHealthCap."
			);

			MaxHealthCap = configFile.Bind(
				"Mechanics",
				nameof(MaxHealthCap),
				100,
				new ConfigDescription(
					"Maximum health cap allowed for life drain.",
					new AcceptableValueRange<int>(0, 1000)
				)
			);

			AllowDrainWhileStunned = configFile.Bind(
				"Mechanics",
				nameof(AllowDrainWhileStunned),
				false,
				"If true, allows health drain to continue even if the player is stunned or tumbling, as long as they are still holding the monster."
			);

			EnableAudioVisualFeedback = configFile.Bind(
				"Feedback",
				nameof(EnableAudioVisualFeedback),
				true,
				"Enable or disable native heal audio and screen pulse effects when draining health."
			);

			EnableFullHealthMoneyConversion = configFile.Bind(
				"Economy",
				nameof(EnableFullHealthMoneyConversion),
				false,
				"If true, damage dealt to monsters while the player is at 100% health is converted into run currency immediately."
			);

			FullHealthMoneyMultiplier = configFile.Bind(
				"Economy",
				nameof(FullHealthMoneyMultiplier),
				10,
				new ConfigDescription(
					"Multiplier applied to drained monster health to calculate currency gained at full health (e.g. 10 means 10x drained HP).",
					new AcceptableValueRange<int>(0, 300)
				)
			);

			MaxCurrencyCap = configFile.Bind(
				"Economy",
				nameof(MaxCurrencyCap),
				999999,
				new ConfigDescription(
					"Maximum total run currency limit allowed to prevent integer overflow or breaking game economy.",
					new AcceptableValueRange<int>(0, 2000000000)
				)
			);

			configFile.Save();
		}

		internal static void Reload()
		{
			configFile?.Reload();
			configFile?.Save();
		}
	}
}
