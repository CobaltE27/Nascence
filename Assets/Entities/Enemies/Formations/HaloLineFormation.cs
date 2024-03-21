using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloLineFormation : Formation
{
    public HaloLineFormation(ref Vector2 displacementFromCenter) : base(ref displacementFromCenter)
    {

    }

	public override void AddPuppet(Enemy puppet)
	{
		throw new System.NotImplementedException();
	}

	public override void RemovePuppet(Enemy puppet)
	{
		throw new System.NotImplementedException();
	}
}
