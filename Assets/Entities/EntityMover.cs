using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMover : MonoBehaviour
{
    public CollisionCalculator collCalc;
    public Vector2 persistentVel = new Vector2();
	public Dictionary<string, Vector2> constantVels = new Dictionary<string, Vector2>();
    public float minimumSpeed = 0.01f;

    void Start()
    {
		collCalc = GetComponent<CollisionCalculator>();
    }

    void FixedUpdate()
    {
        if (Mathf.Abs(persistentVel.x) <= minimumSpeed)
            persistentVel.x = 0;
		if (Mathf.Abs(persistentVel.y) <= minimumSpeed)
			persistentVel.y = 0;

		Vector2 displacement = collCalc.MoveAndSlideRedirectVelocity(ref persistentVel, Time.deltaTime);
        foreach (Vector2 vel in constantVels.Values)
            displacement += collCalc.MoveAndSlide(vel, Time.deltaTime);

        transform.position += (Vector3) displacement;
    }
}
