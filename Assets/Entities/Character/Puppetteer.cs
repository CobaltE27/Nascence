﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Puppetteer : MonoBehaviour
{
    protected List<Formation> formations = new List<Formation>();
    protected List<GroupAttack> groupAttacks = new List<GroupAttack>();

    protected delegate IEnumerator GroupAttack(Action callBack);

    /// <summary>
    /// Runs DecideNextState
    /// </summary>
    protected virtual void Start()
    {
        //initialize formations, group attack, groupAttacks list, run DecideNextState etc.
        StartCoroutine(DecideNextState());
    }

    /// <summary>
    /// Calls CleanFormations()
    /// </summary>
    protected virtual void FixedUpdate()
    {
        CleanFormations();
    }

    /// <summary>
    /// Assigns as many recruits in potentialRecruits as possible to formations of this puppetteer, modifying the input set to remove the accepted recruits
    /// </summary>
    /// <param name="potentialRecruits"></param>
    public void AssignRecruits(ref HashSet<Enemy> potentialRecruits)
    {
        Debug.Log("Assigning recruits");
        List<Enemy> keeps = new List<Enemy>();
        foreach (Enemy recruit in potentialRecruits)
            foreach (Formation form in formations)
                if (form.HasCorrectAttributes(recruit))
                {
                    keeps.Add(recruit);
                    form.AddPuppet(recruit);
                }
                else
                    Debug.Log("Not a match");

        potentialRecruits.ExceptWith(keeps);
    }

    protected void CleanFormations()
    {
        foreach (Formation form in formations)
            foreach (Enemy puppet in form.Puppets)
                if (puppet == null)
                    form.PuppetDied();
    }

    /// <summary>
    /// Coroutine that decides what to do between states. In many cases, will not need to execute over multiple updates, but can as a coroutine.
    /// </summary>
    protected abstract IEnumerator DecideNextState();
}
