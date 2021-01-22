﻿using GameLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Event = GameLogic.Event;
using Random = UnityEngine.Random;



namespace MapLogic
{
    public class MapController 
    {
        private static MapController instance;

        public List<MapNode> Nodes = new List<MapNode>();
        public MapNode CurrentNode;

        public class AmountEvent : UnityEvent<int> { }
        public AmountEvent OnPlayerGoldUpdate = new AmountEvent();

        private int playerGold;

        public static MapController Instance { get {
                if (instance == null)
                    instance = new MapController();
                return instance;
            }
        }

        public int PlayerGold
        {
            get => playerGold; 
            set
            {
                if (value == playerGold)
                    return;

                playerGold = value;

                OnPlayerGoldUpdate.Invoke(value);
            }
        }

        public void CreateMap()
        {
            var settings = MapSettings.Instance;

            int CurrentDifficulty = settings.StepDifficultyIncrease *4 ;


            int[] nodesAtStep = new int[settings.MapLength];

            nodesAtStep[0] = nodesAtStep[settings.MapLength - 1] = 1;
            nodesAtStep[settings.MapLength - 2] = settings.MinNodesAtStep;

            for (int i = 1; i < settings.MapLength - 2; i++)
            {
                nodesAtStep[i] = Mathf.Min(nodesAtStep[i - 1] * settings.MinNodesAtStep, Random.Range(settings.MinNodesAtStep, settings.MaxNodesAtStep));
            }

            var noOfNodes = nodesAtStep.Sum();

            var locations = settings.Locations.OrderBy(c => Random.value).ToList();

            //only one startnode
            while (locations.Count(l => l.IsStartNode()) > 1)
                locations.Remove(locations.First(l => l.IsStartNode()));

            //only one winnode
            while (locations.Count(l => l.IsWinNode()) > 1)
                locations.Remove(locations.First(l => l.IsWinNode()));

            var nonUniques = locations.Where(l => !l.IsUniqueNode()).ToArray();

            //correct number of locations
            while (locations.Count < noOfNodes)
            {
                locations.Add(nonUniques[Random.Range(0, nonUniques.Length)]);
            }

            while (locations.Count > noOfNodes)
            {
                locations.Remove(locations.First(l => !l.IsStartNode() && !l.IsWinNode()));
            }

            locations = locations.OrderBy(d => d.Difficulty() + settings.RandomnessToDifficulty * Random.value).ToList();

            MapNode[] lastStep = { GenerateNode(locations.Single(l => l.IsStartNode()), locations) };

            for (int i = 1; i < settings.MapLength; i++)
            {
                CurrentDifficulty += settings.StepDifficultyIncrease;

                if (nodesAtStep[i] < 1)
                    Debug.LogError("Generated pathless map");

                MapNode[] step = new MapNode[nodesAtStep[i]];

                for (int j = 0; j < nodesAtStep[i]; j++)
                {
                    if (i == settings.MapLength - 1)
                        step[j] = GenerateNode(locations.Single(l => l.IsWinNode()), locations);
                    else
                        step[j] = GenerateNode(Random.value < settings.TreasureChance,CurrentDifficulty);
                }

                var edgesPrNode = step.Length / (float)lastStep.Length;

                var parentIdx = 0;
                var nodeIdx = 0;
                var extraRoadChance = MapSettings.Instance.ExtraRoadChance;

                lastStep[parentIdx].LeadsTo.Add(step[nodeIdx]);

                //Creating road connections
                do
                {
                    //Debug.Log($"parent: {parentIdx} leads to node: {nodeIdx}");
                    if (extraRoadChance < Random.value)
                    {
                        parentIdx++;
                        nodeIdx++;

                        if (nodeIdx == step.Length) nodeIdx--;
                        if (parentIdx == lastStep.Length) parentIdx--;
                    }
                    else
                    {
                        var parentChance = Random.value * (lastStep.Length - parentIdx - 1);
                        var nodeChance = Random.value * (step.Length - nodeIdx - 1);

                        if (parentChance > nodeChance) parentIdx++;
                        else nodeIdx++;
                    }

                    lastStep[parentIdx].LeadsTo.Add(step[nodeIdx]);
                }
                while (nodeIdx < step.Length-1  || parentIdx < lastStep.Length-1 );

                lastStep = step;
            }

            CurrentNode = Nodes.Single(n => n.IsStartNode());
        }

        public void MoveToNode(MapNode node)
        {
            if (!CurrentNode.LeadsTo.Contains(node))
                return;
            if (CurrentNode.Active)
                return;

            CurrentNode = node;
            node.Open();
        }

        public void StartCombat(Deck enemyDeck)
        {
            Event.OnCombatSetup.Invoke(BattleManager.Instance. PlayerDeck, enemyDeck);
        }


        private MapNode GenerateNode(bool goodNode,int CR)
        {
            MapNode node;

            var adjust = Mathf.Min(CR, MapSettings.Instance.RandomnessToDifficulty);

            CR += Random.Range(-adjust, adjust);

            if (CR < MapSettings.Instance.StepDifficultyIncrease) CR = MapSettings.Instance.StepDifficultyIncrease;
                

            if(goodNode)
            {
                var v = Random.value;

                if(v > 0.4f)
                {
                    //village
                    var races = MapSettings.Instance.CivilizedRaces;

                    node = new MapNode( new HireUnitOption(races[Random.Range(0,races.Length)],CR));
                }
                else if(v >0.2f )
                {
                    //gold
                    node = new MapNode(new GainGoldOption(CR));
                }
                else
                {
                    //xp
                    node = new MapNode(new GainXpOption(CR));
                }
                    
            }
            else
            {

                var races = MapSettings.Instance.EnemyRaces;
                var race = races[Random.Range(0, races.Length)];

                float hardCombatChance = 0.25f;

                if (hardCombatChance > Random.value)
                {
                    node = new MapNode( new CombatOption(race,CR * 3,true));

                }
                else
                {
                    node = new MapNode(new CombatOption(race, CR,false));
                }

            }

            Nodes.Add(node);

            node.Id = Nodes.Count;

            return node;
        }

        private MapNode GenerateNode(IMapLocation locationObject, List<IMapLocation> locations)
        {
            locations.Remove(locationObject);

            var node = new MapNode(locationObject);
            Nodes.Add(node);

            node.Id = Nodes.Count;

            return node;
        }
    }
}