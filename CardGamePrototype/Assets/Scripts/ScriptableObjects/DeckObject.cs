﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DeckObject : ScriptableObject
{
    public new string name;
    public string Description;
    public List<Creature> Creatures;
    public Sprite DeckIcon;
    public Sprite DeckImage;

    public Race[] FriendRaces;
    public Race[] EnemyRaces;

    internal void AddCreature(Creature creature)
    {
        throw new NotImplementedException();
    }
}
