using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrawlerMovement : EnemyMovement, IWalker, IDasher
{
	public float BASE_WALK_SPEED = 1.0f;
	public float GRAVITY = -15.0f;
	public CollisionCalculator collCalc;
	public BoxCollider2D collBox;
	public SpriteRenderer spr;
	private Coroutine idleRoutine;

	protected override void Start()
	{
		base.Start();

		idleRoutine = StartCoroutine(Idle());
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
		StopCoroutine(idleRoutine);
	}

	public override void ResumeBehavior()
	{
		idleRoutine = StartCoroutine(Idle());
	}

	public void MoveTowardTarget(float speedMultiplier = 1)
	{
		Vector2 targetDir = moveTarget - (Vector2)transform.position;
		targetDir.y = 0;

		if ((targetDir.x < 0 && (collCalc.NextToLeftWall() || collCalc.OnLeftLedge())) ||
			(targetDir.x > 0 && (collCalc.NextToRightWall() || collCalc.OnRightLedge()))) //refuses to walk off ledges/into walls during normal movement
		{
			mover.persistentVel.x = 0;
			return;
		}

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
		Vector2 IdlingDirection = new Vector2(1, 0);
		float IdlingDistance = 1;
		System.Random rng = new System.Random(GetInstanceID()); //seed is ensured to be unique between enemies
		int wait = rng.Next(50);
		moveTarget = (Vector2)transform.position + (IdlingDirection * IdlingDistance);
		while (true)
		{
			MoveTowardTarget();

			if (Vector2.Distance(transform.position, moveTarget) < 0.1f)
			{
				if (wait > 0)
				{
					wait--;
				}
				else
				{
					IdlingDirection *= -1;
					moveTarget = (Vector2)transform.position + (IdlingDirection * IdlingDistance);
					wait = rng.Next(50);
				}
			}

			yield return new WaitForFixedUpdate();
			if (this == null) //entity death safeguard
				yield break;
		}
	}

	public bool IsMoving()
	{
		return amMoving;
	}

	public IEnumerator DashToward(Vector2 target)
	{
		throw new System.NotImplementedException();
	}

	public bool IsAttacking()
	{
		return amAttacking;
	}
}
