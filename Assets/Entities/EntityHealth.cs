﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityHealth : MonoBehaviour
{
    public int health = 5;
    public int maxHealth = 5;
    public EntityMovement movement;
    public int immunityFrames = 50;
    protected Coroutine iFrameCounter;
    protected bool immune = false;
	public int steamOnHit = 50;

	public virtual void Start()
    {
        //nothing for now
    }

    public virtual void DealDamage(int damage, Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
    {
        if (!immune)
        {
            health -= damage;
            if (health <= 0)
            {
                Destroy(this.gameObject);
            }

            movement.ReceiveKnockback(direction, kbStrengthMult);
            if (iFrameCounter != null)
                StopCoroutine(iFrameCounter);
            iFrameCounter = StartCoroutine(ImmunityCounter());
        }
    }

    protected IEnumerator ImmunityCounter()
    {
        immune = true;
        int framesLeft = immunityFrames;
        while (framesLeft > 0)
        {
            framesLeft--;
            yield return new WaitForFixedUpdate();
            if (this == null)
                yield break;
        }

        immune = false;
        yield break;
    }
}
