using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : Locatable
{
    [Header("Entity Health")]
    [SerializeField] protected float health = 1f;
    [SerializeField] protected float maxHealth = 1f;

    public virtual float Health { get => health; set => health = value; }

    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    protected bool GotNoHealth()
    {
        if (health <= 0)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-BuildingDestroyed", GetComponent<Transform>().position);
            return true;
        }

        return false;
    }
}
