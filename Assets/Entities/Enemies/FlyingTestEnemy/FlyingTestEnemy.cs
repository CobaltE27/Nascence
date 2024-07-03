using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FlyingTestEnemy : EnemyMovement, IFlier, IDasher
{
	public float BASE_MOVEMENT_SPEED = 5.0f; //m per second
	public SpriteRenderer spr;
	private Coroutine idleRoutine;

	protected override void Start()
	{
		base.Start();
		KB_DURATION_FRAMES = 10;
		kbWeaknessMult = 2.5f;

		idleRoutine = StartCoroutine(Idle());
	}

	protected override void FixedUpdate()
	{

	}

	/// <summary>
	/// Sets velocity to point toward the target position in world-space.
	/// Consider adding momentum system in the future so that enemies respond to moving targets in a smoother way
	/// </summary>
	/// <param name="target"></param>
	public void MoveTowardTarget(float speedMultiplier)
	{
		Vector2 targetDir = moveTarget - (Vector2)transform.position;
		float approachSlowFactor = 1.0f;
		if (targetDir.sqrMagnitude < 0.5)
			approachSlowFactor = targetDir.magnitude; //using the squared version so that movement slows less further from the target.

		targetDir.Normalize();

		mover.persistentVel = targetDir * (BASE_MOVEMENT_SPEED * approachSlowFactor * speedMultiplier);
	}

	/// <summary>
	/// Sets velocity to point toward a target position.
	/// </summary>
	/// <param name="target"></param>
	public void MoveTowardArbitrary(Vector2 target, float speedMultiplier)
	{
		Vector2 targetDir = target - (Vector2)transform.position;
		float approachSlowFactor = 1.0f;
		if (targetDir.sqrMagnitude < 0.5)
			approachSlowFactor = targetDir.magnitude; //using the squared version so that movement slows less further from the target.

		targetDir.Normalize();

		mover.persistentVel = targetDir * (BASE_MOVEMENT_SPEED * approachSlowFactor * speedMultiplier);
	}

	public IEnumerator Idle()
	{
		Vector2 IdlingDirection = new Vector2(1, 0);
		float IdlingDistance = 1;
		System.Random rng = new System.Random(GetInstanceID()); //seed is ensured to be unique between enemies
		int wait = rng.Next(50);
		moveTarget = (Vector2) transform.position + (IdlingDirection * IdlingDistance);
		while (true)
		{
			MoveTowardTarget(0.3f);

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
		}
	}

	IEnumerator IMoving.MoveToTarget(float speedMultiplier, float margin)
	{
		amMoving = true;
		while (Vector2.Distance((Vector2)transform.position, moveTarget) > margin)
		{
			if (kbDurationLeft == 0)
				MoveTowardTarget(speedMultiplier);

			yield return new WaitForFixedUpdate();
			if (this == null) //temporary testing fix
				yield break;
		}
		mover.persistentVel = Vector2.zero; //Arrest movement once within margin

		amMoving = false;
		yield break;
	}

	public bool IsMoving()
	{
		return amMoving;
	}

	public override void EndCurrentBehavior()
	{
		StopCoroutine(idleRoutine);
	}

	public override void ResumeBehavior()
	{
		idleRoutine = StartCoroutine(Idle());
	}

	public IEnumerator DashToward(Vector2 target)
	{
		amAttacking = true;
		spr.color = Color.red;
		Vector2 dashDirection = target - (Vector2)transform.position;
		dashDirection.Normalize();
		Vector2 dashEnd = target + dashDirection * 5.0f; //overshoot player
		bool reachedEnd = false;
		bool pastTarget = false;
		int pastCounter = 0;

		while (true)
		{
			MoveTowardArbitrary(dashEnd, 2.0f);
			reachedEnd = Vector2.Distance(dashEnd, (Vector2)transform.position) < 0.1f;
			pastTarget = Vector2.Angle((Vector2)transform.position - target, dashDirection) < 90; //should be 180 when dash starts
			if (pastTarget)
				pastCounter++;
			
			if (reachedEnd || pastCounter > 50)
				break;

			yield return new WaitForFixedUpdate();
			if (this == null) //temporary testing fix
				yield break;
		}

		spr.color = new Color(255/255.0f, 159/255.0f, 52/255.0f);
		amAttacking = false;
		yield break;
	}

	public bool IsAttacking()
	{
		return amAttacking;
	}
}
