using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FleeceMod
{
    //Fleecemanager overrides
    [HarmonyPatch(typeof(PlayerFleeceManager))]
    public class FleecePatch
    {
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


        [HarmonyPatch(nameof(PlayerFleeceManager.GetFreeTarotCards))]
        [HarmonyPrefix]
        public static bool TarotFleecePatch(ref int __result)
        {
            //If using default tarot fleece mode
            if (DataManager.Instance.PlayerFleece == 4 && !Plugin.TarotAlt.Value)
            {
                Helpers.DrawCards(Plugin.TarotCount.Value);
                return false;
            }
            return false;
        }
    }

    //Instakill player - goldenFleeceAlt
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

    //Spawn tarot -whiteFleeceAlt
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
                Helpers.DrawCards(1);
            }
        }
    }
    //Spawn tarot at spawn -makeMaxLevel
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

    //
    // cosmetic mode patches
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
            if (Plugin.TarotAlt.Value)
            {
                DataManager.Instance.CanFindTarotCards = true;
            }
        }

    }
    // this isnt great but it'll do so long as no one else needs this
    [HarmonyPatch(typeof(PlayerFarming), nameof(PlayerFarming.SetSkin))]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public class PlayerFarmingPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (!Plugin.Cosmetic.Value)
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