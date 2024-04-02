using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRecruiteer : MonoBehaviour
{
	private HashSet<GameObject> recruits = new HashSet<GameObject>();
	private DebugLogger dLog;
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
		List<GameObject> deadRecruits = new List<GameObject>();
		foreach (GameObject recruit in recruits)
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
		foreach (GameObject deadR in deadRecruits)
		{
			recruits.Remove(deadR);
			testFormation.PuppetDied();
		}
    }

	private IEnumerator updateRecruits()
	{
		while (true)
		{
			dLog.Log("checking for recruits", "CheckForNewRecruits");
			List<GameObject> candidates = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
			List<GameObject> newRecruits = new List<GameObject>();
			Vector2 charPos = character.transform.position;
			foreach (GameObject candidate in candidates)
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
