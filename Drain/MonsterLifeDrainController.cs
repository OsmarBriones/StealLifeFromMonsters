using System;
using System.Collections.Generic;
using UnityEngine;

namespace StealLifeFromMonsters.Drain
{
	internal static class MonsterLifeDrainController
	{
		private static readonly Dictionary<long, float> drainTimers = new Dictionary<long, float>();
		private static float cleanupTimer;
		private static int accumulatedRawDollars;

		internal static void ProcessDrain(EnemyRigidbody enemyRb)
		{
			if (!ConfigurationController.Enabled.Value)
			{
				return;
			}

			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}

			if (enemyRb == null || enemyRb.physGrabObject == null)
			{
				return;
			}

			Enemy enemy = enemyRb.enemy;
			if (enemy == null || enemy.Health == null || enemy.Health.dead || enemy.Health.healthCurrent <= 0)
			{
				return;
			}

			List<PhysGrabber> grabbers = enemyRb.physGrabObject.playerGrabbing;
			if (grabbers == null || grabbers.Count == 0)
			{
				return;
			}

			float deltaTime = Time.deltaTime;
			float interval = Mathf.Max(0.1f, ConfigurationController.TickIntervalSeconds.Value);

			foreach (PhysGrabber grabber in grabbers)
			{
				if (grabber == null)
				{
					continue;
				}

				PlayerAvatar player = grabber.playerAvatar;
				if (player == null || player.isDisabled || player.playerHealth == null)
				{
					continue;
				}

				if (!ConfigurationController.AllowDrainWhileStunned.Value && player.isTumbling)
				{
					continue;
				}

				int maxAllowedHealth = ConfigurationController.AllowOverheal.Value
					? ConfigurationController.MaxHealthCap.Value
					: player.playerHealth.maxHealth;

				long interactionKey = ((long)enemy.GetInstanceID() << 32) ^ (long)player.GetInstanceID();

				if (!drainTimers.TryGetValue(interactionKey, out float currentTimer))
				{
					currentTimer = 0f;
				}

				currentTimer += deltaTime;

				if (currentTimer >= interval)
				{
					currentTimer = 0f;
					ExecuteDrain(enemy, player, maxAllowedHealth);
				}

				drainTimers[interactionKey] = currentTimer;
			}

			cleanupTimer += deltaTime;
			if (cleanupTimer > 30f)
			{
				cleanupTimer = 0f;
				CleanupStaleTimers();
			}
		}

		private static void ExecuteDrain(Enemy enemy, PlayerAvatar player, int maxAllowedHealth)
		{
			int drainAmount;
			if (ConfigurationController.DrainMode.Value == DrainMode.Percentage)
			{
				float pct = Mathf.Clamp(ConfigurationController.DrainPercentage.Value, 1, 100) / 100f;
				drainAmount = Mathf.Max(1, Mathf.RoundToInt(enemy.Health.health * pct));
			}
			else
			{
				drainAmount = Mathf.Max(0, ConfigurationController.DrainFixedAmount.Value);
			}

			// Do not drain more than monster has
			drainAmount = Mathf.Min(drainAmount, enemy.Health.healthCurrent);
			if (drainAmount <= 0)
			{
				return;
			}

			// Apply full damage to enemy (replicates via HurtRPC)
			enemy.Health.Hurt(drainAmount, Vector3.zero);

			// Calculate how much the player can absorb without exceeding allowed cap
			int missingPlayerHealth = Mathf.Max(0, maxAllowedHealth - player.playerHealth.health);
			int healAmount = Mathf.Min(drainAmount, missingPlayerHealth);

			// Apply heal to player (replicates via UpdateHealthRPC)
			bool showEffect = ConfigurationController.EnableAudioVisualFeedback.Value;
			if (healAmount > 0)
			{
				player.playerHealth.HealOther(healAmount, effect: showEffect);
			}

			// Play drain feedback beam/pulse even if player health was already full
			if (showEffect)
			{
				player.HealedOther();
			}

			// Full Health Money Conversion
			if (ConfigurationController.EnableFullHealthMoneyConversion.Value)
			{
				int drainToConvert = 0;
				if (player.playerHealth.health >= player.playerHealth.maxHealth)
				{
					// Player was already at full health: all damage dealt converts to money
					drainToConvert = drainAmount;
				}
				else if (player.playerHealth.health + healAmount >= player.playerHealth.maxHealth)
				{
					// Player reached full health during this tick: excess drain converts to money
					drainToConvert = drainAmount - healAmount;
				}

				if (drainToConvert > 0)
				{
					int multiplier = Mathf.Clamp(ConfigurationController.FullHealthMoneyMultiplier.Value, 0, 300);
					long rawDollarsGained = (long)drainToConvert * multiplier;

					if (rawDollarsGained > 0)
					{
						accumulatedRawDollars += (int)Math.Min(rawDollarsGained, (long)int.MaxValue - accumulatedRawDollars);
						int currencyUnitsToAdd = accumulatedRawDollars / 1000;

						if (currencyUnitsToAdd > 0)
						{
							accumulatedRawDollars %= 1000;

							int currentCurrency = SemiFunc.StatGetRunCurrency();
							int currentTotalHaul = SemiFunc.StatGetRunTotalHaul();
							int maxCap = ConfigurationController.MaxCurrencyCap.Value;

							long maxAllowed = Math.Min((long)maxCap, (long)int.MaxValue);
							long safeRoom = Math.Max(0L, maxAllowed - currentCurrency);
							int moneyToAdd = (int)Math.Min((long)currencyUnitsToAdd, safeRoom);

							if (moneyToAdd > 0)
							{
								int newCurrency = currentCurrency + moneyToAdd;
								SemiFunc.StatSetRunCurrency(newCurrency);

								long roomTotalHaul = Math.Max(0L, (long)int.MaxValue - currentTotalHaul);
								int totalHaulToAdd = (int)Math.Min((long)moneyToAdd, roomTotalHaul);
								SemiFunc.StatSetRunTotalHaul(currentTotalHaul + totalHaulToAdd);

								if (CurrencyUI.instance != null)
								{
									CurrencyUI.instance.FetchCurrency();

									bool isCurrentlyShown = CurrencyUI.instance.showTimer > 0f ||
										(CurrencyUI.instance.uiText != null && CurrencyUI.instance.uiText.enabled);

									if (!isCurrentlyShown)
									{
										CurrencyUI.instance.Show();
										CurrencyUI.instance.showTimer = 3f;
									}
								}

								if (ShopIncreaseUI.instance != null)
								{
									ShopIncreaseUI.instance.ShowIncrease(moneyToAdd, 3f);
								}

								StealLifeFromMonstersPlugin.Logger.LogDebug(
									$"Converted {drainToConvert} drain into {moneyToAdd}K currency (${moneyToAdd * 1000}) for {player.playerName} (Current: {newCurrency}K, Mult: {multiplier}x, Remainder: ${accumulatedRawDollars})."
								);
							}
						}
					}
				}
			}

			StealLifeFromMonstersPlugin.Logger.LogDebug(
				$"Drained {drainAmount} HP from {enemy.EnemyParent?.enemyName ?? "Monster"} to {player.playerName} (absorbed {healAmount} HP)."
			);
		}

		private static void CleanupStaleTimers()
		{
			if (drainTimers.Count > 100)
			{
				drainTimers.Clear();
			}
		}

		internal static void Reset()
		{
			drainTimers.Clear();
			cleanupTimer = 0f;
			accumulatedRawDollars = 0;
		}
	}
}
