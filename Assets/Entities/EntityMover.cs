using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMover : MonoBehaviour
{
    public CollisionCalculator collCalc;
    public Rigidbody2D rb;
    public Vector2 persistentVel = new Vector2();
	public Dictionary<string, Vector2> constantVels = new Dictionary<string, Vector2>();

    void Start()
    {
		collCalc = GetComponent<CollisionCalculator>(); 
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 displacement = collCalc.MoveAndSlideRedirectVelocity(ref persistentVel, Time.deltaTime);
        foreach (Vector2 vel in constantVels.Values)
            displacement += collCalc.MoveAndSlide(vel, Time.deltaTime);

        rb.MovePosition(rb.position + displacement);
    }
}
