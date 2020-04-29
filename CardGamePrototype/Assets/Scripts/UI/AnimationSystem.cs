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


        private IEnumerator PlayCardFX(CardUI card, ParticleSystem[] fxs, float delay = 0, bool instantiateInWorldSpace = false)
        {
            yield return new WaitForSeconds(delay);

            if (!card) yield break;
            //vector2 to ignore z position to prevent oddities
            Vector2 position = card.transform.position;
            PlayFx(fxs, position, instantiateInWorldSpace ? null : card.transform);
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

        private void PlayAbilityFx(Ability ability, CardUI owner, List<CardUI> targets, float delay = 0)
        {
            var abilityFx = AbilityFx.First(a => a.ActionType == ability.ResultingAction.ActionType);

            int i = 0;

            StartCoroutine(PlayCardFX(owner, abilityFx.OwnerFX, delay * i++));
            StartCoroutine(PlayAbilityIconFx(owner, abilityFx.AbilityIconFX, delay * i++));
            foreach (var t in targets)
                StartCoroutine(PlayCardFX(t, abilityFx.TargetFX, delay * i++));
        }
    }
}