using System.Collections.Generic;
using UnityEngine;

namespace StealLifeFromMonsters.Drain
{
	internal static class MonsterLifeDrainController
	{
		private static readonly Dictionary<long, float> drainTimers = new Dictionary<long, float>();
		private static float cleanupTimer;

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
				if (player == null || player.isDisabled || player.isTumbling || player.playerHealth == null)
				{
					continue;
				}

				int maxAllowedHealth = ConfigurationController.AllowOverheal.Value
					? ConfigurationController.MaxHealthCap.Value
					: player.playerHealth.maxHealth;

				if (player.playerHealth.health >= maxAllowedHealth)
				{
					continue;
				}

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
				float pct = Mathf.Clamp(ConfigurationController.DrainPercentage.Value, 1f, 100f) / 100f;
				drainAmount = Mathf.Max(1, Mathf.RoundToInt(enemy.Health.health * pct));
			}
			else
			{
				drainAmount = Mathf.Max(1, ConfigurationController.DrainFixedAmount.Value);
			}

			// Do not drain more than monster has
			drainAmount = Mathf.Min(drainAmount, enemy.Health.healthCurrent);

			// Do not heal beyond allowed cap
			int missingPlayerHealth = maxAllowedHealth - player.playerHealth.health;
			drainAmount = Mathf.Min(drainAmount, missingPlayerHealth);

			if (drainAmount <= 0)
			{
				return;
			}

			// Apply damage to enemy (replicates via HurtRPC)
			enemy.Health.Hurt(drainAmount, Vector3.zero);

			// Apply heal to player (replicates via UpdateHealthRPC)
			bool showEffect = ConfigurationController.EnableAudioVisualFeedback.Value;
			player.playerHealth.HealOther(drainAmount, effect: showEffect);

			if (showEffect)
			{
				player.HealedOther();
			}

			StealLifeFromMonstersPlugin.Logger.LogDebug(
				$"Drained {drainAmount} HP from {enemy.EnemyParent?.enemyName ?? "Monster"} to {player.playerName}."
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
		}
	}
}
