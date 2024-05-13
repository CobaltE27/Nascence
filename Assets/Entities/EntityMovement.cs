using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityMovement : MonoBehaviour
{
    public EntityMover mover;
	protected int KB_DURATION_FRAMES = 10;
	protected int kbDurationLeft = 0;
	protected float kbWeaknessMult = 1.0f;
	protected float KB_DISTANCE = 1.0f;
	protected Vector2 kbDirectionalBias = Vector2.one;
	protected Coroutine activeKb;

	protected virtual void Start()
    {
		mover.constantVels.Add("kbVelocity", new Vector2());
	}

	/// <summary>
	/// Method that should be called by another component when this entity has been hit or otherwise received kb.
	/// </summary>
    public virtual void ReceiveKnockback(Vector2 direction = new Vector2(), float kbStrengthMult = 1.0f)
    {
		if (activeKb != null)
			StopCoroutine(activeKb);
		activeKb = StartCoroutine(ApplyKnockback(direction, kbStrengthMult));
    }

    protected IEnumerator ApplyKnockback(Vector2 direction, float kbStrengthMult = 1.0f)
    {
		direction.Normalize();
		int kbDurationLeft = KB_DURATION_FRAMES;
		Vector2 kbVel = direction * KB_DISTANCE * kbStrengthMult * kbWeaknessMult / (Time.deltaTime * KB_DURATION_FRAMES);
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
