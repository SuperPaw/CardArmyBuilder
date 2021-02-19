﻿
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLogic;
using Event = GameLogic.Event;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;

namespace MapLogic
{

    public class Shop 
    {
        public Race VillageType;
        public Deck Visitor;
        public List<System.Tuple<Creature,int>> OnOffer = new List<System.Tuple<Creature, int>>();
        public int RerollsLeft;
        public int RerollPrice;


        //shop Events
        public class ShopEvent : UnityEvent<Shop> { }
        public static ShopEvent OnShopOpen = new ShopEvent();
        public static ShopEvent OnShopReroll = new ShopEvent();

        public Shop(Race villageType)
        {
            VillageType = villageType;

            RerollsLeft = ShopOptions.Instance.Rerolls;

            RerollPrice += ShopOptions.Instance.RerollInitialCost;

            SetupOptions();

            OnShopOpen.Invoke(this);
        }

        public static void ResetEvents()
        {
            OnShopOpen.RemoveAllListeners();
            OnShopReroll.RemoveAllListeners();
        }

        public void Reroll()
        {
            if (RerollsLeft-- == 0 || RerollPrice > MapController.Instance.PlayerGold)
                throw new System.ArgumentException("No rerolls left");

            MapController.Instance.PlayerGold -= RerollPrice;

            RerollPrice += ShopOptions.Instance.RerollCostIncrease;

            SetupOptions();

            OnShopReroll.Invoke(this);
        }

        private void SetupOptions()
        {
            OnOffer.Clear();

            foreach (var choice in ShopOptions.Instance.Options)
            {
                SetupChoice(choice.Options[Random.Range(0, choice.Options.Count)]);
            }
        }

        private void SetupChoice(ShopOptionType choice)
        {
            Creature forSale = null;
            int stop = 10;

            int price = 0 ;

            while ( (forSale == null || OnOffer.Any(a=> a.Item1 ==  forSale)) && stop-- > 0)
            {
                if (choice == ShopOptionType.OwnerRace && VillageType)
                    forSale = CreatureLibrary.Instance.GetCreature(VillageType);
                else if (choice == ShopOptionType.FriendRace && VillageType && VillageType.FriendRaces.Length > 0)
                    forSale = CreatureLibrary.Instance.GetCreature(VillageType.FriendRaces[Random.Range(0, VillageType.FriendRaces.Length)]);
                else
                    forSale = CreatureLibrary.Instance.GetShopCreature();

                price = (int)( forSale.CR * Random.Range(0.5f, 1.2f));


                if (forSale.Rarity == Creature.RarityType.Unique) price *= 3;
                if (forSale.Rarity == Creature.RarityType.Rare) price *= 2;
            }

            if(!OnOffer.Any(a => a.Item1 == forSale))
                OnOffer.Add(new System.Tuple<Creature, int>(forSale, price));
        }

        //returns whether or not the player bought it
        public bool Buy(Creature card)
        {
            var sale = OnOffer.Single(o => o.Item1 == card);

            if(MapController.Instance.PlayerGold >= sale.Item2)
            {
                OnOffer.Remove(sale);

                BattleManager.Instance.PlayerDeck.AddCard(new Card(sale.Item1));

                MapController.Instance.PlayerGold -= sale.Item2;

                return true;
            }

            return false;

        }
    }
}