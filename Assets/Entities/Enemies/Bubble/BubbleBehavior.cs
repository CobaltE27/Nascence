using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleBehavior : EntityMovement
{
	public int lifeFramesLeft = 400;
	public int startupFramesLeft = 50;
	public float initVelMult = 1.0f;
	public Collider2D hurtBox;
	public SpriteRenderer sprite;

	protected override void Start()
	{
		base.Start();

		hurtBox.enabled = false;
		UnityEngine.Color newSpriteColor = sprite.color;
		newSpriteColor.a = 0.5f;
		sprite.color = newSpriteColor;
	}

	void FixedUpdate()
	{
		mover.persistentVel *= 0.93f;

		startupFramesLeft--;
		if (startupFramesLeft == 0)
		{
			hurtBox.enabled = true;
			UnityEngine.Color newSpriteColor = sprite.color;
			newSpriteColor.a = 1f;
			sprite.color = newSpriteColor;
		}

		lifeFramesLeft--;
		if (lifeFramesLeft == 0)
			Destroy(this.gameObject);
	}

	public void InitVelocity(Vector2 velocity)
	{
		mover.persistentVel = velocity * initVelMult;
	}

	public override void ReceiveKnockback(Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
	{
		//override base behavior to do nothing do nothing
	}
}
