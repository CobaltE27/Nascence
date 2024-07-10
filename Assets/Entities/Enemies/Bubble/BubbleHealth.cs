using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHealth : EntityHealth
{
	public override void DealDamage(int damage, Vector2 direction = default, float kbStrengthMult = 1)
	{
		if (damage >= 5) //resists any amount of mini-hits at base damage
			Destroy(this.gameObject);
	}

	public override void EnvironmentalDamage(int damage, Vector2 direction)
	{
		//maybe it's better if this doesn't care about hazards?
		Destroy(this.gameObject);
	}
}
