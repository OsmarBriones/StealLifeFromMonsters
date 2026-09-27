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
		internal static ConfigEntry<bool> EnableAudioVisualFeedback { get; private set; } = null!;

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
				10,
				new ConfigDescription(
					"Fixed amount of HP to drain per tick (when DrainMode is Fixed).",
					new AcceptableValueRange<int>(1, 100)
				)
			);

			TickIntervalSeconds = configFile.Bind(
				"Mechanics",
				nameof(TickIntervalSeconds),
				1.0f,
				new ConfigDescription(
					"Seconds between each health drain tick (matches native player transfer interval by default).",
					new AcceptableValueRange<float>(0.2f, 5.0f)
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
				"Maximum health cap allowed for life drain."
			);

			EnableAudioVisualFeedback = configFile.Bind(
				"Feedback",
				nameof(EnableAudioVisualFeedback),
				true,
				"Enable or disable native heal audio and screen pulse effects when draining health."
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
