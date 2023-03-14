﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumConstants;
using System;

/// <summary>
/// This class provides many of the same utilities as the MovementCastUtility Class, but uses Physics2D.BoxCastAll() or similar methods instead
/// of casting a set collider.
/// </summary>
public class CastUtility : object
{

    public CastUtility() { }

    /// <summary>
    /// returns the a vector of the distance to the nearest surface along a displacement path.
    /// This is the distance between the center of the given collision shape and
	/// the center of that shape's location when it collides with something.
    /// </summary>
    /// <param name="castResults"></param>
    /// <returns>Vector2D(distToWall, distToFloor), the new displacement vector</returns>
    public Vector2 DistanceToCollider(RaycastHit2D[] castResults, Vector2 displacement)
    {
        Vector2 distanceToCollision = new Vector2(displacement.x, displacement.y);

        if (castResults[0].collider != null)
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

        throw new ArgumentException("None of the given casts hit walls.");
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

        throw new ArgumentException("None of the given casts hit floors.");
    }

    /// <summary>
    /// This casts a given shape forward along a given displacement.
    /// The result may then be used by the other methods in this class
	/// to determine properties about the result of the cast.
	/// Layer, shape, shape angle, and z-depth can all be specified.
    /// </summary>
    /// <param name="displacement"> The displacement from the origin to the end of where the given shape should be cast</param>
	/// <param name="castShape"></param>
    /// <returns>The array of RaycastHit2D that results from the cast.</returns>
    public RaycastHit2D[] DisplacementShapeCast(Vector2 origin, Vector2 displacement, Collider2D castShape, EnumConstants.Layer layer, float angle = 0.0f, float minZDepth = 0, float maxZDepth = 0)
    {
        if (castShape is BoxCollider2D)
		{
            BoxCollider2D castBox = (BoxCollider2D)castShape;
            return Physics2D.BoxCastAll(origin, castBox.size, angle, displacement, displacement.magnitude, (int)layer, minZDepth, maxZDepth);
        }
        if (castShape is CircleCollider2D)
		{
            CircleCollider2D castCircle = (CircleCollider2D)castShape;
            return Physics2D.CircleCastAll(origin, castCircle.radius, displacement, displacement.magnitude, (int)layer, minZDepth, maxZDepth);
        }
        if (castShape is CapsuleCollider2D)
        {
            CapsuleCollider2D castCapsule = (CapsuleCollider2D)castShape;
            return Physics2D.CapsuleCastAll(origin, castCapsule.size, castCapsule.direction, angle, displacement, displacement.magnitude, (int)layer, minZDepth, maxZDepth);
        }

        throw new ArgumentException("castShape must be a BoxCollider2D, CircleCollider2D, or CapsuleCollider2D!");
    }
}
