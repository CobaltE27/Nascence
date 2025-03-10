﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Puppetteer : MonoBehaviour
{
	public GameObject player;
	protected List<Formation> formations = new List<Formation>();
    protected List<GroupAttack> groupAttacks = new List<GroupAttack>();

    protected delegate IEnumerator GroupAttack(Action callBack);

    /// <summary>
    /// Runs DecideNextState
    /// </summary>
    protected virtual void Start()
    {
        //initialize formations, group attack, groupAttacks list, run DecideNextState etc.
        DecideNextState();
    }

    /// <summary>
    /// Does nothing right now
    /// </summary>
    protected virtual void FixedUpdate()
    {

    }

    /// <summary>
    /// Assigns as many recruits in potentialRecruits as possible to formations of this puppetteer, modifying the input set to remove the accepted recruits
    /// </summary>
    /// <param name="potentialRecruits"></param>
    public void AssignRecruits(ref HashSet<EnemyMovement> potentialRecruits)
    {
        List<EnemyMovement> keeps = new List<EnemyMovement>();
        foreach (EnemyMovement recruit in potentialRecruits)
            foreach (Formation form in formations)
                if (form.HasCorrectAttributes(recruit))
                {
                    keeps.Add(recruit);
                    form.AddPuppet(recruit);

                    recruit.inFormation = true;
                    recruit.EndCurrentBehavior();
                }

        potentialRecruits.ExceptWith(keeps);
    }

    protected void CleanFormations()
    {
        HashSet<Formation> hadDeaths = new HashSet<Formation>();
        foreach (Formation form in formations)
            foreach (EnemyMovement puppet in form.Puppets)
                if (puppet == null)
                {
                    hadDeaths.Add(form);
                    break;
                }

        foreach (Formation form in hadDeaths)
            form.PuppetDied();
    }

    /// <summary>
    /// Coroutine that decides what to do between states. In many cases, will not need to execute over multiple updates, but can as a coroutine.
    /// </summary>
    protected abstract void DecideNextState();

	protected IEnumerator WaitAndThen(Action callBack, float durationSec = 1.0f)
	{
		yield return new WaitForSeconds(durationSec);
		callBack();
		yield break;
	}

	protected IEnumerator NextFrameAndThen(Action callBack)
	{
		yield return new WaitForFixedUpdate();
		callBack();
		yield break;
	}
}
