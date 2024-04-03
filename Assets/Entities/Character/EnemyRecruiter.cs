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

	private HaloLineFormation testFormation;

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

		testFormation = new HaloLineFormation(new Vector2(0, 4), character.transform, 7, new HashSet<System.Type>() { typeof(IFlier) });
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

			//placeholder for ai component
			Enemy enemyBehavior = recruit.GetComponent<Enemy>();
			enemyBehavior.moveTarget = testFormation.FormationPositionOf(enemyBehavior);
			IMoving enemyMoveBehavior = (IMoving)enemyBehavior;
			if (!enemyMoveBehavior.IsMoving())
				enemyMoveBehavior.MoveToTarget(3);
		}
		foreach (Enemy deadR in deadRecruits)
		{
			recruits.Remove(deadR);
			testFormation.PuppetDied();
		}

		puppetteer.AssignRecruits(ref recruits);
    }

	private IEnumerator updateRecruits()
	{
		while (true)
		{
			dLog.Log("checking for recruits", "CheckForNewRecruits");
			List<GameObject> taggedEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
			List<Enemy> candidates = taggedEnemies.Cast<Enemy>().ToList();
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

					//temporary for testing
					if (!recruits.Contains(candidate))
						testFormation.AddPuppet(enemyBehavior);
				}
			}

			recruits.UnionWith(newRecruits);
					
			yield return new WaitForSeconds(0.5f); 
		}
	}
}
