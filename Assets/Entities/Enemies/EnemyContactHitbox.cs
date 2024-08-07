using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContactHitbox : MonoBehaviour
{ 
    public Collider2D hitBox;
    public int DAMAGE = 1;
    
    void FixedUpdate()
    {
		RaycastHit2D[] hitDetectResults = PhysicsCastUtility.DisplacementShapeCast((Vector2)transform.position, Vector2.zero, hitBox,
		   new string[] { "Hurtbox" });

        foreach (RaycastHit2D hit in hitDetectResults)
        {
            if (hit.collider == null) //hit nothing
                continue;

            if (!hit.collider.gameObject.CompareTag("Player"))
                continue;

			EntityHealth hitHealth = hit.collider.transform.parent.GetComponent<EntityHealth>();
            
			Vector2 hitDir = Vector2.up;
			Vector2 dispToHit = hit.collider.transform.position - transform.position;
            if (dispToHit.x < 0) //hit entity is the the left
                hitDir += Vector2.left;
            else
                hitDir += Vector2.right;
            
            hitHealth.DealDamage(DAMAGE, hitDir);
        }
	}
}
