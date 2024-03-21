using System;
using System.Collections;
using System.Collections.Specialized;
using UnityEngine;

public abstract class Formation : MonoBehaviour
{
	/// <summary>
	/// Displacement from center of puppetteer to "center" of formation
	/// </summary>
	private Vector2 displacementFromCenter;
    /// <summary>
    /// Enemy positions relative to the formation center are keyed by the enemy itself.
    /// </summary>
    private OrderedDictionary positions;

    public Formation(ref Vector2 displacementFromCenter)
    {
        this.displacementFromCenter = displacementFromCenter;
        positions = new OrderedDictionary();
	}

    public abstract void AddPuppet(Enemy puppet);

    public abstract void RemovePuppet(Enemy puppet);

    /// <summary>
    /// Gets the formation position of the specified enemy relative to puppeteer center.
    /// </summary>
    /// <param name="puppet"></param>
    public virtual Vector2 FormationPositionOf(Enemy puppet)
    {
        System.Object formationPos = positions[puppet];
        if (formationPos is Vector2)
            return (Vector2) formationPos + displacementFromCenter;

        throw new Exception("Formation had a position stored as something other than a Vector!");
    }
}
