﻿using UnityEngine.Events;

namespace GameLogic
{
    public class DamageTrigger : AbilityTrigger
    {
        public override Ability.Verb TriggerType => Ability.Verb.IsDAMAGED;

        internal override string Description(string instigatorString)
        {
            return $"When {instigatorString } is damaged";
        }

        internal override float GetValue()
        {
            return 3f;
        }

        internal override UnityAction SetupListener(Card owner, Noun subjekt, UnityAction<Card, Card, Noun> executeIfTrue)
        {
            UnityAction<Card, int> handler = (a, i) => executeIfTrue.Invoke(a, owner, subjekt);

            Event.CardValueEvent trigger = Event.OnDamaged;

            trigger.AddListener(handler);

            return () => Event.OnDamaged.RemoveListener(handler);
        }
    }
}