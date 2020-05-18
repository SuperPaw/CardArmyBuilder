﻿using System.Collections.Generic;

namespace GameLogic
{
    public class DrawAction : AbilityAction
    {
        public override PassiveAbility.ActionType ActionType => PassiveAbility.ActionType.Draw;

        public override string Description(string target, int amount, Creature summon)
        {
            return $"draw {amount} cards";
        }

        public override void ExecuteAction(PassiveAbility ability, Card owner, List<Card> targets)
        {

            Event.OnAbilityTrigger.Invoke(ability, owner, new List<Card>());
            owner.InDeck.Draw(ability.ResultingAction.Amount);
        }

        public override float GetValue(float targetValue, int amount)
        {
            return 1f * amount;
        }
    }
}