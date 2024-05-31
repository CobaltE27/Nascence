using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehavior : EntityMovement
{
	public float GRAVITY = -0.4f;
	public float MAX_FALL = -15.0f;
	public int lifeFramesLeft = 100;

	void FixedUpdate()
	{
		
		mover.persistentVel.y += GRAVITY;
		if (mover.persistentVel.y < MAX_FALL)
			mover.persistentVel.y = MAX_FALL;


		lifeFramesLeft--;
		if (lifeFramesLeft <= 0)
			Destroy(this.gameObject);
	}

    public void InitVelocity(Vector2 velocity)
    {
		mover.persistentVel = velocity;
    }

	public override void ReceiveKnockback(Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
	{
		mover.persistentVel = direction * kbStrengthMult * 24.0f;
	}
}
