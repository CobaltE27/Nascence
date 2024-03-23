using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTestEnemy : Enemy, IFlier
{
	public float MAX_MOVEMENT_SPEED = 5.0f; //m per second
	private bool amMoving = false;

	protected override void Start()
	{
		base.Start();
		kbStrength = 1.0f;
		KB_DURATION_FRAMES = 25;

		StartCoroutine(Idle());
	}

	protected override void FixedUpdate()
	{

	}

	/// <summary>
	/// Damages this enemy for the specified amount, if knockback is applied, it moves this
	/// enemy in the specified direction.
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="direction"></param>
	public override void DealDamage(float damage, Vector2 direction = new Vector2())
	{
		health -= damage;

		if (health <= 0.0f)
			Destroy(this.gameObject);

		StartCoroutine(ApplyKnockback(direction.normalized * kbStrength * kbDirectionalBias));
	}

	/// <summary>
	/// Sets velocity to point toward a target position in world-space.
	/// Consider adding momentum system in the future so that enemies respond to moving targets in a smoother way
	/// </summary>
	/// <param name="target"></param>
	public void MoveTowardTarget(float speedMultiplier)
	{
		Vector2 targetDir = moveTarget - (Vector2)rb.transform.position;
		float approachSlowFactor = 1.0f;
		if (targetDir.sqrMagnitude < 0.5)
			approachSlowFactor = targetDir.magnitude; //using the squared version so that movement slows less further from the target.

		targetDir.Normalize();

		mover.persistentVel = targetDir * (MAX_MOVEMENT_SPEED * approachSlowFactor * speedMultiplier);
	}

	private IEnumerator ApplyKnockback(Vector2 kbVel)
	{
		kbDurationLeft = KB_DURATION_FRAMES;
		mover.constantVels["kbVelocity"] = kbVel;
		while (kbDurationLeft > 0)
		{
			kbDurationLeft--;
			yield return new WaitForFixedUpdate();
		}

		mover.constantVels["kbVelocity"] *= 0;
		yield break;
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
		}

		amMoving = false;
		yield break;
	}

	public bool IsMoving()
	{
		return amMoving;
	}

	public override void EndCurrentBehavior()
	{
		StopCoroutine(Idle());
	}

	public override void ResumeBehavior()
	{
		StartCoroutine(Idle());
	}
}
