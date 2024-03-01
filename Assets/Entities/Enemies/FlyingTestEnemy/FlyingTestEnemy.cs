using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTestEnemy : Enemy, IFlier
{
	public float MAX_MOVEMENT_SPEED = 0.5f; //m per second

	int count = 0;

	protected override void Start()
	{
		base.Start();
		kbStrength = 1.0f;
		KB_DURATION_FRAMES = 25;
	}

	protected override void FixedUpdate()
	{
		mover.persistentVel *= 0; //reset velocity
		if (isTargeting)
			MoveTowardTarget();

		if (count % 100 < 50)
		{
			mover.persistentVel += new Vector2(0, 0.5f);
		}
		else
		{
			mover.persistentVel += new Vector2(0, -0.5f);
		}

		count++;
		if (kbDurationLeft < 1)
			mover.constantVels["kbVelocity"] *= 0;
		else
			kbDurationLeft--;
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

		mover.constantVels["kbVelocity"] = direction.normalized * kbStrength * kbDirectionalBias;
		kbDurationLeft = KB_DURATION_FRAMES;
	}

	/// <summary>
	/// Sets velocity to point toward a target position in world-space.
	/// Consider adding momentum system in the future so that enemies respond to moving targets in a smoother way
	/// </summary>
	/// <param name="target"></param>
	public void MoveTowardTarget()
	{
		Vector2 targetDir = moveTarget - (Vector2)rb.transform.position;
		float approachSlowFactor = 1.0f;
		if (targetDir.sqrMagnitude < 0.5)
			approachSlowFactor = targetDir.sqrMagnitude * 2; //using the squared version so that movement slows less further from the target.

		targetDir.Normalize();

		mover.persistentVel += targetDir * (MAX_MOVEMENT_SPEED * approachSlowFactor);
	}
}
