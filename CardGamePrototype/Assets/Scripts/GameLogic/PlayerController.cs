﻿using System;

namespace GameLogic
{
    public class PlayerController : IDeckController
    {
        private Deck ControlledDeck;
        private Action TurnFinished;
        public Hero PlayerHero;

        public int PlayerActionsLeft;

        public void SetupDeckActions(Deck deck, Action onfinish)
        {
            if (ControlledDeck != deck)
            {
                ControlledDeck = deck;

                TurnFinished = onfinish;

                Event.OnPlayerAction.AddListener(UsedAction);
            }

            deck.DrawInitialHand();
        }

        public void UsedAction(Deck deck )
        {
            if (deck != ControlledDeck)
                return;

            PlayerActionsLeft--;

            if (PlayerActionsLeft <= 0 || ControlledDeck.CreaturesInZone(Deck.Zone.Hand).Count == 0)
            {
                TurnFinished.Invoke(); 
            }

        }

        public void YourTurn()
        {
            ControlledDeck.Draw(GameSettings.Instance.DrawPrTurn);

            ResetActions();
        }

        public void ResetActions()
        {
            PlayerActionsLeft = GameSettings.Instance.PlaysPrTurn;

        }

        public bool ActionAvailable()
        {
            return PlayerActionsLeft > 0;
        }

        public int ActionsLeft() => PlayerActionsLeft;
    }
}