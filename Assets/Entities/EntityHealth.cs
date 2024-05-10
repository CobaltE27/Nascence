using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityHealth : Component
{
    public float health;
    public float maxHealth;
    public EntityMovement movement;

    protected virtual void DealDamage(float damage, Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(this.gameObject);
        }

        movement.ReceiveKnockback(direction, kbStrengthMult);
    }
}
