using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedTestEnemyMovement : EnemyMovement, IWalker
{
	public override bool DoesNotice(Vector2 entityPos)
	{
		return false;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();


	}

	public override void EndCurrentBehavior()
	{
		//nothing since this enemy never notices
	}

	public override void ResumeBehavior()
	{
		//nothing since this enemy never notices
	}
}
