using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContactHitbox : MonoBehaviour
{ 
    public Collider2D hitBox;
    PhysicsCastUtility caster;
    public float DAMAGE = 5.0f;
    void Start()
    {
        caster = new PhysicsCastUtility();
    }
    void FixedUpdate()
    {
		RaycastHit2D[] hitDetectResults = caster.DisplacementShapeCast((Vector2)transform.position, Vector2.zero, hitBox,
		   new string[] { "Player" });

        foreach (RaycastHit2D hit in hitDetectResults)
        {
            if (hit.collider == null) //hit nothing
                continue;

            EntityHealth hitHealth = hit.collider.gameObject.GetComponent<EntityHealth>();
            if (hitHealth == null)
                continue;
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
