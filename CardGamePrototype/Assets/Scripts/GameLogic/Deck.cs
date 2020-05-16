using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace GameLogic
{
    public class Deck
    {
        public enum Zone { Library, Battlefield, Graveyard, Hand, COUNT }
        public DeckObject DeckObject;
        public IDeckController DeckController;

        private Dictionary<Zone, List<Card>> Creatures = new Dictionary<Zone, List<Card>>();

        public Deck(DeckObject deckObject)
            : this(deckObject.Creatures.Select(c => new Card(c)).ToList())
        {
            DeckObject = deckObject;
        }

        public Deck(List<Card> initialLibrary)
        {
            for (int i = (int)Zone.Library; i < (int)Zone.COUNT; i++)
            {
                Creatures[(Zone)i] = new List<Card>();
            }
            
            foreach (var card in initialLibrary)
            {
                AddCard(card);
            }

            //if (!playerDeck)
            //    AI = new AI(this);

        }

        public void AddCard(Card card)
        {
            card.InDeck = this;
            card.Location = Zone.Library;

            Creatures[Zone.Library].Add(card);
        }

        public List<Card> CreaturesInZone(Zone z)
            => Creatures[z];

        internal void DrawInitialHand(bool enemy = false)
        {
            ShuffleLibrary();

            var amountToDraw = GameSettings.Instance.StartingHandSize;

            //Move AVANTGARDE cards to the front and shuffle the rest
            while (CreaturesInZone(Zone.Library).Any(c => c.Avantgarde()))
            {
                Draw(CreaturesInZone(Zone.Library).First(c => c.Avantgarde()));
                amountToDraw--;
            }


            Draw(amountToDraw);
        }


        internal void PackUp()
        {
            //removing dead creatures
            while (Creatures[Zone.Graveyard].Any(c=>!c.Deathless()))
                Remove(Creatures[Zone.Graveyard].First(c => !c.Deathless()));

            foreach (var c in AllCreatures())
            {
                c.ResetAfterBattle();
            }
        }

        public void ShuffleLibrary()
        {
            Creatures[Zone.Library] = Creatures[Zone.Library].OrderBy(x => Random.value).ToList();
        }

        public void Draw(int amount)
        {
            if (Creatures[Zone.Library].Count() == 0 || amount < 0) return;
            if (Creatures[Zone.Library].Count() < amount) amount = Creatures[Zone.Library].Count();

            var draws = Creatures[Zone.Library].Take(amount).ToArray();

            foreach (var card in draws)
            {
                Draw(card);
            }
        }

        public void Draw(Card card)
        {
            card.ChangeLocation(Deck.Zone.Library, Deck.Zone.Hand);
        }

        public List<Card> AllCreatures()
        {
            return Creatures.SelectMany(x => x.Value).ToList();
        }

        //returns count of all creatures not in Graveyard
        public int Alive() => Creatures.Sum(a => a.Key == Zone.Graveyard ? 0 : a.Value.Count);

        internal Card GetAttackTarget()
        {
            //empty list check?
            if (CreaturesInZone(Zone.Battlefield).Count > 0)
            {
                List<Card> battlefield = CreaturesInZone(Zone.Battlefield).Where(c => !c.Ethereal()).ToList();
                var defenders = battlefield.Where(c => c.Defender()).ToList();

                if (defenders.Any())
                    return defenders[Random.Range(0, defenders.Count())];

                if (!battlefield.Any()) //meaning only ethereals
                    return CreaturesInZone(Zone.Battlefield)[Random.Range(0, CreaturesInZone(Zone.Battlefield).Count)];

                return battlefield[Random.Range(0, battlefield.Count())];
            }
            else if (CreaturesInZone(Zone.Hand).Count > 0)
            {
                return CreaturesInZone(Zone.Hand)[Random.Range(0, CreaturesInZone(Zone.Hand).Count())];
            }
            else if (CreaturesInZone(Zone.Library).Count > 0)
            {
                return CreaturesInZone(Zone.Library)[Random.Range(0, CreaturesInZone(Zone.Library).Count())];
            }
            else return null;
        }

        internal void Remove(Card card)
        {
            Creatures[card.Location].Remove(card);
            card.InDeck = null;
        }

        internal void Add(Card card)
        {
            Creatures[card.Location].Add(card);
        }
    }
}