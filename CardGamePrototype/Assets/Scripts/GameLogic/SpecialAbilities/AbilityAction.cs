﻿using System.Collections.Generic;

namespace GameLogic
{
    public abstract class AbilityAction
    {
        public AbilityAction() { }

        public abstract EffectType ActionType { get; }
        public abstract string Description(string v, int amount,Creature summon);
        public abstract void ExecuteAction(AbilityWithEffect ability, AbilityHolder _owner, List<Card> targets);
        public abstract float GetValue(float targetvalue, int amount);


    }
}