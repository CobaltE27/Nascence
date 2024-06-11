using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinHealth : EntityHealth
{
	public override void DealDamage(int damage, Vector2 direction = default, float kbStrengthMult = 1)
	{
		movement.ReceiveKnockback(direction, kbStrengthMult);
		immune = true;
	}
}
