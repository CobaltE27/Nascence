using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPuppeteer : MonoBehaviour
{
	private HashSet<GameObject> puppets = new HashSet<GameObject>();
	private DebugLogger dLog;
	public GameObject character;

	private HaloLineFormation testFormation;

	void Start()
    {
        dLog = new DebugLogger();
		#region DebugLogger Keys
		dLog.loggableSystems = new Dictionary<string, bool>
		{
			{ "CheckForNewPuppets", false },
			{ "targeting", false }
		};
		#endregion

		testFormation = new HaloLineFormation(new Vector2(0, 4), character.transform, 7, new HashSet<System.Type>() { typeof(IFlier) });
		StartCoroutine(updatePuppets());
    }

    void FixedUpdate()
    {
		List<GameObject> deadPuppets = new List<GameObject>();
		foreach (GameObject puppet in puppets)
        {
			if (puppet == null)
			{
				deadPuppets.Add(puppet);
				continue;
			}

			//placeholder for ai component
			Enemy enemyBehavior = puppet.GetComponent<Enemy>();
			//enemyBehavior.moveTarget = character.transform.position;
			enemyBehavior.moveTarget = testFormation.FormationPositionOf(enemyBehavior);
			IMoving enemyMoveBehavior = (IMoving)enemyBehavior;
			if (!enemyMoveBehavior.IsMoving())
				enemyMoveBehavior.MoveToTarget(3);
			//dLog.Log("Set an enemy to target: " + character.transform.position, "targeting");
		}
		foreach (GameObject deadP in deadPuppets)
		{
			puppets.Remove(deadP);
			testFormation.PuppetDied();
		}
    }

	private IEnumerator updatePuppets()
	{
		while (true)
		{
			dLog.Log("checking for puppets", "CheckForNewPuppets");
			List<GameObject> candidates = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
			List<GameObject> newPuppets = new List<GameObject>();
			Vector2 charPos = character.transform.position;
			foreach (GameObject candidate in candidates)
			{
				Enemy enemyBehavior = candidate.GetComponent<Enemy>();
				if (enemyBehavior.DoesNotice(charPos))
				{
					newPuppets.Add(candidate);
					enemyBehavior.EndCurrentBehavior();
					dLog.Log("found new puppet!", "CheckForNewPuppets");

					//temporary for testing
					if (!puppets.Contains(candidate))
						testFormation.AddPuppet(enemyBehavior);
				}
			}

			puppets.UnionWith(newPuppets); //automatically avoids duplicates
					
			yield return new WaitForSeconds(0.5f); 
		}
	}

}
