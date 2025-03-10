﻿using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class GroundedTestEnemyMovement : EnemyMovement, IWalker
{
	public float BASE_WALK_SPEED = 1.0f;
	public float GRAVITY = -20.0f;
	public CollisionCalculator collCalc;
	public BoxCollider2D collBox;
	public override bool DoesNotice(Vector2 entityPos)
	{
		return false;
	}

	protected override void Start()
	{
		base.Start();

		StartCoroutine(Idle());
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (collCalc.IsOnWalkableGround())
			mover.persistentVel.y = 0;
		else
			mover.persistentVel.y += GRAVITY * Time.deltaTime;
	}

	public override void EndCurrentBehavior()
	{
		//nothing since this enemy never notices
	}

	public override void ResumeBehavior()
	{
		//nothing since this enemy never notices
	}

	public void MoveTowardTarget(float speedMultiplier = 1)
	{
		Vector2 targetDir = moveTarget - (Vector2)transform.position;
		targetDir.y = 0;
		float approachSlowFactor = 1.0f;
		if (targetDir.sqrMagnitude < 0.5)
			approachSlowFactor = targetDir.magnitude; //using the squared version so that movement slows less further from the target.

		targetDir.Normalize();

		mover.persistentVel = targetDir * (BASE_WALK_SPEED * approachSlowFactor * speedMultiplier);
	}

	public IEnumerator MoveToTarget(float speedMultiplier = 1, float margin = 0.1F)
	{
		amMoving = true;
		while (Mathf.Abs(transform.position.x - moveTarget.x) > margin) //only compares horizontal distance
		{
			if (kbDurationLeft == 0)
				MoveTowardTarget(speedMultiplier);

			yield return new WaitForFixedUpdate();
			if (this == null) //entity death safeguard
				yield break;
		}
		mover.persistentVel = Vector2.zero; //Arrest movement once within margin

		amMoving = false;
		yield break;
	}

	public IEnumerator Idle()
	{
		yield return new WaitForFixedUpdate(); //waits until eerything finishes starting
		mover.persistentVel.x = BASE_WALK_SPEED;

		while (true)
		{
			if (collCalc.NextToLeftWall() || collCalc.OnLeftLedge())
					mover.persistentVel.x = BASE_WALK_SPEED;
			if (collCalc.NextToRightWall() || collCalc.OnRightLedge())
					mover.persistentVel.x = -BASE_WALK_SPEED;

			yield return new WaitForFixedUpdate();
			if (this == null) //entity death safeguard
				yield break;
		}
	}

	public bool IsMoving()
	{
		return amMoving; 
	}
}
