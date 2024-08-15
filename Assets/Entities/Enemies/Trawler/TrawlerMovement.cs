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

		DebugDrawer.DrawPoint(moveTarget);
		if (!collCalc.IsOnWalkableGround())
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
		float IdlingDistance = 3;
		System.Random rng = new System.Random(GetInstanceID()); //seed is ensured to be unique between enemies
		int wait = rng.Next(50) + 100;
		moveTarget = (Vector2)transform.position + (IdlingDirection * IdlingDistance);
		while (true)
		{
			MoveTowardTarget();

			if (Mathf.Abs(transform.position.x - moveTarget.x) < 0.1f)
			{
				if (wait > 0)
				{
					wait--;
				}
				else
				{
					IdlingDirection *= -1;
					moveTarget = (Vector2)transform.position + (IdlingDirection * IdlingDistance);
					wait = rng.Next(50) + 100;
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
		amAttacking = true;
		DebugDrawer.DrawPoint(target, Color.red, 5.0f);
		//windup
		Vector2 back = (Vector2)transform.position + ((Vector2)transform.position - target).normalized;
		Vector2 toward = (target - (Vector2)transform.position).normalized;
		Vector2 past = target + toward * 5.0f;
		for (int windUpTimer = 0; windUpTimer < 25; windUpTimer++)
		{
			MoveTowardArbitrary(back);
			yield return new WaitForFixedUpdate();
		}
		//dash loop
		bool hitWall = false;
		while (true)
		{
			MoveTowardArbitrary((Vector2)transform.position + toward, 7.0f);

			if (transform.position.x - past.x < 0.1f) //charged past target
			{
				break;
			}
			if ((toward.x < 0 && collCalc.NextToLeftWall()) || (toward.x > 0 && collCalc.NextToRightWall())) //if hit wall
			{
				hitWall = true;
				break;
			}
			yield return new WaitForFixedUpdate();
		}

		//conditional wind down if hit wall
		if (hitWall)
		{
			mover.persistentVel = -toward * 7.0f + Vector2.up * 6.0f;
			for (int stunTimer = 0; stunTimer < 50; stunTimer++)
			{
				mover.persistentVel.x *= 0.95f;
				yield return new WaitForFixedUpdate();
			}
		}

		//wind down
		for (int stunTimer = 0; stunTimer < 25; stunTimer++)
		{
			mover.persistentVel.x *= 0.95f;
			yield return new WaitForFixedUpdate();
		}
		mover.persistentVel.x = 0;

		amAttacking = false;
		yield break;
	}

	/// <summary>
	/// Sets velocity to point toward a target position.
	/// </summary>
	/// <param name="target"></param>
	public void MoveTowardArbitrary(Vector2 target, float speedMultiplier = 1.0f)
	{
		Vector2 targetDir = target - (Vector2)transform.position;
		targetDir.y = 0;
		float approachSlowFactor = 1.0f;
		if (targetDir.sqrMagnitude < 0.5)
			approachSlowFactor = targetDir.magnitude; //using the squared version so that movement slows less further from the target.

		targetDir.Normalize();

		mover.persistentVel = targetDir * (BASE_WALK_SPEED * approachSlowFactor * speedMultiplier);
	}

	public bool IsAttacking()
	{
		return amAttacking;
	}
}
