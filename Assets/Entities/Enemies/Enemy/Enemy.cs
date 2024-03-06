using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public Rigidbody2D rb;
	public float health = 10.0f;
    public int steamOnHit = 50;

	public Vector2 moveTarget;
	public float NOTICE_RANGE = 10.0f;

    protected EntityMover mover;

    protected float kbStrength = 0.8f;
    protected int kbDurationLeft = 0;
    public int KB_DURATION_FRAMES = 20;
    protected Vector2 kbDirectionalBias = new Vector2(1, 0);

    protected virtual void Start()
    {
        mover = GetComponent<EntityMover>();
        mover.constantVels.Add("kbVelocity", new Vector2());
        moveTarget = (Vector2)transform.position;
	}

    protected virtual void FixedUpdate()
    {
        
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
    public virtual void DealDamage(float damage, Vector2 direction = new Vector2())
	{
        health -= damage;

        if (health <= 0.0f)
            Destroy(this.gameObject);

		mover.constantVels["kbVelocity"] = direction.normalized * kbStrength * kbDirectionalBias;
        kbDurationLeft = KB_DURATION_FRAMES; 
	}

    /// <summary>
    /// Returns whether this enemy would notice and entity at the given position.
    /// </summary>
    public virtual bool DoesNotice(Vector2 entityPos)
    {
		float distanceToChar = Vector2.Distance(transform.position, entityPos);
		return distanceToChar < NOTICE_RANGE;
	}
}
