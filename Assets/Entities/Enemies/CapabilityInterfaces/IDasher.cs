using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDasher : IAttacker
{
    /// <summary>
    /// Coroutine that performs a dash-attack toward the specified position.
    /// </summary>
    /// <param name="target">The position to dash toward</param>
    IEnumerator DashToward(Vector2 target);
}
