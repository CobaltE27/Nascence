﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HaloLineFormation : Formation
{
	private float width;
	public HaloLineFormation(Vector2 displacementFromCenter, Transform centerOfFormations, float width, HashSet<Type> attributes = null) : base(displacementFromCenter, centerOfFormations, attributes)
	{
		attributes.Add(typeof(IFlier));
		this.width = width;
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
		float positionProximity = width / (positions.Count + 1);
		if (Puppets.Count > 0)
			positions[0] = new Vector2();
		for (int i = 1; i < Puppets.Count; i++)
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

		//Everything below here can be extracted to a helper
		Vector2 formationCenterWorldCoords = DisplacementFromCenter + CenterOfFormations;
		Vector2[] posCopy = new Vector2[positions.Count()];
		positions.CopyTo(posCopy);
		List<Vector2> positionsCopy = new List<Vector2>(posCopy);
		List<EnemyMovement> enemiesByDistance = Puppets.OrderByDescending(o => Vector2.Distance(formationCenterWorldCoords, o.transform.position)).ToList();

		EnemyMovement[] newPuppetsList = new EnemyMovement[Puppets.Count];
		for (int i = 0; i < enemiesByDistance.Count(); i++) //Give the farthest enemy its closest position
		{
			positionsCopy = positionsCopy.OrderBy(o => Vector2.Distance(o + formationCenterWorldCoords, enemiesByDistance[i].transform.position)).ToList();
			Vector2 nearestPos = positionsCopy.First();
			positionsCopy.RemoveAt(0);
			newPuppetsList[positions.IndexOf(nearestPos)] = enemiesByDistance[i];
		}

		Puppets = new List<EnemyMovement>(newPuppetsList);
	}
}
