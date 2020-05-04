﻿using GameLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Event = GameLogic.Event;

namespace UI
{
    public class AnimationSystem : Singleton<AnimationSystem>
    {
        public AnimationCurve AttackAnimationCurve;

        public ParticleSystem[] WithdrawParticlesPrefab;
        public ParticleSystem[] ETBParticlesPrefab;
        public ParticleSystem[] DamageParticlesPrefab;
        public ParticleSystem[] DeathParticlesPrefab;
        public AbilityAnimationFX[] AbilityFx;

        [Serializable]
        public struct AbilityAnimationFX
        {
            public Ability.ActionType ActionType;
            public ParticleSystem[] AbilityIconFX;
            public ParticleSystem[] TargetFX;
            public ParticleSystem[] OwnerFX;
        }

        private void Start()
        {
            //Event.OnWithdraw.AddListener(c => StartCoroutine(PlayCardFX(c, WithdrawParticlesPrefab, 0, true)));
            //Event.OnPlay.AddListener(c => StartCoroutine(PlayCardFX(c, ETBParticlesPrefab, BattleUI.Instance.MoveDuration + 0.1f)));
            //Event.OnDeath.AddListener(c => StartCoroutine(PlayCardFX(c, DeathParticlesPrefab, 0.1f)));
            //Event.OnDamaged.AddListener(c => StartCoroutine(PlayCardFX(c, DamageParticlesPrefab)));

            //Event.OnAbilityTrigger.AddListener((a, c, ts) => PlayAbilityFx(a, c, ts, 0.25f));
        }

        public static IEnumerator AttackAnimation(CardUI owner, CardUI target, float duration)
        {
            var rect = owner.GetComponent<RectTransform>();
            var startPos = rect.position;
            var endPos = target.GetComponent<RectTransform>();


            var startTime = Time.time;

            while (Time.time < startTime + duration)
            {
                yield return null;

                rect.position = Vector3.LerpUnclamped(startPos, endPos.position, AnimationSystem.Instance.AttackAnimationCurve.Evaluate((Time.time - startTime) / duration));
            }
        }

        public void WithdrawParticles(CardUI cardUI)
        {
            StartCoroutine(PlayCardFX(cardUI, WithdrawParticlesPrefab, 0, true));
        }
        //Event.OnWithdraw.AddListener(c => StartCoroutine(PlayCardFX(c, WithdrawParticlesPrefab, 0, true)));
        //Event.OnPlay.AddListener(c => StartCoroutine(PlayCardFX(c, ETBParticlesPrefab, BattleUI.Instance.MoveDuration + 0.1f)));
        public void PlayParticles(CardUI cardUI)
        {
            StartCoroutine(PlayCardFX(cardUI, ETBParticlesPrefab, BattleUI.Instance.MoveDuration + 0.1f));
        }
        //Event.OnDeath.AddListener(c => StartCoroutine(PlayCardFX(c, DeathParticlesPrefab, 0.1f)));
        public void DeathParticles(CardUI cardUI)
        {
            StartCoroutine(PlayCardFX(cardUI, DeathParticlesPrefab, 0.1f));
        }
        //Event.OnDamaged.AddListener(c => StartCoroutine(PlayCardFX(c, DamageParticlesPrefab)));
        public void DamageParticles(CardUI c)
        {
            StartCoroutine(PlayCardFX(c, DamageParticlesPrefab));
        }

        private IEnumerator PlayCardFX(CardUI card, ParticleSystem[] fxs, float delay = 0, bool instantiateInWorldSpace = false)
        {
            yield return new WaitForSeconds(delay);

            if (!card) yield break;
            //vector2 to ignore z position to prevent oddities
            Vector2 position = card.transform.position;
            PlayFx(fxs, position, instantiateInWorldSpace ? null : card.transform);
        }

        internal static IEnumerator ZoneMoveEffects(CardUI card, Deck.Zone from, Deck.Zone to)
        {
            switch (to)
            {
                case Deck.Zone.Library:
                    Instance.WithdrawParticles(card);
                    break;
                case Deck.Zone.Battlefield:
                    if(from == Deck.Zone.Graveyard)
                        yield return card.CardAnimation.UnDissolve();
                    Instance.PlayParticles(card);
                    break;
                case Deck.Zone.Graveyard:
                    yield return card.CardAnimation.Dissolve();
                    Instance.DeathParticles(card);
                    break;
            }
        }

        private IEnumerator PlayAbilityIconFx(CardUI abilityOwner, ParticleSystem[] fxs, float delay = 0)
        {
            yield return new WaitForSeconds(delay);

            if (!abilityOwner || !abilityOwner.CardAnimation.SpecialAbilityIcon) yield break;
            abilityOwner.CardAnimation.HighlightAbility();
            //vector2 to ignore z position to prevent oddities
            Vector2 position = abilityOwner.CardAnimation.SpecialAbilityIcon.transform.position;
            PlayFx(fxs, position, abilityOwner.CardAnimation.SpecialAbilityIcon.transform);

        }

        internal static IEnumerator StartAttack(CardUI ui)
        {
            yield return new WaitForSeconds(0.5f);


        }

        private static void PlayFx(ParticleSystem[] fxs, Vector2 position, Transform parent)
        {
            foreach (var fx in fxs)
            {
                if (parent)
                    Instantiate(fx, position, parent.rotation).transform.SetParent(parent);
                else
                    Instantiate(fx, position, fx.transform.localRotation);//.transform.SetParent(parent);

            }
        }

        public IEnumerator PlayAbilityFx(Ability ability, CardUI owner, List<CardUI> targets, float delay = 0)
        {
            var abilityFx = AbilityFx.First(a => a.ActionType == ability.ResultingAction.ActionType);

            int i = 0;

            yield return PlayCardFX(owner, abilityFx.OwnerFX, delay * i++);
            yield return PlayAbilityIconFx(owner, abilityFx.AbilityIconFX, delay * i++);
            foreach (var t in targets)
                yield return PlayCardFX(t, abilityFx.TargetFX, delay * i++);
        }
    }
}