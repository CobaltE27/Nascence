using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Rigidbody2D rb;
    public Collider2D collider;
    private CollisionCalculator collCalc;
	public float health = 10.0f;
    public int steamOnHit = 50;

    public Vector2 target;
    public float NOTICE_RANGE = 10.0f;

    public Vector2 velocity = new Vector2();
    public float MAX_MOVEMENT_SPEED = 0.5f; //m per second

    private float kbStrength = 0.8f;
    private Vector2 kbVelocity = new Vector2();
    private int kbDurationLeft = 0;
    public readonly int KB_DURATION_FRAMES = 20;
    private Vector2 kbDirectionalBias = new Vector2(1, 0);

    int count = 0;

    void Start()
    {
		collCalc = new CollisionCalculator(collider);
	}

    void FixedUpdate()
    {
        velocity *= 0; //reset velocity
        if (target != null)
            SetVelocityTowardTarget(target);

        if (count % 100 < 50)
		{
            velocity += new Vector2(0, 0.5f);
		}
        else
		{
            velocity += new Vector2(0, -0.5f);
        }

        rb.MovePosition(rb.position + collCalc.MoveAndSlideRedirectVelocity(ref velocity, Time.deltaTime) + collCalc.MoveAndSlide(kbVelocity, Time.deltaTime));

		count++;
        if (kbDurationLeft < 1)
            kbVelocity.Set(0, 0);
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

        kbVelocity = direction.normalized * kbStrength * kbDirectionalBias;
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
		float approachFactor = 1.0f;
        if (targetDir.sqrMagnitude < 0.5)
            approachFactor = targetDir.sqrMagnitude * 2; //using the squared version so that movement slows less further from the target.
			
        targetDir.Normalize();

		velocity += targetDir * (MAX_MOVEMENT_SPEED * approachFactor);
    }
}
