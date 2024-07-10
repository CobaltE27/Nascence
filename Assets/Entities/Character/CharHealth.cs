using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharHealth : EntityHealth
{
	public HudUi hud;

	public override void Start()
	{
		base.Start();
		hud.UpdateHealth(health);
	}
	public override void DealDamage(int damage, Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
	{
		if (!immune)
		{
			TakeDamage(damage);

			movement.ReceiveKnockback(direction, kbStrengthMult);
			if (iFrameCounter != null)
				StopCoroutine(iFrameCounter);
			iFrameCounter = StartCoroutine(ImmunityCounter());
		}
	}

	public override void EnvironmentalDamage(int damage, Vector2 direction)
	{
		if (!immune)
		{
			TakeDamage(damage);

			((CharMovement)movement).GoToLastSafe();
			if (iFrameCounter != null)
				StopCoroutine(iFrameCounter);
			iFrameCounter = StartCoroutine(ImmunityCounter());
		}
	}

	private void TakeDamage(int damage)
	{
		health -= damage;
		if (health < 0)
			health = 0;
		hud.UpdateHealth(health);

		if (health <= 0)
		{
			//something other than destroy
		}
	}
}
