using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTestEnemy : Enemy
{
	public float MAX_MOVEMENT_SPEED = 0.5f; //m per second

	private new float kbStrength = 1.0f;
	public new readonly int KB_DURATION_FRAMES = 25;

	int count = 0;

	void Start()
	{
		mover = GetComponent<EntityMover>();
		mover.constantVels.Add("kbVelocity", new Vector2());
	}

	void FixedUpdate()
	{
		mover.persistentVel *= 0; //reset velocity
		if (isTargeting)
			SetVelocityTowardTarget(moveTarget);

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
	/// Sets velocity to point toward a target position in world-space.
	/// Consider adding momentum system in the future so that enemies respond to moving targets in a smoother way
	/// </summary>
	/// <param name="target"></param>
	private void SetVelocityTowardTarget(Vector2 target)
	{
		Vector2 targetDir = target - (Vector2)rb.transform.position;
		float approachSlowFactor = 1.0f;
		if (targetDir.sqrMagnitude < 0.5)
			approachSlowFactor = targetDir.sqrMagnitude * 2; //using the squared version so that movement slows less further from the target.

		targetDir.Normalize();

		mover.persistentVel += targetDir * (MAX_MOVEMENT_SPEED * approachSlowFactor);
	}
}
