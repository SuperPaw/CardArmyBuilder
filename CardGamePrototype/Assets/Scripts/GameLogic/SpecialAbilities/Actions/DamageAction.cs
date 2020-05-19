﻿using System.Collections.Generic;

namespace GameLogic
{
    public class DamageAction : AbilityAction
    {
        public override PassiveAbility.ActionType ActionType => PassiveAbility.ActionType.DealDamage;

        public override string Description(string target, int amount, Creature summon)
        {
            return $"Deal {amount} damage to {target}";
        }

        public override void ExecuteAction(Ability ability, IAbilityHolder owner, List<Card> targets)
        {

            Event.OnAbilityExecution.Invoke(ability, owner, targets);

            targets.ForEach(c => c.HealthChange(-ability.ResultingAction.Amount));
        }

        public override float GetValue(float targetValue, int amount)
        {
            return -1f * targetValue * (1 + amount / 20f);
        }
    }
}