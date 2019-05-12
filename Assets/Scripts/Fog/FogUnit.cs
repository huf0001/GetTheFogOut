using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogUnit : FogEntity
{
    //Non-Serialized Fields
    [SerializeField] private bool spill = false;
    [SerializeField] private bool neighboursFull = false;

    [SerializeField] private float healthProgress = 0;
    [SerializeField] private float startHealth;
    [SerializeField] private float targetHealth;
    [SerializeField] private bool takingDamage = false;
    
    private float start = 0f;
    private float end = 0.90f;

    //Public Properties
    public Fog Fog { get => fog; set => fog = value; }
    public bool TakingDamage { get => takingDamage; }
    public bool Spill { get => spill; set => spill = value; }
    public bool NeighboursFull { get => neighboursFull; set => neighboursFull = value; }

    // Start is called before the first frame update
    void Start()
    {
        startHealth = base.Health;
        targetHealth = base.Health;
    }

    // Update is called once per frame
    public void UpdateDamageToFogUnit()
    {
        //if (takingDamage)
        //{
            base.Health = Mathf.Lerp(startHealth, targetHealth, healthProgress);

            if (base.Health <= targetHealth)
            {
                base.Health = targetHealth;
                takingDamage = false;
            }
            else
            {
                healthProgress += Time.deltaTime * 3;
            }

            if (base.Health <= 0)
            {
                ReturnToFogPool();
            }
        //}
    }

    public void UpdateRender()
    {
        gameObject.GetComponent<Renderer>().material.SetFloat("_Alpha", Mathf.Lerp(start, end, base.Health / HealthLimit));
    }

    public override void UpdateDamageToBuilding()
    {
        if (Location.Building != null)
        {
            //Why is it a coroutine? Wouldn't doing that cause the coroutines to pile up?
            StartCoroutine(Location.Building.DealDamageToBuilding(damage * (base.Health / HealthLimit)));
            //Location.Building.DamageBuilding(damage * (base.Health / HealthLimit));
        }
    }

    public override void DealDamageToFogUnit(float damage, Vector3 location)
    {
        DealDamageToFogUnit(damage);
    }

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
            //else if (t.FogBatch != Location.FogBatch && t.FogBatch.Batched)
            //{
            //    t.FogBatch.NeighboursFull = false;
            //}
        }
    }

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

                if (base.Health >= healthLimit)
                {
                    base.Health = healthLimit;
                }
                else if (base.Health <= 0)
                {
                    ReturnToFogPool();
                }

                targetHealth = base.Health;
            }
        }
    }

    public void ReturnToFogPool()
    {
        if (fog)
        {
            fog.ReturnFogUnitToPool(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
