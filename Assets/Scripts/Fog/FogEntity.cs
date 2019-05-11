using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FogEntity : Entity
{
    //Serialized Fields
    [SerializeField] protected float damage = 1f;

    //Private Fields
    [SerializeField] protected float healthLimit;
    protected Fog fog;

    //Public Properties
    public float HealthLimit { get => healthLimit; set => healthLimit = value; }

    //Abstract Methods
    public abstract void DealDamage(float damage, Vector3 location);

    public abstract void DamageBuilding();
}
