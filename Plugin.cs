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
        public static ConfigEntry<int> TarotCount;
        // INIT...
        private void Awake()
        {
            _log = new ManualLogSource("fleeceMod-Log");
            BepInEx.Logging.Logger.Sources.Add(_log);

            IncrementValue = Config.Bind("General", "IncDamage", 0.1f, "Set damage increase per kill");
            TarotCount = Config.Bind("General", "TarotCount", 4, "Set amount of tarot's recieved");
            // Apply all the patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            _log.LogInfo($"Fleecemod loaded");
        }
    }
    // --Patches--
    [HarmonyPatch(typeof(PlayerFleeceManager))]
    public class PlayerFleeceManagerPatch
    {
        //  Golden fleece patch
        [HarmonyPatch(nameof(PlayerFleeceManager.IncrementDamageModifier))]
        [HarmonyPrefix]
        public static bool goldenFleecePatch(ref float ___damageMultiplier)
        {
            if (DataManager.Instance.PlayerFleece == 1)
            {
                ___damageMultiplier += Plugin.IncrementValue.Value;
                PlayerFleeceManager.OnDamageMultiplierModified?.Invoke(___damageMultiplier);
                return false;
            }
            return false;
        }
        // Tarot fleece patch --untested, but should work i assume
        [HarmonyPatch(nameof(PlayerFleeceManager.GetFreeTarotCards))]
        [HarmonyPrefix]
        public static int tarotFleecePatch()
        {
            if (DataManager.Instance.PlayerFleece == 4)
            {
                return Plugin.TarotCount.Value;
            }
            return 0;
        }
    }
}