using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : Locatable
{
    [SerializeField] protected float health = 1f;
    [SerializeField] protected float maxHealth = 1f;

    public virtual float Health { get => health; set => health = value; }

    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    protected bool GotNoHealth()
    {
        if (health <= 0)
        {
            return true;
        }

        return false;
    }
}
