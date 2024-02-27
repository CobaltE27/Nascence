using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMover : MonoBehaviour
{
    private CollisionCalculator collCalc;
    private Rigidbody2D rb;
    public Vector2 persistentVel;
    public Dictionary<string, Vector2> constantVels;

    void Start()
    {
        persistentVel = new Vector2();
        constantVels = new Dictionary<string, Vector2>();
		collCalc = GetComponent<CollisionCalculator>(); 
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 frameDisplacement = collCalc.MoveAndSlideRedirectVelocity(ref persistentVel, Time.deltaTime);
        foreach (Vector2 vel in constantVels.Values)
            frameDisplacement += collCalc.MoveAndSlide(vel, Time.deltaTime);

        rb.MovePosition(rb.position + frameDisplacement);
    }
}
