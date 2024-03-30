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
	public Vector2 DisplacementFromCenter { protected get; set; }
    /// <summary>
    /// Enemy positions relative to the formation center, the index of a position matches the index of its corresponding enemy.
    /// </summary>
    protected List<Vector2> positions;
	/// The index of an enemy matches the index of its corresponding position.
	/// </summary>
	protected List<Enemy> puppets;
	/// <summary>
	/// Attribute interfaces that are required for enemies in this formation
	/// </summary>
	public HashSet<Type> Attributes { get; private set; }
    protected Vector2 CenterOfFormations { get { return centerObject.transform.position; } }
    protected Transform centerObject;

    public Formation(Vector2 displacementFromCenter, Transform centerOfFormations, HashSet<Type> attributes = null)
    {
        attributes = attributes ?? new HashSet<Type>(); //lets attributes be an optional argument

        centerObject = centerOfFormations;
        this.DisplacementFromCenter = displacementFromCenter;
        positions = new List<Vector2>();
        puppets = new List<Enemy>();
	}

    public abstract void AddPuppet(Enemy puppet);

    public abstract void RemovePuppet(Enemy puppet);

    protected abstract void ReevaluatePositions();

	public virtual void PuppetDied()
    {
        List<int> deadIndexes = new List<int>();
        for (int i = 0; i < puppets.Count; i++)
            if (puppets[i] == null)
                deadIndexes.Add(i);

        int indexesRemoved = 0;
        foreach (int index in deadIndexes)
        {
            puppets.RemoveAt(index - indexesRemoved);
			positions.RemoveAt(index - indexesRemoved);
            indexesRemoved++;
		}

        ReevaluatePositions();
	}

    /// <summary>
    /// Gets the formation position of the specified enemy relative to puppeteer center.
    /// </summary>
    /// <param name="puppet"></param>
    public Vector2 FormationPositionOf(Enemy puppet)
    {
        return DisplacementFromCenter + positions[puppets.IndexOf(puppet)];
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
