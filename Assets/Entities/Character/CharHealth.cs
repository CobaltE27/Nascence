﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharHealth : EntityHealth
{
	public override void DealDamage(int damage, Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
	{
		if (!immune)
		{
			health -= damage;
			if (health < 0)
				health = 0;

			if (health <= 0)
			{
				//something other than destroy
			}

			movement.ReceiveKnockback(direction, kbStrengthMult);
			if (iFrameCounter != null)
				StopCoroutine(iFrameCounter);
			iFrameCounter = StartCoroutine(ImmunityCounter());
		}
	}
}
