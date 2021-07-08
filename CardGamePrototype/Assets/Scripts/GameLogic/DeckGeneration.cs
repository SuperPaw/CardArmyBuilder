﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLogic
{

    public abstract class DeckGeneration
    {
        //test if this list persists through plays
        public static HashSet<Creature> UniquesGenerated = new HashSet<Creature>();

        public static Deck GenerateDeck(int CR, List<Race> races = null, List<Creature> creatures = null, bool uniquesAllowed = false)
        {
            var possibleRaces = races ?? CreatureLibrary.Instance.AllRaces.OrderBy(c=>Random.value).ToList();
            var race = possibleRaces.First();

            if (creatures == null) creatures = new List<Creature>();

            foreach (var c in creatures.Where(c => c.Rarity == Creature.RarityType.Unique))
                UniquesGenerated.Add(c);

            var origCreatures = creatures;

            //TODO: possible for more races together

            if(creatures == null || creatures.Count == 0)
                creatures = CreatureLibrary.Instance.AllCreatures.Where(c => c.Race == race && !c.IsSummon()).ToList();

            var library = new List<Card>();

            if (!uniquesAllowed)
                creatures = creatures.Where(c => c.Rarity != Creature.RarityType.Unique || origCreatures.Contains(c)).ToList();


            var difficultyLeft = CR;

            if (!creatures.Any(c => c.CR <= difficultyLeft) )
                throw new System.Exception("Creatures of type " + creatures.First().Race + " has no creature with CR below " + difficultyLeft);

            int v = (GameSettings.DeckSize());
            for (int i = 0; i < v; i++)
            {
                Creature selected;

                if (!creatures.Any(c => c.CR <= difficultyLeft))
                    break;

                selected = CreatureLibrary.Instance.TakeRandom(creatures,CR);

                var card = new Card(selected);

                difficultyLeft -= selected.CR;

                library.Add(card);

                if (selected.Rarity == Creature.RarityType.Unique) 
                { 
                    UniquesGenerated.Add(selected);
                    creatures.Remove(selected);
                }
            }


            var deck = new Deck(library);

            return deck;
        }

        //TODO: seperate the testcreature generation and general generation
        public static Deck GenerateDeck(int CR, Creature testCreature , HeroObject testHero = null)
        {
            List<Creature> creatures;

            //Generate a deck full of the test creature
                creatures = new List<Creature>() { testCreature };

            var library = new List<Card>();


            var difficultyLeft = CR;

            int v = (GameSettings.DeckSize());
            for (int i = 0; i < v; i++)
            {
                Creature selected;

                selected = creatures[Random.Range(0, creatures.Count())];

                var card = new Card(selected);

                difficultyLeft -= selected.CR;

                library.Add(card);
            }

            var deck = new Deck(library);

            if ( testHero)
            {
                GenerateHero(testHero, deck);
            }

            return deck;
        }

        private static void GenerateHero(HeroObject heroObject, Deck deck)
        {
            var hero = new Hero(heroObject);

            hero.InDeck = deck;

            deck.Hero = hero;
        }
    }

}