using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHealth : EnemyHealth
{
	public override void DealDamage(int damage, Vector2 direction = default, float kbStrengthMult = 1)
	{
		if (damage >= 5) //resists any amount of mini-hits at base damage
			Destroy(this.gameObject);
	}
}
