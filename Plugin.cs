using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;

namespace FleeceMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string LEGACY_GUID = "com.lily.goldenFleeceFix";
        public const string GUID = "goldenFleeceFix";
        public const string NAME = "Golden Fleece Fix";
        public const string VERSION = "1.2";

        public static ManualLogSource _log;

        public static ConfigEntry<float> IncrementValue;
        // INIT...
        private void Awake()
        {
            _log = new ManualLogSource("fleeceMod-Log");
            BepInEx.Logging.Logger.Sources.Add(_log);

            IncrementValue = Config.Bind("General", "IncDamage", 0.1f, "Set damage increase per kill");
            // Apply all the patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            _log.LogInfo($"Fleecemod loaded");
        }
    }
    // --Patches--
    //  Golden fleece patch
    [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.IncrementDamageModifier))]
    public class PlayerFleeceManagerPatch
    {
        [HarmonyPrefix]
        public static bool goldenFleecePatch(ref float ___damageMultiplier)
        {
            if (DataManager.Instance.PlayerFleece == 1)
            {
                ___damageMultiplier += Plugin.IncrementValue.Value;
                PlayerFleeceManager.OnDamageMultiplierModified?.Invoke(___damageMultiplier);
                return false;
            }
            Plugin._log.LogInfo($"GoldenFleece patched");
            return false;
        }

    }
}