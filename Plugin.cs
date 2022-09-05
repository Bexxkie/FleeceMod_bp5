using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using RewiredConsts;
using System.Runtime.Remoting.Messaging;

namespace FleeceMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string LEGACY_GUID = "com.lily.goldenFleeceFix";
        public const string GUID = "goldenFleeceFix";
        public const string NAME = "Golden Fleece Fix";
        public const string VERSION = "1.4";

        public static ManualLogSource _log;

        public static ConfigEntry<float> IncrementValue;
        public static ConfigEntry<int> TarotCount;
        public static ConfigEntry<bool> TarotAlt;
        public static ConfigEntry<bool> GoldenAlt;
        public static int opened = 0;
        // INIT...
        private void Awake()
        {
            _log = new ManualLogSource(" fleeceMod-Log");
            BepInEx.Logging.Logger.Sources.Add(_log);

            IncrementValue = Config.Bind("General", "IncDamage", 0.1f, "Set damage increase per kill");
            TarotCount = Config.Bind("General", "TarotCount", 4, "Set amount of tarot's recieved");
            TarotAlt = Config.Bind("General", "TarotAlt", false, "Alternative tarot fleece mode -recieve max level card every second chest instead of x at start");
            GoldenAlt = Config.Bind("General", "GoldenAlt", false, "Alternative golden fleece mode -one hit mode");
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
        public static bool GoldenFleecePatch(ref float ___damageMultiplier)
        {
            if (DataManager.Instance.PlayerFleece == 1)
            {
                ___damageMultiplier += Plugin.IncrementValue.Value;
                PlayerFleeceManager.OnDamageMultiplierModified?.Invoke(___damageMultiplier);
                return false;

            }
            return true;
        }
        // Tarot fleece patch
        [HarmonyPatch(nameof(PlayerFleeceManager.GetFreeTarotCards))]
        [HarmonyPrefix]
        public static bool TarotFleecePatch()
        {
            //If using default tarot fleece mode
            if (DataManager.Instance.PlayerFleece == 4 && !Plugin.TarotAlt.Value)
            {
                PlayerFleeceManagerPatch.DrawCards(Plugin.TarotCount.Value);
                return false;
            }
            return false ;
        }

        public static void DrawCards(int cardCount)
        {
            if (cardCount > TarotCards.TarotCardsUnlockedCount())
            { 
                cardCount = TarotCards.TarotCardsUnlockedCount();
            }
            for(int i = 0; i<cardCount; i++)
            {
                TarotCards.TarotCard card = TarotCards.DrawRandomCard();
                DataManager.Instance.PlayerRunTrinkets.Add(card);
                TrinketManager.AddTrinket(card);
            }
        }
    }
    [HarmonyPatch(typeof(PlayerController))]
    public class PlayerControllerPatch
    {
        [HarmonyPatch("OnHit")]
        [HarmonyPrefix]
        public static bool InstakillPlayerPatch()
        {
            if (DataManager.Instance.PlayerFleece == 1 && Plugin.GoldenAlt.Value)
            {
                DataManager.Instance.PLAYER_HEALTH = 0f;
                DataManager.Instance.PLAYER_BLACK_HEARTS = 0f;
                DataManager.Instance.PLAYER_BLUE_HEARTS = 0f;
                DataManager.Instance.PLAYER_TOTAL_HEALTH = 0;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(TarotCards))]
    public class CardLevelPatch
    {
        [HarmonyPatch(nameof(TarotCards.DrawRandomCard))]
        [HarmonyPostfix]
        public static void RandomCardPatch(ref TarotCards.TarotCard __result)
        {

            if (DataManager.Instance.PlayerFleece == 4 && Plugin.TarotAlt.Value)
            {
                //modify the upgrade to be max
                TarotCards.Card card = __result.CardType;
                int maxLevel = TarotCards.GetMaxTarotCardLevel(card);
                __result.UpgradeIndex = maxLevel;
            }
        }

    }
    [HarmonyPatch(typeof(Interaction_Chest))]
    public static class ChestSpawnPatch
    {
        [HarmonyPatch(nameof(Interaction_Chest.Reveal))]
        [HarmonyPostfix]
        public static void OpenChestPatch()
        {

            if (Plugin.opened % 2 != 0)
            {
                PlayerFleeceManagerPatch.DrawCards(1);
            }
        }

    }


}