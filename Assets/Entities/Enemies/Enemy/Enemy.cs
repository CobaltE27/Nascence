using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Rigidbody2D rb;
    public Collider2D collider;
	public float health = 10.0f;
    public int steamOnHit = 50;

    public Vector2 moveTarget;
    public bool isTargeting = false;
    public float NOTICE_RANGE = 10.0f;

    public float MAX_MOVEMENT_SPEED = 0.5f; //m per second
    private EntityMover mover;

    private float kbStrength = 0.8f;
    private int kbDurationLeft = 0;
    public readonly int KB_DURATION_FRAMES = 20;
    private Vector2 kbDirectionalBias = new Vector2(1, 0);

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
	/// Damages this enemy for the specified amount, if knockback is applied, it moves this
	/// enemy in the specified direction.
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="direction"></param>
    public void DealDamage(float damage, Vector2 direction = new Vector2())
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
