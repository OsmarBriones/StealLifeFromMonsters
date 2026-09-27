using HarmonyLib;
using StealLifeFromMonsters.Drain;

namespace StealLifeFromMonsters.Patches
{
	[HarmonyPatch(typeof(RoundDirector), "StartRoundLogic")]
	internal static class RoundDirector_StartRoundLogic_Patch
	{
		[HarmonyPostfix]
		private static void Postfix()
		{
			MonsterLifeDrainController.Reset();
		}
	}
}
