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
    private Collider2D parentCollider;

    public CollisionCalculator(Collider2D parentCollider)
    {
        castUtils = new MovementCastUtility(parentCollider);
        this.parentCollider = parentCollider;
        WALKABLE_ANGLE_THRESHOLD = 30.0f;
    }

    public Vector2 MoveAndSlideRedirectVelocity(Vector2 parentVelocity, float deltaTime)
    {
        Vector2 remainingDisplacement = parentVelocity * deltaTime; //begins as parent displacement
        Vector2 rectifiedDisplacement = new Vector2(0, 0);
        RaycastHit2D[] predictCastResults = castUtils.DisplacementCast(remainingDisplacement);
        Vector2 distToCollision = castUtils.DistanceToCollider(predictCastResults, remainingDisplacement);

        bool displacementNeedsRectification = predictCastResults[0].collider != null;

        if (!displacementNeedsRectification)
            return remainingDisplacement;

        int terminator = 0;
        Vector2 collisionNormal;
        float collisionNormalAngle;
        while (displacementNeedsRectification && terminator < 3)
		{
            remainingDisplacement -= distToCollision;

            collisionNormal = castUtils.FirstCastNormal(predictCastResults);
            collisionNormalAngle = Vector2.SignedAngle(new Vector2(1, 0), collisionNormal);
            distToCollision += new Vector2(0.01f * Mathf.Cos(collisionNormalAngle * (Mathf.PI / 180)), 0.01f * Mathf.Sin(collisionNormalAngle * (Mathf.PI / 180)));

            rectifiedDisplacement += distToCollision;

            remainingDisplacement = RedirectWithNormal(remainingDisplacement, collisionNormalAngle);

            parentVelocity = RedirectWithNormal(parentVelocity, collisionNormalAngle);
            parentCollider.gameObject.GetComponent<CharMovement>().velocity = parentVelocity;

            parentCollider.offset.Set(distToCollision.x, distToCollision.y);
            predictCastResults = castUtils.DisplacementCast(remainingDisplacement);
            displacementNeedsRectification = predictCastResults[0].collider != null;
            distToCollision = castUtils.DistanceToCollider(predictCastResults, remainingDisplacement);

            terminator++;
        }

        if (terminator < 3)
		{
            rectifiedDisplacement += remainingDisplacement;
        }
		else
		{
            parentCollider.gameObject.GetComponent<CharMovement>().velocity *= 0.2f;
        }

        parentCollider.offset.Set(0, 0);

        if (!(terminator < 5))
            Debug.Log("CollisionCalculator: Rectification terminated early due to redirecting too many times");
        return rectifiedDisplacement;
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
		Vector2 remainingDisplacement = moveVelocity * deltaTime; //begins as parent displacement
		Vector2 rectifiedDisplacement = new Vector2(0, 0);
		RaycastHit2D[] predictCastResults = castUtils.DisplacementCast(remainingDisplacement);
		Vector2 distToCollision = castUtils.DistanceToCollider(predictCastResults, remainingDisplacement);

		bool displacementNeedsRectification = predictCastResults[0].collider != null;

		if (!displacementNeedsRectification)
			return remainingDisplacement;

		int terminator = 0;
		Vector2 collisionNormal;
		float collisionNormalAngle;
		while (displacementNeedsRectification && terminator < 3)
		{
			remainingDisplacement -= distToCollision;

			collisionNormal = castUtils.FirstCastNormal(predictCastResults);
			collisionNormalAngle = Vector2.SignedAngle(new Vector2(1, 0), collisionNormal);
			distToCollision += new Vector2(0.01f * Mathf.Cos(collisionNormalAngle * (Mathf.PI / 180)), 0.01f * Mathf.Sin(collisionNormalAngle * (Mathf.PI / 180)));

			rectifiedDisplacement += distToCollision;

			remainingDisplacement = RedirectWithNormal(remainingDisplacement, collisionNormalAngle);

			parentCollider.offset.Set(distToCollision.x, distToCollision.y);
			predictCastResults = castUtils.DisplacementCast(remainingDisplacement);
			displacementNeedsRectification = predictCastResults[0].collider != null;
			distToCollision = castUtils.DistanceToCollider(predictCastResults, remainingDisplacement);

			terminator++;
		}

		if (terminator < 3)
		{
			rectifiedDisplacement += remainingDisplacement;
		}

		parentCollider.offset.Set(0, 0);

		if (!(terminator < 5))
			Debug.Log("CollisionCalculator: Rectification terminated early due to redirecting too many times");
		return rectifiedDisplacement;
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
