using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPuppeteer : MonoBehaviour
{
    private HashSet<GameObject> puppets;
    private DebugLogger dLog;


	void Start()
    {
        dLog = new DebugLogger();
		#region BebugLogger Keys
		dLog.loggableSystems = new Dictionary<string, bool>
		{
			{ "CheckForNewPuppets", true }
		};
		#endregion

		StartCoroutine(CheckForNewPuppets());
    }

    void Update()
    {
        
    }

    IEnumerator CheckForNewPuppets()
    {
        while (true)
        {
            List<GameObject> candidates = new List<GameObject>(FindObjectsOfType<GameObject>());
            foreach (GameObject candidate in candidates)
            {
                //this would be the place to let enemies refuse to be added if that's ever a thing
            }

            puppets.UnionWith(candidates); //automatically avoids duplicates

            dLog.Log("Finished looking for candidates", "CheckForNewPuppets");
            yield return new WaitForSeconds(0.5f); 
        }
    }
}
