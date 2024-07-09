using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class manages movement and collisions for any object that has a collider
/// </summary>
public class CollisionCalculator : MonoBehaviour
{
    private float WALKABLE_ANGLE_THRESHOLD;
    private MovementCastUtility castUtils;
    public Collider2D parentCollider;

    void Start()
    {
        if (!parentCollider)
		    parentCollider = GetComponent<Collider2D>();
		castUtils = new MovementCastUtility(parentCollider);
        
        WALKABLE_ANGLE_THRESHOLD = 30.0f;
    }

    public Vector2 MoveAndSlideRedirectVelocity(ref Vector2 parentVelocity, float deltaTime)
    {
        Vector2 attemptedDisplacement = parentVelocity * deltaTime; //begins as parent displacement
        RaycastHit2D[] predictCastHits = castUtils.DisplacementCast(attemptedDisplacement);
        Vector2 dispToCollision = castUtils.DistanceToCollider(predictCastHits, attemptedDisplacement);

        if (!predictCastHits[0]) //hit nothing, can move unimpeded
            return attemptedDisplacement;

		Vector2 collisionNormal = castUtils.FirstCastNormal(predictCastHits);
		float collisionNormalAngle = Vector2.SignedAngle(new Vector2(1, 0), collisionNormal);

		parentVelocity = RedirectWithNormal(parentVelocity, collisionNormalAngle);

		dispToCollision += collisionNormal * 0.01f;
		return dispToCollision;
	}

    /// <summary>
    /// Gives the displacement that something moving at the given velocity should move in one frame, accounting for collisions.
    /// Does not redirect parent velocity.
    /// </summary>
    /// <param name="moveVelocity"></param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
	public Vector2 MoveAndSlide(Vector2 moveVelocity, float deltaTime)
	{
		Vector2 attemptedDisplacement = moveVelocity * deltaTime; //begins as parent displacement
		RaycastHit2D[] predictCastHits = castUtils.DisplacementCast(attemptedDisplacement);
		Vector2 dispToCollision = castUtils.DistanceToCollider(predictCastHits, attemptedDisplacement);

		if (!predictCastHits[0]) //hit nothing, can move unimpeded
			return attemptedDisplacement;

		Vector2 collisionNormal = castUtils.FirstCastNormal(predictCastHits);

		dispToCollision += collisionNormal * 0.01f;
		return dispToCollision;
	}

	public bool IsOnWalkableGround()
    {
        if (castUtils.DisplacementCast(new Vector2(0, -0.02f))[0].collider == null)
            return false;
        return Vector2.Angle(Vector2.up, castUtils.FirstCastNormal(castUtils.DisplacementCast(new Vector2(0, -0.02f)))) <= WALKABLE_ANGLE_THRESHOLD;
    }

    public bool BelowFlatCeiling()
    {
        return castUtils.FirstCastNormal(castUtils.DisplacementCast(new Vector2(0, 0.02f))) == Vector2.down;
    }

    public bool NextToRightWall()
    {
        return castUtils.FirstCastNormal(castUtils.DisplacementCast(new Vector2(0.02f, 0))) == Vector2.left;
    }

    public bool NextToLeftWall()
    {
		return castUtils.FirstCastNormal(castUtils.DisplacementCast(new Vector2(-0.02f, 0))) == Vector2.right;
    }

    public bool OnRightSlope()
    {
        if (castUtils.DisplacementCast(new Vector2(0, -0.02f))[0].collider == null)
            return false;
        float slopeNormalAngleFromUp = Vector2.SignedAngle(Vector2.up, castUtils.FirstCastNormal(castUtils.DisplacementCast(new Vector2(0.02f, -0.02f))));
        return slopeNormalAngleFromUp > WALKABLE_ANGLE_THRESHOLD && slopeNormalAngleFromUp < 90.0f;
    }

    public bool OnLeftSlope()
    {
        if (castUtils.DisplacementCast(new Vector2(0, -0.02f))[0].collider == null)
            return false;
        float slopeNormalAngleFromUp = Vector2.SignedAngle(Vector2.up, castUtils.FirstCastNormal(castUtils.DisplacementCast(new Vector2(-0.02f, -0.02f))));
        return slopeNormalAngleFromUp < -WALKABLE_ANGLE_THRESHOLD && slopeNormalAngleFromUp > -90.0f;
    }

    /// <summary>
	/// Takes the remaining displacement after a collision, and redirects it perpendicular to the collision normal,
	/// which will direct it along the surface of what it collides with.
	/// </summary>
	/// <param name="remainingDisplacement"></param>
	/// <returns> The redirected displacement</returns>
    private Vector2 RedirectWithNormal(Vector2 remainingDisplacement, float normalAngle)
	{
        float remDisMagnitude = Mathf.Abs(Vector2.Distance(new Vector2(0, 0), remainingDisplacement));
        float angleToRemaining = Vector2.SignedAngle(new Vector2(1, 0), remainingDisplacement);
        angleToRemaining *= (Mathf.PI / 180);
        float angleToNormal = normalAngle * (Mathf.PI / 180);
        float angleToCorrected = angleToNormal + ((Mathf.PI * 0.5f) * Mathf.Sign(angleToRemaining));


        return new Vector2(remDisMagnitude * Mathf.Cos(angleToRemaining - angleToCorrected) * Mathf.Cos(angleToCorrected),
            remDisMagnitude * Mathf.Cos(angleToRemaining - angleToCorrected) * Mathf.Sin(angleToCorrected));
    }
}
