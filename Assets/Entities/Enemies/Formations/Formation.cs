using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public abstract class Formation : MonoBehaviour
{
	/// <summary>
	/// Displacement from center of puppetteer to "center" of formation
	/// </summary>
	protected Vector2 displacementFromCenter;
    /// <summary>
    /// Enemy positions relative to the formation center are keyed by the enemy itself.
    /// </summary>
    protected OrderedDictionary positions;
    /// <summary>
    /// Attribute interfaces that are required for enemies in this formation
    /// </summary>
    public HashSet<Type> Attributes { get; private set; }

    public Formation(ref Vector2 displacementFromCenter, HashSet<Type> attributes = null)
    {
        attributes = attributes ?? new HashSet<Type>(); //lets attributes be an optional argument

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
        if (positions[puppet] is Vector2 formationPos)
            return (Vector2) formationPos + displacementFromCenter;

        throw new Exception("Formation had a position stored as something other than a Vector!");
    }

    /// <summary>
    /// Checks if the suspect enemy has the right attributes for this formation
    /// </summary>
    public bool HasCorrectAttributes(Enemy suspect)
    {
        foreach (Type attribute in Attributes)
            if (!attribute.Equals(suspect)) //checks that suspect's type is equal to attribute's type.
                return false;
        return true;
    }
}
