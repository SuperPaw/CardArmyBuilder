﻿using System.Collections.Generic;

namespace GameLogic
{
    public class WithdrawAction : AbilityAction
    {
        public override PassiveAbility.ActionType ActionType => PassiveAbility.ActionType.Withdraw;

        public override string Description(string target, int amount, Creature summon)
        {
            return "withdraw " + target;
        }

        public override void ExecuteAction(Ability ability, IAbilityHolder owner, List<Card> targets)
        {
            Event.OnAbilityExecution.Invoke(ability, owner, targets);
            targets.ForEach(c => c.Withdraw());
        }

        public override float GetValue(float targetValue, int amount)
        {
            return targetValue;
        }
    }
}