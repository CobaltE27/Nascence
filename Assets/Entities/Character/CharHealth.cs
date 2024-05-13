using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharHealth : EntityHealth
{
	protected override void DealDamage(float damage, Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
	{
		health -= damage;
		if (health <= 0)
		{
			//something other than destroy?
		}

		movement.ReceiveKnockback(direction, kbStrengthMult);
	}
}
