using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class has many methods which deal with the boxcasting involved in an object's movement and collision.
/// </summary>
public class MovementCastUtility : MonoBehaviour
{
    private Collider2D parentCollider;

    public MovementCastUtility(Collider2D parentCollider)
    {
        this.parentCollider = parentCollider;
    }

    /// <summary>
    /// returns the a vector of the y dist to the farthest(change eventually) floor and x dist to the farthest(ditto) wall.
    /// This is the distance between this center of this object's collider, and the center of where the collider
    /// would be when it hit another collider after being cast outward.
    /// </summary>
    /// <param name="castResults"></param>
    /// <returns>Vector2D(distToWall, distToFloor), the new displacement vector</returns>
    public Vector2 DistanceToCollider(RaycastHit2D[] castResults, Vector2 displacement)
    {
        Vector2 distanceToCollision = new Vector2(displacement.x, displacement.y);

        if(castResults[0].collider != null)
		{
            distanceToCollision *= castResults[0].fraction;
        }

        return distanceToCollision;
    }

    /// <summary>
    /// traverses an array of RaycastHit2D's and looks at the normal of the collider they hit, 
    /// returns true if ANY hit colliders have an upward pointing normal(are a floor)
    /// </summary>
    /// <param name="castResults"></param>
    /// <returns></returns>
    public bool DidCastDetectFloor(RaycastHit2D[] castResults)
    {
        foreach (RaycastHit2D castResult in castResults)
        {
            //Debug.Log(castResults[i].normal);
            if (castResult.normal == new Vector2(0, 1))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// traverses an array of RaycastHit2D's and looks at the normal of the collider they hit, 
    /// returns true if ANY hit colliders have an horizontal pointing normal(are a wall)
    /// </summary>
    /// <param name="castResults"></param>
    /// <returns></returns>
    public bool DidCastDetectWall(RaycastHit2D[] castResults)
    {
        foreach (RaycastHit2D castResult in castResults)
        {
            if (Vector2.Angle(castResult.normal, new Vector2(0, 1)) == 90)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
	/// DEPRECIATED
    /// Returns the average normal of all the colliders hit by this cast.
    /// </summary>
    /// <param name="castResults"></param>
    /// <returns></returns>
    public Vector2 AverageCastNormal(RaycastHit2D[] castResults)
    {
        Vector2 sum = new Vector2();
        foreach (RaycastHit2D castResult in castResults)
        {
            sum += castResult.normal;
        }

        sum.Normalize();
        return sum;
    }

    /// <summary>
    /// Returns the first normal of all the colliders hit by the cast
    /// </summary>
    /// <param name="castResults"></param>
    /// <returns></returns>
    public Vector2 FirstCastNormal(RaycastHit2D[] castResults)
    {
        Vector2 result = castResults[0].normal;
        result.Normalize();
        return result;
    }

    /// <summary>
    /// Searches through an array of RaycastHit2D to determine which one hit a wall
    /// </summary>
    /// <param name="results"></param>
    /// <returns>The RaycastHit2D which hit a wall</returns>
    public RaycastHit2D WhichCastDetectedWall(RaycastHit2D[] results)
    {
        foreach (RaycastHit2D result in results)
        {
            if (Vector2.SignedAngle(result.normal, new Vector2(0, 1)) == 90)
            {
                return result;
            }
        }
        throw new ArgumentException("No casts in the provided array actually hit a wall.");
    }

    /// <summary>
    /// Searches through an array of RaycastHit2D to determine which one hit a floor
    /// </summary>
    /// <param name="results"></param>
    /// <returns>The RaycastHit2D which hit a floor</returns>
    public RaycastHit2D WhichCastDetectedFloor(RaycastHit2D[] results)
    {
        foreach (RaycastHit2D result in results)
        {
            if (result.normal == new Vector2(0, 1))
            {
                return result;
            }
        }
        throw new ArgumentException("No casts in the provided array actually hit a floor.");
    }

    /// <summary>
    /// This casts the parent's collider forward using their displacement
    /// This must be done at some point in the current frame before using other functions from this class.
    /// </summary>
    /// <param name="displacement"></param>
    /// <returns>The array of RaycastHit2D that results from the cast.</returns>
    public RaycastHit2D[] DisplacementCast(Vector2 displacement)
    {
        RaycastHit2D[] castResults = new RaycastHit2D[10];
        parentCollider.Cast(displacement, castResults, displacement.magnitude);

        //foreach (RaycastHit2D result in castResults)
        //{
        //    if (result.normal != new Vector2(0, 0))
        //        Debug.Log(parentCollider.ToString() + " with normals " + result.normal);
        //}

        return castResults;
    }
}