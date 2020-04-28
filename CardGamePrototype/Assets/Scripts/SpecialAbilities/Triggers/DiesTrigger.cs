﻿using UnityEngine.Events;

namespace GameLogic
{
    public class DiesTrigger : AbilityTrigger
    {
        public override Ability.Verb TriggerType => Ability.Verb.DIES;

        internal override string Description(string instigatorString)
        {
            return $"When {instigatorString } dies";
        }

        internal override float GetValue()
        {
            return 1f;
        }

        internal override void SetupListener(Card owner, Noun subjekt, UnityAction<Card, Card, Noun> executeIfTrue)
        {
            Event.OnDeath.AddListener(a => executeIfTrue.Invoke(a, owner, subjekt));
        }
        internal override void RemoveListener(Card owner, Noun subjekt, UnityAction<Card, Card, Noun> executeIfTrue)
        {
            Event.OnDeath.RemoveListener(a => executeIfTrue.Invoke(a, owner, subjekt));
        }
    }
}