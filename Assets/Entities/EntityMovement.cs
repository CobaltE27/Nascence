using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityMovement : MonoBehaviour
{
    EntityMover mover;
	int KB_DURATION_FRAMES;
	float KB_DISTANCE;
	Vector2 kbDirectionalBias = Vector2.one;

	protected virtual void Start()
    {
		mover.constantVels.Add("kbVelocity", new Vector2());
	}
    public virtual void ReceiveKnockback(Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
    {
		StartCoroutine(ApplyKnockback(direction, kbStrengthMult));
    }

    protected IEnumerator ApplyKnockback(Vector2 direction, float kbStrengthMult = 1.0f)
    {
		direction.Normalize();
		int kbDurationLeft = KB_DURATION_FRAMES;
		Vector2 kbVel = direction * KB_DISTANCE * kbStrengthMult / (Time.deltaTime * KB_DURATION_FRAMES);
		kbVel *= kbDirectionalBias;

		mover.constantVels["kbVelocity"] = kbVel;
		while (kbDurationLeft > 0)
		{
			kbDurationLeft--;
			yield return new WaitForFixedUpdate();
		}

		mover.constantVels["kbVelocity"] *= 0;
		yield break;
	}
}
