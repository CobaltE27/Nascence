using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Puppetteer : MonoBehaviour
{
    protected List<Formation> formations = new List<Formation>();
    //protected Queue???<> groupAttacks;

    void Start()
    {
        
    }

    // Update is called once per frame
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
        List<Enemy> keeps = new List<Enemy>();
        foreach (Enemy recruit in potentialRecruits)
            foreach (Formation form in formations)
                if (form.HasCorrectAttributes(recruit))
                {
                    keeps.Add(recruit);
                    form.AddPuppet(recruit);
                }

        potentialRecruits.ExceptWith(keeps);
    }

    protected void CleanFormations()
    {
        foreach (Formation form in formations)
            foreach (Enemy puppet in form.Puppets)
                if (puppet == null)
                    form.PuppetDied();
    }
}
