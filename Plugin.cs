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
        public const string LEGACY_GUID = "com.lily.cotl.fleecemod";
        public const string GUID = "FleeceMod";
        public const string NAME = "FleeceMod";
        public const string VERSION = "1.6";

        internal static new ManualLogSource _log;

        public static ConfigEntry<float> IncrementValue;
        public static ConfigEntry<int> TarotCount;
        public static ConfigEntry<bool> TarotAlt;
        public static ConfigEntry<bool> GoldenAlt;
        public static ConfigEntry<bool> Cosmetic;
        public static ConfigEntry<int> displayFleece;

        public static int opened = 0;
        public static int _displayFleece = 0;
        public static Plugin instance;
        // INIT...
        private void Awake()
        {
            Plugin._log = base.Logger;
            IncrementValue = Config.Bind("General", "IncDamage", 0.1f, "Set damage increase per kill");
            TarotCount = Config.Bind("General", "TarotCount", 4, "Set amount of tarot's recieved");
            TarotAlt = Config.Bind("General", "TarotAlt", false, "Alternative tarot fleece mode -recieve max level card every second chest instead of x at start");
            GoldenAlt = Config.Bind("General", "GoldenAlt", false, "Alternative golden fleece mode -one hit mode");
            Cosmetic = Config.Bind("Cosmetic", "CosmeticMode", false, "Disable all fleece modifiers, just wear what you like");
            displayFleece = Config.Bind("Cosmetic", "DisplayFleece", 0, "Fleece worn when Cosmetic is 'true' (0-red, 1-gold, 2-green, 3-purple, 4-white, 5-blue)");
            _displayFleece = displayFleece.Value;
            // Apply all the patches
            //Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            _log.LogInfo($"Fleecemod loaded");
        }

        private void OnEnable()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Loaded {GUID}!");
        }

        private void OnDisable()
        {
            Harmony.UnpatchAll();
            Logger.LogInfo($"Unloaded {GUID}!");
        }


    }
    
}