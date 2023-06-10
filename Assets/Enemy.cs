using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Rigidbody2D rb;
    public float health = 10.0f;
    public int steamOnHit = 50;
    private float kbStrength = 0.3f;
    private Vector2 kbDirectionalBias = new Vector2(1, 0);

    int count = 0;

    void FixedUpdate()
    {
        if (count % 100 < 50)
		{
            rb.MovePosition(rb.position + new Vector2(0, 0.01f));
		}
        else
		{
            rb.MovePosition(rb.position + new Vector2(0, -0.01f));
        }

        count++;
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

        rb.MovePosition(rb.position + (direction.normalized * kbStrength * kbDirectionalBias)); ; //needs to use proper colliison calculation at some point.
	}
}
