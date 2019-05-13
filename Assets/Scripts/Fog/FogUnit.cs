using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogUnit : Entity
{
    //Serialized Fields
    [SerializeField] private float damage = 0.1f;
    [SerializeField] private float lerpToMaxInterval;
    [SerializeField] private float rapidLerpMultiplier = 3f;

    //Non-Serialized Fields
    private Fog fog;

    private float start = 0f;
    private float end = 0.90f;

    /*[SerializeField]*/ private float healthProgress = 0;
    /*[SerializeField]*/ private float startHealth;
    /*[SerializeField]*/ private float targetHealth;
    /*[SerializeField]*/ private bool takingDamage = false;

    /*[SerializeField]*/ private bool spill = false;
    /*[SerializeField]*/ private bool neighboursFull = false;

    //Public Properties
    public Fog Fog { get => fog; set => fog = value; }
    public bool NeighboursFull { get => neighboursFull; set => neighboursFull = value; }
    public bool Spill { get => spill; set => spill = value; }
    public bool TakingDamage { get => takingDamage; }

    //Altered Public Properties
    public override float Health
    {
        get
        {
            return base.Health;
        }

        set
        {
            if (!takingDamage)
            {
                base.Health = value;

                if (base.Health >= maxHealth)
                {
                    base.Health = maxHealth;
                }
                else if (base.Health <= 0)
                {
                    ReturnToFogPool();
                }

                targetHealth = base.Health;
            }
        }
    }

    //Sets the starting values for fog damage health variables
    void Start()
    {
        startHealth = base.Health;
        targetHealth = base.Health;
    }

    //Fog unit deals damage to the building on its tile
    public void DealDamageToBuilding()
    {
        if (Location.Building != null)
        {
            StartCoroutine(Location.Building.DealDamageToBuilding(damage * (base.Health / MaxHealth)));
        }
    }

    //A defence has dealtt damage to the fog unit
    public void DealDamageToFogUnit(float damage)
    {
        takingDamage = true;

        startHealth = base.Health;
        targetHealth -= damage;
        healthProgress = 0;

        if (targetHealth < 0)
        {
            targetHealth = 0;
        }

        foreach (TileData t in Location.AdjacentTiles)
        {
            if (t.FogUnit != null)
            {
                t.FogUnit.NeighboursFull = false;
            }
        }
    }
 
    //Updates the damage dealt to the fog unit
    public void UpdateDamageToFogUnit(float damageInterval)
    {
        base.Health = Mathf.Lerp(startHealth, targetHealth, healthProgress);

        if (base.Health <= targetHealth)
        {
            base.Health = targetHealth;
            takingDamage = false;
        }
        else
        {
            healthProgress += damageInterval * rapidLerpMultiplier;
        }

        if (base.Health <= 0)
        {
            ReturnToFogPool();
        }
    }

    //Updates the fog unit's shader according to its health
    public void Render()
    {
        gameObject.GetComponent<Renderer>().material.SetFloat("_Alpha", Mathf.Lerp(start, end, base.Health / MaxHealth));
    }

    //Tells Fog to put the fog unit back in the pool
    public void ReturnToFogPool()
    {
        if (fog)
        {
            fog.QueueFogUnitForPooling(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
