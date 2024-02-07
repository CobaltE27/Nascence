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
			{ "CheckForNewPuppets", false },
			{ "targeting", true }
		};
		#endregion
    }

    void Update()
    {
		List<GameObject> candidates = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
		Vector2 charPos = character.transform.position;
		List<GameObject> newPuppets = new List<GameObject>();
		foreach (GameObject candidate in candidates)
		{
			//this would be the place to let enemies refuse to be added if that's ever a thing
			Enemy enemyBehavior = candidate.GetComponent<Enemy>();
			float distanceToChar = Vector2.Distance(candidate.transform.position, charPos);
			if (distanceToChar < enemyBehavior.NOTICE_RANGE)
				newPuppets.Add(candidate);
		}

		puppets.UnionWith(newPuppets); //automatically avoids duplicates
		dLog.Log("Finished looking for candidates", "CheckForNewPuppets");

		foreach (GameObject puppet in puppets)
        {
            Enemy enemyBehavior = puppet.GetComponent<Enemy>();
			enemyBehavior.target = character.transform.position;
            dLog.Log("Set an enemy to target: " + character.transform.position, "targeting");
        }
    }

}
