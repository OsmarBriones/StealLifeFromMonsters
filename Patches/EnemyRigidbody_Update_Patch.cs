using HarmonyLib;
using StealLifeFromMonsters.Drain;

namespace StealLifeFromMonsters.Patches
{
	[HarmonyPatch(typeof(EnemyRigidbody), "Update")]
	internal static class EnemyRigidbody_Update_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(EnemyRigidbody __instance)
		{
			MonsterLifeDrainController.ProcessDrain(__instance);
		}
	}
}
