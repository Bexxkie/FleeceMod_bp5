﻿using HarmonyLib;
using Lamb.UI;
using Lamb.UI.FollowerInteractionWheel;
using Rewired;
using RewiredConsts;
using src.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FleeceMod
{
    internal class Helpers
    {
        internal static InventoryItem.ITEM_TYPE GetCardCategory(TarotCards.Card Type)
        {
            if (Type - TarotCards.Card.Sword <= 3)
            {
                return InventoryItem.ITEM_TYPE.WEAPON_CARD;
            }
            if (Type - TarotCards.Card.Fireball > 1)
            {
                return InventoryItem.ITEM_TYPE.TRINKET_CARD;
            }
            return InventoryItem.ITEM_TYPE.CURSE_CARD;
        }

        public static List<TarotCards.TarotCard> DrawCards(int cardCount)
        {
            List<TarotCards.TarotCard> cards = new List<TarotCards.TarotCard>();
            if (cardCount > TarotCards.TarotCardsUnlockedCount())
            {
                cardCount = TarotCards.TarotCardsUnlockedCount();
            }
            for (int i = 0; i < cardCount; i++)
            {
                TarotCards.TarotCard card = TarotCards.DrawRandomCard();
                DataManager.Instance.PlayerRunTrinkets.Add(card);
                TrinketManager.AddTrinket(card);
                NotificationCentre.Instance.PlayGenericNotification($" +{TarotCards.LocalisedName(card.CardType)}");
                //cards.Add(card);                
            }
            return cards;
        }

      

    }
}
