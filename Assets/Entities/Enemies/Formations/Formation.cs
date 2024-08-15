using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
	public List<EnemyMovement> Puppets { get; protected set; }
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
        Puppets = new List<EnemyMovement>();
	}

    public abstract void AddPuppet(EnemyMovement puppet);

    public abstract void RemovePuppet(EnemyMovement puppet);

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
    public Vector2 FormationPositionOf(EnemyMovement puppet)
    {
        return CenterOfFormations + DisplacementFromCenter + positions[Puppets.IndexOf(puppet)];
    }

    /// <summary>
    /// Checks if the suspect enemy has the right attributes for this formation
    /// </summary>
    public bool HasCorrectAttributes(EnemyMovement suspect)
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

    /// <summary>
    /// Optionally overridden method that uses a formation's current position and the player's position to choose a place where it would rather be.
    /// If a formation isn't made for a specific position, it just returns its current position and logs it's lack of preference.
    /// </summary>
    public virtual Vector2 PreferredPosition(Vector2 playerPosition)
    {
        Debug.Log("This formation has no preference!");
        return CenterOfFormations + DisplacementFromCenter; //if nothing else is implemented, just returns the current position
    }

    /// <summary>
    /// Reorders the puppets list to minimize travel from puppets to positions in the list.
    /// Good to use any time the position list changes.
    /// </summary>
    protected void ReassignPuppetPositions()
    {
		Vector2 formationCenterWorldCoords = DisplacementFromCenter + CenterOfFormations;
		Vector2[] posCopy = new Vector2[positions.Count()];
		positions.CopyTo(posCopy);
		List<Vector2> positionsCopy = new List<Vector2>(posCopy);
		List<EnemyMovement> enemiesByDistance = Puppets.OrderByDescending(o => Vector2.Distance(formationCenterWorldCoords, o.transform.position)).ToList();

		EnemyMovement[] newPuppetsList = new EnemyMovement[Puppets.Count];
		for (int i = 0; i < enemiesByDistance.Count(); i++) //Give the farthest enemy its closest position
		{
			positionsCopy = positionsCopy.OrderBy(o => Vector2.Distance(o + formationCenterWorldCoords, enemiesByDistance[i].transform.position)).ToList();
			Vector2 nearestPos = positionsCopy.First();
			positionsCopy.RemoveAt(0);
			newPuppetsList[positions.IndexOf(nearestPos)] = enemiesByDistance[i];
		}

		Puppets = new List<EnemyMovement>(newPuppetsList);
	}
}
