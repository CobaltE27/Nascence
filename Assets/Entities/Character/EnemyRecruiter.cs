using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyRecruiter : MonoBehaviour
{
	private HashSet<Enemy> recruits = new HashSet<Enemy>();
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
		List<Enemy> deadRecruits = new List<Enemy>();
		foreach (Enemy recruit in recruits)
        {
			if (recruit == null)
			{
				deadRecruits.Add(recruit);
				continue;
			}
		}
		foreach (Enemy deadR in deadRecruits)
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
			List<Enemy> candidates = new List<Enemy>();
			foreach (GameObject tagged in taggedEnemies)
			{
				Enemy e = tagged.GetComponent<Enemy>();
				if (e != null)
					candidates.Add(e);
			}
			List<Enemy> newRecruits = new List<Enemy>();
			Vector2 charPos = character.transform.position;
			foreach (Enemy candidate in candidates)
			{
				Enemy enemyBehavior = candidate.GetComponent<Enemy>();
				if (enemyBehavior.DoesNotice(charPos))
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
