﻿using MapLogic;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LocationUI : Singleton<LocationUI>
    {
        public Image LocationImage;
        public TextMeshProUGUI Description;
        public Button OptionInstance;
        private List<Button> InstantiatedButtons = new List<Button>();
        private MapNode CurrentNode;
        public GameObject Holder;

        private void Start()
        {
            MapNode.OpenLocationEvent.AddListener(Open);
            MapNode.CloseLocationEvent.AddListener(n=>Close());
            OptionInstance.gameObject.SetActive(false);
        }

        public void Open(MapNode node)
        {
            Holder.SetActive(true);

            LocationImage.sprite = node.Location.LocationImage;

            Description.text = node.Location.LocationDescription;

            foreach(var oldBut in InstantiatedButtons)
            {
                Destroy(oldBut.gameObject);
            }

            InstantiatedButtons.Clear();

            CurrentNode = node;

            foreach (var opt in node.GetOptions())
            {
                CreateButton(opt);
            }
        }

        private void Close ()
        {
            Holder.SetActive(false);
        }

        private void CreateButton(MapOption option)
        {
            var instance = Instantiate(OptionInstance, OptionInstance.transform.parent);

            instance.onClick.AddListener(() => CurrentNode.SelectOption(option));

            instance.GetComponent<TextMeshProUGUI>().text = option.OptionDescription;

            instance.gameObject.SetActive(true);

            InstantiatedButtons.Add(instance);
        }
    }
}