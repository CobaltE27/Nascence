using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttacker
{
    /// <summary>
    /// Returns whether this enemy is currently in an attck coroutine.
    /// </summary>
    bool IsAttacking();
}
