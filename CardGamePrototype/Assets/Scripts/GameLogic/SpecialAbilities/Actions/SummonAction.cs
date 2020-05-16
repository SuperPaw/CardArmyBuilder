﻿using System.Collections.Generic;

namespace GameLogic
{
    public class SummonAction : AbilityAction
    {
        public override Ability.ActionType ActionType => Ability.ActionType.Summon;

        public override string Description(string target, int amount, Creature summon)
        {
            return $"summon a {summon.Attack}/{summon.Health} {summon.name}" ;
        }

        public override void ExecuteAction(Ability ability, Card owner, List<Card> targets)
        {
            var summon = ability.ResultingAction.Summons;

            Event.OnAbilityTrigger.Invoke(ability, owner, targets);

            if (owner == null || owner.InDeck == null || !ability.ResultingAction.Summons)
                return;
            Summon(ability.ResultingAction.Summons, owner);
        }

        private static void Summon(Creature summon, Card owner)
        {
            var card = new Card(summon);

            owner.InDeck.AddCard(card);

            Event.OnSummon.Invoke(card);

            card.ChangeLocation(Deck.Zone.Battlefield);

        }

        public override float GetValue(float targetValue, int amount)
        {
            return targetValue;
        }
    }
}