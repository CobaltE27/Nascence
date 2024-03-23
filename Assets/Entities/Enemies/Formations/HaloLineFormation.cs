using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HaloLineFormation : Formation
{
	private float width;
	public HaloLineFormation(Vector2 displacementFromCenter, float width, HashSet<Type> attributes = null) : base(displacementFromCenter, attributes)
	{
		attributes.Add(typeof(IFlier));
		this.width = width;
    }

	public override void AddPuppet(Enemy puppet)
	{
		puppets.Add(puppet);
		positions.Add(new Vector2());
		ReevaluatePositions();
	}

	public override void RemovePuppet(Enemy puppet)
	{
		positions.RemoveAt(puppets.IndexOf(puppet));
		puppets.Remove(puppet);
		ReevaluatePositions();
	}

	protected override void ReevaluatePositions()
	{
		float positionProximity = width / (positions.Count + 1);
		positions[0] = new Vector2();
		for (int i = 1; i < puppets.Count; i++)
		{
			Vector2 newPosition = new Vector2(((i + 1) / 2) * positionProximity, 0);
			if (i % 2 == 1)
				newPosition.x *= -1;
			positions[i] = newPosition;
		}

		if (positions.Count % 2 == 0)
		{
			for (int i = 0; i < positions.Count; i++)
			{
				Vector2 pos = positions[i];
				pos.x += positionProximity / 2;
				positions[i] = pos;
			}
		}				
	}
}
