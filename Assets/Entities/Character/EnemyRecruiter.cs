using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyRecruiter : MonoBehaviour
{
	private HashSet<EnemyMovement> recruits = new HashSet<EnemyMovement>();
	private DebugLogger dLog;
	public Puppetteer puppetteer;
	public GameObject character;


	void Start()
    {
        dLog = new DebugLogger();
		#region DebugLogger Keys
		dLog.loggableSystems = new Dictionary<string, bool>
		{
			{ "CheckForNewRecruits", false },
			{ "targeting", false }
		};
		#endregion

		StartCoroutine(updateRecruits());
    }

    void FixedUpdate()
    {
		List<EnemyMovement> deadRecruits = new List<EnemyMovement>();
		foreach (EnemyMovement recruit in recruits)
        {
			if (recruit == null)
			{
				deadRecruits.Add(recruit);
				continue;
			}
		}
		foreach (EnemyMovement deadR in deadRecruits)
		{
			recruits.Remove(deadR);
		}

		if (recruits.Count != 0)
			puppetteer.AssignRecruits(ref recruits);
	}

	private IEnumerator updateRecruits()
	{
		while (true)
		{
			dLog.Log("checking for recruits", "CheckForNewRecruits");
			List<GameObject> taggedEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
			List<EnemyMovement> candidates = new List<EnemyMovement>();
			foreach (GameObject tagged in taggedEnemies)
			{
				EnemyMovement e = tagged.GetComponent<EnemyMovement>();
				if (e != null)
					candidates.Add(e);
			}
			List<EnemyMovement> newRecruits = new List<EnemyMovement>();
			Vector2 charPos = character.transform.position;
			foreach (EnemyMovement candidate in candidates)
			{
				EnemyMovement enemyBehavior = candidate.GetComponent<EnemyMovement>();
				if (enemyBehavior.DoesNotice(charPos) && !enemyBehavior.inFormation)
				{
					newRecruits.Add(candidate);
					enemyBehavior.EndCurrentBehavior();
					dLog.Log("found new recruit!", "CheckForNewRecruits");
				}
			}

			recruits.UnionWith(newRecruits);
					
			yield return new WaitForSeconds(0.5f); 
		}
	}
}
