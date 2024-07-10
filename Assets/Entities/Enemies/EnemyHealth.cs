using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class EnemyHealth : EntityHealth
{
	public override void EnvironmentalDamage(int damage, Vector2 direction)
	{
		if (!immune)
		{
			health -= damage;
			if (health <= 0)
			{
				Destroy(this.gameObject);
			}

			movement.ReceiveKnockback(direction, 2);
			if (iFrameCounter != null)
				StopCoroutine(iFrameCounter);
			iFrameCounter = StartCoroutine(ImmunityCounter());
		}
	}
}
