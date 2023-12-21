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

    public Vector2 velocity = new Vector2();

    private float kbStrength = 0.8f;
    private Vector2 kbVelocity = new Vector2();
    private int kbDurationLeft = 0;
    public readonly int KB_DURATION_FRAMES = 20;
    private Vector2 kbDirectionalBias = new Vector2(1, 0);

    int count = 0;

    void Start()
    {
		collider = GetComponent<Collider2D>();
		collCalc = new CollisionCalculator(collider);
	}

    void FixedUpdate()
    {
        if (count % 100 < 50)
		{
            velocity = new Vector2(0, 0.5f);
		}
        else
		{
            velocity = new Vector2(0, -0.5f);
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
}
