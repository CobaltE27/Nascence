using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : EntityMovement
{
    public Rigidbody2D rb;

	public Vector2 moveTarget;
	public float NOTICE_RANGE = 10.0f;

    protected bool amMoving = false;
    protected bool amAttacking = false;
    public bool inFormation = false;

    /// <summary>
    /// Gets mover component attached to this object, adds "kbVelocity" as a constant velocity, sets move target to current position.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        moveTarget = (Vector2)transform.position;
	}

    protected virtual void FixedUpdate()
    {

	}

    /// <summary>
    /// Returns whether this enemy would notice and entity at the given position.
    /// </summary>
    public virtual bool DoesNotice(Vector2 entityPos)
    {
		float distanceToChar = Vector2.Distance(transform.position, entityPos);
		return distanceToChar < NOTICE_RANGE;
	}

	/// <summary>
	/// A method that can be called to stop whatever this enemy is doing once it is abducted by the puppeteer.
	/// </summary>
	public abstract void EndCurrentBehavior();

	/// <summary>
	/// A method to resume (usaually idle) behavior when the pupeteer drop this enemy
	/// </summary>
	public abstract void ResumeBehavior();
}
