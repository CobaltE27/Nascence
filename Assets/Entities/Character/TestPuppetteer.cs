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

	protected override void DecideNextState()
	{
		CleanFormations();
		if (fliers.Puppets.Count == 0)
		{
			StartCoroutine(WaitAndThen(DecideNextState));
			return;
		}
		else
		{
			foreach (Enemy flier in fliers.Puppets)
			{
				IAttacker flierAttackBehavior = (IAttacker)flier;
				if (!flierAttackBehavior.IsAttacking())
				{
					flier.moveTarget = fliers.FormationPositionOf(flier);
					IMoving flierMoveBehavior = (IMoving)flier;
					if (!flierMoveBehavior.IsMoving())
						StartCoroutine(flierMoveBehavior.MoveToTarget()); 
				}
			}

			if (fliers.Aligned())
			{
				//Debug.Log("Formation aligned");
				GroupAttack nextMove = groupAttacks[rng.Next(groupAttacks.Count)];
				StartCoroutine(nextMove(DecideNextState));
			}
			else
			{
				StartCoroutine(NextFrameAndThen(DecideNextState));
			}
		}
	}

	private IEnumerator AllAttack(Action callBack)
	{
		//Debug.Log("All attacking");
		foreach (IDasher enemy in fliers.Puppets)
		{
			StartCoroutine(enemy.DashToward(player.transform.position));
		}

		StartCoroutine(WaitAndThen(callBack));
		yield break;
	}

	private IEnumerator WaitAndThen(Action callBack)
	{
		yield return new WaitForSeconds(1);
		callBack();
		yield break;
	}

	private IEnumerator NextFrameAndThen(Action callBack)
	{
		yield return new WaitForFixedUpdate();
		callBack();
		yield break;
	}
}
