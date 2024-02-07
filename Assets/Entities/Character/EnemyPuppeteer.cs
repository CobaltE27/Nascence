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
			{ "CheckForNewPuppets", true }
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
        }
    }

    IEnumerator UpdatePuppetSet()
    {
        while (true)
        {
            List<GameObject> candidates = new List<GameObject>(FindObjectsOfType<GameObject>());
            Vector2 charPos = character.transform.position;
            foreach (GameObject candidate in candidates)
            {
                //this would be the place to let enemies refuse to be added if that's ever a thing
                if (Vector2.Distance(candidate.transform.position, charPos) > candidate.GetComponent<Enemy>().NOTICE_RANGE)
                    candidates.Remove(candidate);
            }

            puppets.UnionWith(candidates); //automatically avoids duplicates
            dLog.Log("Finished looking for candidates", "CheckForNewPuppets");

            yield return new WaitForSeconds(0.5f); 
        }
    }
}
