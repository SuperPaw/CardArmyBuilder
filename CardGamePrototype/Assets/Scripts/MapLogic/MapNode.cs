﻿using GameLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MapLogic
{
    public class MapNode
    {
        public List<MapNode> LeadsTo = new List<MapNode>();
        public int Id;
        public bool Visited;
        public bool Active;
        public readonly MapLocation Location;
        public string name;
        public Dictionary<MapOption, List<Card>> SelectedCards = new Dictionary<MapOption, List<Card>>();

        public class LocationEvent : UnityEvent<MapNode> { }
        public static LocationEvent OpenLocationEvent = new LocationEvent();
        public static LocationEvent CloseLocationEvent = new LocationEvent();

        //public List<MapOption> SelectedOptions = new List<MapOption>();


        public MapNode(MapLocation mapLocation)
        {
            this.Location = mapLocation;
            name = mapLocation.name + " " + Guid.NewGuid();
        }

        public void Visit()
        {
            Visited = true;
        }

        public MapOption[] GetOptions()
        {
            return Location.LocationOptions.Where(o => o.IsApplicable()).ToArray();
        }

        public void Open()
        {
            Visited = Active = true;

            foreach (var option in Location.LocationOptions)
            {
                option.FindCandidate(this);
            }

            OpenLocationEvent.Invoke(this);
        }

        public void AddAssociation(MapOption option, Card unit)
        {
            if (!SelectedCards.ContainsKey(option))
                SelectedCards.Add(option, new List<Card>() );
            SelectedCards[option].Add(unit);
        }

        public void SelectOption(int i)
        {
            if (i >= Location.LocationOptions.Length)
                return;

            MapOption mapOption = Location.LocationOptions[i];
            SelectOption(mapOption);
        }

        public void SelectOption(MapOption mapOption)
        {
            if (!Location.LocationOptions.Contains(mapOption) || !mapOption.IsApplicable())
                return;

            mapOption.ExecuteOption(this);

            if (mapOption.ClosesLocationOnSelection)
            {
                Active = false;
                CloseLocationEvent.Invoke(this);
            }
        }

        public bool IsFinalNode() => Location.WinNode;
        public bool IsStartNode() => Location.StartNode;

        private bool OptionApplicable(MapOption mapOption)
        {
            return mapOption.OnlyForHeroRaces.Count == 0 //|| mapOption.OnlyForHeroRaces.Contains(GameManager.)
                &&
                mapOption.OnlyForAbility.Count == 0 //|| mapOption.OnlyForAbility.Contains
                ;
        }
    }
}