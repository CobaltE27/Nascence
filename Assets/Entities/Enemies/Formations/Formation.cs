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
	public List<Enemy> Puppets { get; protected set; }
	/// <summary>
	/// Attribute interfaces that are required for enemies in this formation
	/// </summary>
	public HashSet<Type> Attributes { get; private set; }
    protected Vector2 CenterOfFormations { get { return centerObject.transform.position; } }
    protected Transform centerObject;

    public Formation(Vector2 displacementFromCenter, Transform centerOfFormations, HashSet<Type> attributes = null)
    {
        Attributes = attributes ?? new HashSet<Type>(); //lets attributes be an optional argument

        centerObject = centerOfFormations;
        this.DisplacementFromCenter = displacementFromCenter;
        positions = new List<Vector2>();
        Puppets = new List<Enemy>();
	}

    public abstract void AddPuppet(Enemy puppet);

    public abstract void RemovePuppet(Enemy puppet);

    protected abstract void ReevaluatePositions();

	public virtual void PuppetDied()
    {
        List<int> deadIndexes = new List<int>();
        for (int i = 0; i < Puppets.Count; i++)
            if (Puppets[i] == null)
                deadIndexes.Add(i);

        int indexesRemoved = 0;
        foreach (int index in deadIndexes)
        {
            Puppets.RemoveAt(index - indexesRemoved);
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
        return CenterOfFormations + DisplacementFromCenter + positions[Puppets.IndexOf(puppet)];
    }

    /// <summary>
    /// Checks if the suspect enemy has the right attributes for this formation
    /// </summary>
    public bool HasCorrectAttributes(Enemy suspect)
    {
        Type[] a = suspect.GetType().FindInterfaces((t, o) => { return true;}, new object()); //Gets all interfaces using dummy filter
        List<Type> suspectAttributes = new List<Type>(a);

        foreach (Type attribute in Attributes)
            if (!suspectAttributes.Contains(attribute)) //makes sure suspect has all interfaces in Attribute, though it can have extras
                return false;
        return true;
    }

    /// <summary>
    /// Indicates if a certain amount of puppets are within margin of the formation position.
    /// </summary>
    public bool Aligned(float margin = 0.1f, int exceptionsAllowed = 0)
    {
        int alignedCount = 0;
        for (int i = 0; i < Puppets.Count; i++)
        {
            //Debug.Log(Vector2.Distance((Vector2)Puppets[i].transform.position, WorldPositionOf(i)));
            if (Vector2.Distance((Vector2)Puppets[i].transform.position, WorldPositionOf(i)) < margin)
                alignedCount++;
        }

        return alignedCount >= Puppets.Count - exceptionsAllowed;
    }

    /// <summary>
    /// Returns the world position of puppet[i]
    /// </summary>
    private Vector2 WorldPositionOf(int index)
    {
        return CenterOfFormations + DisplacementFromCenter + positions[index];

	}
}
