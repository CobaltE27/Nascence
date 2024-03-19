using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Formation
{
    Vector2 puppeteerCenter; //this should usually be the player, but may lag behind them or be the center of a room or something
    private Vector2 FormationCenter { get { return puppeteerCenter + displacementFromCenter; } }//Where the center/beginning of this formation is
    private Vector2 displacementFromCenter;
    private List<Tuple<Enemy, Vector2>> positions; //consider whether this is really the best way to store these, or whether there should be two seperate lists.

    public Formation(ref Vector2 puppeteerCenter, ref Vector2 displacementFromCenter)
    {
        this.puppeteerCenter = puppeteerCenter;
        this.displacementFromCenter = displacementFromCenter;
        positions = new List<Tuple<Enemy, Vector2>>();
	}

    public abstract void AddPuppet(Enemy puppet);

    public abstract void RemovePuppet(Enemy puppet);
}
