using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Defence : Building
{
    [SerializeField] protected int directDamage;
    [SerializeField] protected int aoeDamage;
    [SerializeField] protected float rateOfFire;
    [SerializeField] protected bool placedInEditor ;

    public GameObject rangeIndicator;

    public override bool IsOverclockOn
    {
        get { return isOverclockOn; }
        set
        {
            isOverclockOn = value;
            ResetFire();
        }
    }

    protected abstract void ResetFire();

    public abstract bool TargetInRange();
}
