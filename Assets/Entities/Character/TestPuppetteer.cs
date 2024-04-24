using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPuppetteer : Puppetteer
{
	public GameObject player;
	Formation fliers;
	System.Random rng = new System.Random();

	protected override void Start()
	{
		fliers = new HaloLineFormation(new Vector2(0, 4), player.transform, 7, new HashSet<System.Type>() { typeof(IFlier), typeof(IDasher) });
		formations.Add(fliers);
		groupAttacks.Add(AllAttack);
		base.Start();
	}

	protected override IEnumerator DecideNextState()
	{
		CleanFormations();
		if (fliers.Puppets.Count == 0)
		{
			Debug.Log("No puppets yet");
			yield return new WaitForSeconds(0.5f);
			StartCoroutine(DecideNextState());
			yield break;
		}
		else
		{
			foreach (Enemy flier in fliers.Puppets)
			{
				flier.moveTarget = fliers.FormationPositionOf(flier);
				IMoving flierMoveBehavior = (IMoving)flier;
				if (!flierMoveBehavior.IsMoving())
					flierMoveBehavior.MoveToTarget(3);
			}

			GroupAttack nextMove = groupAttacks[rng.Next(groupAttacks.Count)];
			yield return new WaitForSeconds(1);
			nextMove(() => StartCoroutine(DecideNextState()));
			yield break;
		}
	}

	private IEnumerator AllAttack(Action callBack)
	{
		foreach (IDasher enemy in fliers.Puppets)
		{
			enemy.DashToward(player.transform.position);
		}

		callBack();
		yield break;
	}
}
