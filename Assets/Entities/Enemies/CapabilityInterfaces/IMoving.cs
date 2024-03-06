using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoving
{
	/// <summary>
	/// Method that can be called each frame as the flier move toward its target
	/// </summary>
	/// <param name="speedMultiplier">Multiplier on this enemy's base speed.</param>
	void MoveTowardTarget(float speedMultiplier = 1.0f);

	/// <summary>
	/// Coroutine that continually moves this enemy toward its target until it reaches the target position.
	/// </summary>
	/// <param name="speedMultiplier">Multiplier on this enemy's base speed.</param>
	/// /// <param name="margin">Distance from the target this enemy must reach to be considered to have reached it.</param>
	IEnumerator MoveToTarget(float speedMultiplier = 1.0f, float margin = 0.1f);

	/// <summary>
	/// Coroutine which makes the enemy move idly. Should be stopped when puppeted.
	/// </summary>
	IEnumerator Idle();

	/// <summary>
	/// Returns whether this enemy is currently in a movement coroutine
	/// </summary>
	bool IsMoving();
}
