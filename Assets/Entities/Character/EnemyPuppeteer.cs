using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPuppeteer : MonoBehaviour
{
    private HashSet<GameObject> puppets = new HashSet<GameObject>();
    private DebugLogger dLog;
    public GameObject character;


	void Start()
    {
        dLog = new DebugLogger();
		#region BebugLogger Keys
		dLog.loggableSystems = new Dictionary<string, bool>
		{
			{ "CheckForNewPuppets", true },
			{ "targeting", true }
		};
		#endregion

		StartCoroutine(UpdatePuppetSet());
    }

    void Update()
    {
        foreach (GameObject puppet in puppets)
        {
            Enemy enemyBehavior = puppet.GetComponent<Enemy>();
			enemyBehavior.target = character.transform.position;
            dLog.Log("Set an enemy to target: " + character.transform.position, "targeting");
        }
    }

    IEnumerator UpdatePuppetSet()
    {
        while (true)
        {
            List<GameObject> candidates = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
            Vector2 charPos = character.transform.position;
            foreach (GameObject candidate in candidates)
            {
                //this would be the place to let enemies refuse to be added if that's ever a thing
                Enemy enemyBehavior = candidate.GetComponent<Enemy>();
				float distanceToChar = Vector2.Distance(candidate.transform.position, charPos);
                if (distanceToChar > enemyBehavior.NOTICE_RANGE)
                    candidates.Remove(candidate);
            }

            puppets.UnionWith(candidates); //automatically avoids duplicates
            dLog.Log("Finished looking for candidates", "CheckForNewPuppets");

            yield return new WaitForSeconds(0.5f); 
        }
    }
}
