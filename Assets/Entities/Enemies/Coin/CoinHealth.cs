using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinHealth : EnemyHealth
{
	public override void DealDamage(int damage, Vector2 direction = default, float kbStrengthMult = 1)
	{
		if (!immune)
		{
			//no health
			movement.ReceiveKnockback(direction, kbStrengthMult);
			immune = true;
		}
	}
}
