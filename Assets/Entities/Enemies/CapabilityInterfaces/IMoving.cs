using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoving
{
	/// <summary>
	/// Method that can be called each frame as the flier move toward its target
	/// </summary>
	/// <param name="speedMultiplier">Multiplier on this enemy's base speed.</param>
	void MoveTowardTarget(float speedMultiplier);

	/// <summary>
	/// Coroutine which makes the enemy move idly. Should be stopped when puppeted.
	/// </summary>
	IEnumerator Idle();
}
