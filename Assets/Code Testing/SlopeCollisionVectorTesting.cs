using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeCollisionVectorTesting : MonoBehaviour
{
    public Vector2 normal = new Vector2(0, 1);
    public Vector2 remainingDisplacement = new Vector2(-2, -2);
    public Vector2 correctedDisplacement = new Vector2();
    public float remDisMagnitude;
    public float angleToRemaining;
    public float angleToNormal;
    public float angleToCorrected;

    //remember that Mathf trif functions use radians 
    void Start()
    {
        /*
        remDisMagnitude = Mathf.Abs(Vector2.Distance(new Vector2(0, 0), remainingDisplacement));
        angleToRemaining = Vector2.SignedAngle(new Vector2(0, 1), remainingDisplacement);
        angleToNormal = Vector2.SignedAngle(new Vector2(0, 1), normal);
        angleToCorrected = angleToNormal + (90 * Mathf.Sign(angleToRemaining));

        Debug.Log("A: rem to cor - " + (angleToRemaining - angleToCorrected));
        Debug.Log("A: to cor - " + angleToCorrected);

        angleToRemaining *= (Mathf.PI / 180);
        angleToNormal *= (Mathf.PI / 180);
        angleToCorrected *= (Mathf.PI / 180);

        Debug.Log("C: cos(cor) - " + Mathf.Cos(angleToCorrected));
        Debug.Log("C: sin(cor) - " + Mathf.Sin(angleToCorrected));

        correctedDisplacement.Set(remDisMagnitude * Mathf.Cos(angleToRemaining - angleToCorrected) * -Mathf.Sin(angleToCorrected),
            remDisMagnitude * Mathf.Cos(angleToRemaining - angleToCorrected) * Mathf.Cos(angleToCorrected));

        // code above this line uses angle the mesure from (0, 1) as 0 instead of (1, 0)
        */

        remDisMagnitude = Mathf.Abs(Vector2.Distance(new Vector2(0, 0), remainingDisplacement));
        angleToRemaining = Vector2.SignedAngle(new Vector2(1, 0), remainingDisplacement);
        angleToNormal = Vector2.SignedAngle(new Vector2(1, 0), normal);
        angleToCorrected = angleToNormal + (90 * Mathf.Sign(angleToRemaining));

        Debug.Log("A: rem to cor - " + (angleToRemaining - angleToCorrected));
        Debug.Log("A: to cor - " + angleToCorrected);

        angleToRemaining *= (Mathf.PI / 180);
        angleToNormal *= (Mathf.PI / 180);
        angleToCorrected *= (Mathf.PI / 180);

        Debug.Log("C: cos(cor) - " + Mathf.Cos(angleToCorrected));
        Debug.Log("C: sin(cor) - " + Mathf.Sin(angleToCorrected));

        correctedDisplacement.Set(remDisMagnitude * Mathf.Cos(angleToRemaining - angleToCorrected) * Mathf.Cos(angleToCorrected),
            remDisMagnitude * Mathf.Cos(angleToRemaining - angleToCorrected) * Mathf.Sin(angleToCorrected));
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(new Vector2(0, 0), normal, Color.white, 0.1f);
        Debug.DrawRay(new Vector2(0, 0), remainingDisplacement, Color.red, 0.1f);
        Debug.DrawRay(new Vector2(0, 0), correctedDisplacement, Color.green, 0.1f);
    }
}
