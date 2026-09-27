using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace StealLifeFromMonsters
{
	[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
	public class StealLifeFromMonstersPlugin : BaseUnityPlugin
	{
		// Se reemplaza com.osmar por el parÃ¡metro AuthorId del template.json
		// y StealLifeFromMonsters por el nombre del proyecto (sourceName).
		public const string PluginGuid = "com.osmar.StealLifeFromMonsters";
		public const string PluginName = "StealLifeFromMonsters";
		public const string PluginVersion = "1.0.0";

		internal Harmony? Harmony { get; set; }
		internal static new BepInEx.Logging.ManualLogSource Logger { get; private set; } = null!;

		private void Awake()
		{
			Logger = base.Logger;

			// Prevent the plugin from being deleted
			this.gameObject.transform.parent = null;
			this.gameObject.hideFlags = HideFlags.HideAndDontSave;

			ConfigurationController.Initialize(this.Config);

			Harmony = new Harmony(Info.Metadata.GUID);
			Harmony.PatchAll();

			Logger.LogInfo($"{PluginName} {PluginVersion} loaded!");
		}

		internal void Unpatch()
		{
			Harmony?.UnpatchSelf();
		}
	}
}
