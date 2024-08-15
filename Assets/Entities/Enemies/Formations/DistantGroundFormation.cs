using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DistantGroundFormation : Formation
{
	public Vector2 facingDir = Vector2.right;
	public DistantGroundFormation(Vector2 displacementFromCenter, Transform centerOfFormations, HashSet<Type> attributes = null) : base(displacementFromCenter, centerOfFormations, attributes)
	{
		attributes.Add(typeof(IWalker));
	}

	public override void AddPuppet(EnemyMovement puppet)
	{
		Puppets.Add(puppet);
		positions.Add(new Vector2());
		ReevaluatePositions();
	}

	public override void RemovePuppet(EnemyMovement puppet)
	{
		positions.RemoveAt(Puppets.IndexOf(puppet));
		Puppets.Remove(puppet);
		ReevaluatePositions();
	}

	protected override void ReevaluatePositions()
	{
		float width = Puppets.Count + 1;
		float positionProximity = width / (positions.Count + 1);
		if (Puppets.Count > 0)
			positions[0] = new Vector2();
		for (int i = 1; i < Puppets.Count; i++)
		{
			positions[i] = new Vector2(i * positionProximity * -facingDir.normalized.x, 0); //if facing right, additional enemies are added on the left, and vice versa
		}

		ReassignPuppetPositions();
	}

	public override Vector2 PreferredPosition(Vector2 playerPosition)
	{
		Vector2 toward = playerPosition - (CenterOfFormations + DisplacementFromCenter);
		toward.y = 0;
		toward.Normalize();
		return playerPosition - 10.0f * toward; //wants to be 10 units away form the player on the current side
	}
}
