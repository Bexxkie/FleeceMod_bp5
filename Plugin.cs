using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine.UIElements;
using System;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace FleeceMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string LEGACY_GUID = "com.lily.goldenFleeceFix";
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
            
           /* if(Plugin.Cosmetic.Value)
            {
                return false;
            }*/
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
        public static bool TarotFleecePatch(ref int __result)
        {
            if (Plugin.Cosmetic.Value)
            {
                __result = 0;
                return false;
            }
            //If using default tarot fleece mode
            if (DataManager.Instance.PlayerFleece == 4 && !Plugin.TarotAlt.Value)
            {
                PlayerFleeceManagerPatch.DrawCards(Plugin.TarotCount.Value);
                return false;
            }
            return false ;
        }
        // Tarot fleece patch
        [HarmonyPatch(nameof(PlayerFleeceManager.OnTarotCardPickedUp))]
        [HarmonyPrefix]
        public static bool TarotPickupPatch()
        {
            if (Plugin.Cosmetic.Value)
            {
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerFleeceManager.GetCursesDamageMultiplier))]
        [HarmonyPrefix]
        public static bool CurseDamMultPatch(ref float __result)
        {
            if (Plugin.Cosmetic.Value)
            {
                __result = 1f;
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerFleeceManager.GetCursesFeverMultiplier))]
        [HarmonyPrefix]
        public static bool FeverMultPatch(ref float __result)
        {
            if (Plugin.Cosmetic.Value)
            {
                __result = 1f;
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerFleeceManager.GetWeaponDamageMultiplier))]
        [HarmonyPrefix]
        public static bool WeaponDamMultPatch(ref float __result)
        {
            if (Plugin.Cosmetic.Value)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerFleeceManager.GetDamageReceivedMultiplier))]
        [HarmonyPrefix]
        public static bool DamRecMultPatch(ref float __result)
        {
            if (Plugin.Cosmetic.Value)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerFleeceManager.GetHealthMultiplier))]
        [HarmonyPrefix]
        public static bool HealthMultPatch(ref float __result)
        {
            if (Plugin.Cosmetic.Value)
            {
                __result = 1f;
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerFleeceManager.GetLootMultiplier))]
        [HarmonyPrefix]
        public static bool LootMultPatch(ref float __result)
        {
            if (Plugin.Cosmetic.Value)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerFleeceManager.ResetDamageModifier))]
        [HarmonyPrefix]
        public static void ResetDamModPatch()
        {
            Plugin._log.LogError(DataManager.Instance.PlayerFleece.ToString());
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
            if (DataManager.Instance.PlayerFleece == 1 && Plugin.GoldenAlt.Value && !Plugin.Cosmetic.Value)
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

            if (DataManager.Instance.PlayerFleece == 4 && Plugin.TarotAlt.Value && !Plugin.Cosmetic.Value)
            {
                //modify the upgrade to be max
                TarotCards.Card card = __result.CardType;
                int maxLevel = TarotCards.GetMaxTarotCardLevel(card);
                __result.UpgradeIndex = maxLevel;
            }
        }

    }
    [HarmonyPatch(typeof(Interaction_Chest))]
    public class ChestSpawnPatch
    {
        [HarmonyPatch(nameof(Interaction_Chest.Reveal))]
        [HarmonyPostfix]
        public static void OpenChestPatch()
        {
            Plugin.opened++;
            if (DataManager.Instance.PlayerFleece == 4 && Plugin.TarotAlt.Value && !Plugin.Cosmetic.Value && Plugin.opened % 2 != 0)
            { 
                PlayerFleeceManagerPatch.DrawCards(1);
            }

        }

    }
    [HarmonyPatch(typeof(DataManager))]
    public class DataManagerMenuPatch
    {
        [HarmonyPatch(nameof(DataManager.SetNewRun))]
        [HarmonyPostfix]
        public static void SetNewRunPatch()
        {

            if (Plugin.Cosmetic.Value)
            {
                DataManager.Instance.PlayerFleece = 0;
            }
            
        }
    }

    [HarmonyPatch(typeof(PlayerFarming), nameof(PlayerFarming.SetSkin))]
    [HarmonyPatch(new Type[] {typeof(bool)})]
    public class PlayerFarmingPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if(!Plugin.Cosmetic.Value)
            {
                return new CodeMatcher(instructions)
                    .InstructionEnumeration();
            }
            return new CodeMatcher(instructions)
                .Advance(offset: 10)
                .RemoveInstructionsInRange(9, 11)
                //.Set(OpCodes.Ldc_I4, Plugin.displayFleece)
                .Insert(new CodeInstruction(OpCodes.Ldstr, Plugin._displayFleece.ToString()))
                .InstructionEnumeration();

        }
    }

}