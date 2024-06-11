using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtility : object
{
	/// <summary>
	/// This function takes the provided "initial" vector and deflects it using the provided
	/// "normal" vector. This is similar to deflecting after collision against a surface, where
	/// any magnitude of the initial vector in the opposite of the normal direction is removed.
	/// </summary>
	/// <param name="initial"></param>
	/// <param name="normal"></param>
	/// <returns></returns>
	public static Vector2 DeflectWithNormal(Vector2 initial, Vector2 normal)
	{
		float initMagnitude = Mathf.Abs(Vector2.Distance(new Vector2(0, 0), initial));
		float angleToInitial = Vector2.SignedAngle(Vector2.right, initial) * (Mathf.PI / 180);
		float angleToNormal = Vector2.SignedAngle(Vector2.right, normal) * (Mathf.PI / 180);
		float angleToDeflected = angleToNormal + ((Mathf.PI * 0.5f) * Mathf.Sign(angleToInitial));

		float normalInitialDiff = Mathf.Abs(angleToNormal - angleToInitial);
		if (normalInitialDiff < Mathf.PI / 2) //The initial vector's angle is not far enough from the normal's to get deflected
			return initial;


		return new Vector2(initMagnitude * Mathf.Cos(angleToInitial - angleToDeflected) * Mathf.Cos(angleToDeflected),
			initMagnitude * Mathf.Cos(angleToInitial - angleToDeflected) * Mathf.Sin(angleToDeflected));
	}

	/// <summary>
	/// Reflects the initial vector off of the surface to which the normal vector belongs.
	/// </summary>
	/// <param name="normal">Doesn't need to be normalized</param>
	/// <param name="restitution">Number from 0 to 1 that multiplies the reflected component of the outgoing vector</param>
	public static Vector2 ReflectOffNormal(Vector2 initial, Vector2 normal, float restitution = 1.0f)
	{
		normal.Normalize();
		float dotProd = Vector2.Dot(initial, normal);
		if (dotProd > 0) //initial vector is coming from behind normal's surface or doesn't contact it
			return initial;

		return initial + ((1 + restitution) * -dotProd * normal);
	}
}
