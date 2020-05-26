﻿using GameLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Event = GameLogic.Event;

namespace UI
{
    public class HeroView : Singleton<HeroView>
    {
        public Image HeroImage;
        public TextMeshProUGUI HeroName;
        public TextMeshProUGUI XpText;
        public TextMeshProUGUI RaceText;
        public TextMeshProUGUI ClassText;
        public AbilityUI StartingAbility;
        public Color NormalAbilityColor, NotSelectedColor, UnselectableColor;

        public GameObject Holder;

        public LevelAbility[] LevelAbilities;

        [Serializable]
        public struct LevelAbility
        {
            public AbilityUI Race;
            public AbilityUI Class;
        }

        private void Start()
        {
            Event.OnLevelUpSelection.AddListener(UpdateIcons);
            Event.OnLevelUp.AddListener(UpdateIcons);
        }

        public static void Open(Hero hero)
        {
            Instance.ShowHero(hero);
        }

        private void ShowHero(Hero hero)
        {
            Holder.SetActive(true);

            var raceopt = hero.HeroObject.RaceOption;
            var classopt = hero.HeroObject.Class;

            HeroImage.sprite = hero.HeroObject.Portrait;

            HeroName.text = hero.GetName();

            XpText.text = $" {hero.Xp} / {Hero.LevelCaps[hero.CurrentLevel]}";

            if (raceopt)
                RaceText.text = raceopt.name;

            if (classopt)
                ClassText.text = classopt.name;

            StartingAbility.SetAbility(hero.HeroObject.StartingAbility, hero);

            UpdateIcons(hero);
        }

        private void UpdateIcons(Hero hero)
        {

            var raceopt = hero.HeroObject.RaceOption;
            var classopt = hero.HeroObject.Class;

            for (int i = 0; i < LevelAbilities.Length; i++)
            {
                var opt = LevelAbilities[i];

                if (raceopt && raceopt.Options.Count > i)
                    opt.Race?.SetAbility(raceopt.Options[i], hero);

                if (classopt && classopt.Options.Count > i)
                    opt.Class?.SetAbility(classopt.Options[i], hero);
            }
        }
    }
}