﻿using GameLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{

    [RequireComponent(typeof(RectTransform))]
    public class CardUI : AbilityHolderUI, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler, IDragHandler,IEndDragHandler,IBeginDragHandler
    {
        public Creature Creature;

        [Header("UI Refs")]
        public GameObject FrontHolder, CardBackHolder, CardBattleUI;
        public Image[] CardImage;
        public Image RaceInstance;
        //TODO: replace with clickable
        public Image[] AttributeInstances;
        public CardAnimation CardAnimation;
        public TextMeshProUGUI[] AttackText;
        public TextMeshProUGUI[] HealthText;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI DescriptionText;
        public TextMeshProUGUI PriceText;
        
        public List<GameObject> InstantiatedObjects = new List<GameObject>();


        //For deck view and the like
        public bool AlwaysFaceUp;
        public bool Interactable = true;
        public class IntEvent : UnityEvent<int> { }
        public readonly IntEvent OnClick = new IntEvent();

        private IntEvent OnPositionChanged = new IntEvent();

        //Not equal to Card.health, since UI may be behind
        public int HealthValueDisplayed;
        private Color ReducedStatsColor = new Color(0.75f, 0.75f, 0.75f);

        //being dragged? maybe change name
        public bool BeingDragged { get; internal set; }
        
        [HideInInspector]
        public CardLayoutGroup CurrentZoneLayout;
        [HideInInspector]
        public CardLayoutGroup CameFromGroup { get; private set; }

        [HideInInspector]
        public CardLayoutGroup CanTransitionTo;
        //which position those the card have in the ui
        [HideInInspector]
        public int LayoutIndex;

        public enum CardState { FaceUp, FaceDown, Battle }

        public class CardUIEvent : UnityEvent<CardUI> { }
        public static CardUIEvent OnUIDestroyed = new CardUIEvent();


        public void OnDestroy()
        {
            OnUIDestroyed.Invoke(this);
        }

        public void SetCard(Card c)
        {
            UpdateCreature(c.Creature);
            UpdateStats(c.Attack, c.CurrentHealth, c.Damaged());
            OnClick.AddListener(c.Click);
            OnPositionChanged.AddListener(c.PositionChanged);
        }
        public void SetCreature(Creature c)
        {
            UpdateCreature(c);
            UpdateStats(c.Attack, c.Health);

        }

        public void UpdateCreature(Creature creature)
        {
            if (!creature) return;

            Creature = creature;

            if (String.IsNullOrEmpty(creature.name)) creature.name = creature.ToString();

            foreach (var cardImage in CardImage)
                cardImage.sprite = creature.Image;
            if (NameText)
                NameText.text = creature.name;

            name = creature?.name;

            InstantiatedObjects.ForEach(DestroyImmediate);
            InstantiatedObjects.Clear();

            if (DescriptionText)
                DescriptionText.text = "";

            if (RaceInstance)
                if (creature.Race)
                {
                    var instance = Instantiate(RaceInstance, RaceInstance.transform.parent);
                    instance.gameObject.SetActive(true);
                    instance.sprite = creature.Race.Shield;

                    InstantiatedObjects.Add(instance.gameObject);
                }


            foreach (var AttributeInstance in AttributeInstances)
            {
                foreach (var a in creature.Traits)
                {
                    var instance = Instantiate(AttributeInstance, AttributeInstance.transform.parent);
                    instance.gameObject.SetActive(true);
                    instance.sprite = a.Icon;

                    if(DescriptionText)
                        DescriptionText.text += $"<b>{a.name}</b>\n";

                    InstantiatedObjects.Add(instance.gameObject);
                }
            }

            foreach (var AttributeInstance in AttributeInstances)
            {
                if (creature.SpecialAbility is PassiveAbility ability)
                {

                    if (DescriptionText)
                        DescriptionText.text += $"{creature.SpecialAbility.Description(Creature)}\n";

                    var instance = Instantiate(AttributeInstance, AttributeInstance.transform.parent);
                    instance.gameObject.SetActive(true);
                    instance.sprite = IconLibrary.GetAbilityIconSprite(ability.ResultingAction.ActionType);

                    InstantiatedObjects.Add(instance.gameObject);

                    SpecialAbilityIcon.Add(instance);
                }
            }
        }

        public void StatModifier(int amount)
        {
            if (amount > 0)
                CardAnimation.StatPlusAnimation.Show(amount);
            else if (amount < 0)
                CardAnimation.StatMinusAnimation.Show(amount);
        }

        private void UpdateStats(int attack, int health, bool damaged = false)
        {
            if (!Creature) return;

            UpdateHealth(health, damaged);
            UpdateAttack(attack);
        }

        public void UpdateAttack(int attack)
        {
            foreach (var a in AttackText)
            {
                a.text = attack.ToString("N0");

                a.color = Creature.Attack < attack ? Color.green :
                    attack < Creature.Attack ? ReducedStatsColor :
                    Color.white;
            }
        }

        public void UpdateHealth(int health, bool damaged)
        {
            foreach (var h in HealthText)
            {
                h.text = health.ToString("N0");

                h.color = damaged ? Color.red :
                    health > Creature.Health ? Color.green :
                    health < Creature.Health ? Color.gray :
                    Color.white;
            }
        }

        public CardState GetCardState()
        {
            if (FrontHolder && FrontHolder.activeInHierarchy)
                return CardState.FaceUp;
            else if (CardBattleUI && CardBattleUI.activeInHierarchy)
                return CardState.Battle;

            return CardState.FaceDown;
        }



        #region Input Handling
#if true
        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_ANDROID
            if (CardHoverInfo.IsActive()) return;
#endif
            if (!BeingDragged && IsClickable() && Interactable && UIFlowController.Instance.EmptyQueue())
                OnClick.Invoke(0);
        }

        private bool IsClickable()
        {
            return GetCardState() == CardState.FaceUp || GetCardState() == CardState.Battle;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CardAnimation?.TurnOffHighlight();
            CardHoverInfo.Hide();

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (AlwaysFaceUp || (IsClickable()))
            {
                CardHoverInfo.Show(this);
                CardAnimation?.Highlight();
            }

        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!BeingDragged) //if the card is not allowed to be dragged
                return;

            transform.position = eventData.pointerCurrentRaycast.worldPosition;

            CurrentZoneLayout.UpdateDraggedCardPos();

            if (CurrentZoneLayout.HiddenZone)
                transform.SetAsFirstSibling();
            else
                transform.SetAsLastSibling();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            CurrentZoneLayout =  GetComponentInParent<CardLayoutGroup>();
            CameFromGroup = CurrentZoneLayout;

            CanTransitionTo = CurrentZoneLayout?.TransitionsTo;

            if (CurrentZoneLayout && CurrentZoneLayout.CardsAreDraggable && !BattleUI.Instance.UILocked)
            {
                BeingDragged = true;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {

            BeingDragged = false;

            //if we have moved to the other layout group
            if (CameFromGroup != CurrentZoneLayout)
            {
                //Debug.Log($"{name} dragged to {CurrentZoneLayout.CardZone} at pos {LayoutIndex}");
                OnClick.Invoke(LayoutIndex);
            }
            else
                OnPositionChanged.Invoke(LayoutIndex);
            
            //move it into place
            CurrentZoneLayout.MoveCardsToDesiredPositions();
        }

        internal IEnumerator Flip(CardState state, float flipTime = 0.2f)
        {
            
            //already correct face up
            if (state == GetCardState()) yield break;

            if (AlwaysFaceUp) yield break;

            bool flipping = GetCardState() == CardState.FaceDown || state == CardState.FaceDown;

            if (flipping)
            {
                gameObject.LeanScaleX(0, flipTime);

                yield return new WaitForSeconds(flipTime);


                CardBackHolder.SetActive(state == CardState.FaceDown);
                CardBattleUI.SetActive(state == CardState.Battle);
                FrontHolder.SetActive(state == CardState.FaceUp);

                FrontHolder.GetComponent<CanvasGroup>().alpha = 1;

                gameObject.LeanScaleX(1, flipTime);

                yield return new WaitForSeconds(flipTime);
            }
            else
            {
                var from = GetCardState() == CardState.Battle ? CardBattleUI : FrontHolder;
                var to = state == CardState.FaceUp ? FrontHolder : CardBattleUI;

                to.SetActive(true);

                StartCoroutine(Fade(to.GetComponent<CanvasGroup>(), true, flipTime));
                StartCoroutine(Fade(from.GetComponent<CanvasGroup>(), false, flipTime));

                yield return new WaitForSeconds(flipTime);

                from.SetActive(false);

            }
        }

        private IEnumerator Fade(CanvasGroup canvasGroup, bool fadeIn, float duration)
        {
            var endTime = Time.time + duration;

            if (fadeIn)
            {
                while (Time.time < endTime)
                {
                    canvasGroup.alpha = 1f - (endTime - Time.time) / duration;
                    yield return null;
                }
            }
            else
            {
                while (Time.time < endTime)
                {
                    canvasGroup.alpha = (endTime - Time.time) / duration; 
                    yield return null;
                }
            }
        }

#endif
#if false
    public void Update()
    {
        var touch = Input.GetTouch(0);

        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            // ui touched
            CardHighlight.Show(this);
        }
        else if (touch.phase == TouchPhase.Ended)
            CardHighlight.Hide();

    }
#endif
        #endregion
    }
}