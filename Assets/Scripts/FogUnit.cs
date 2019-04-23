using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogUnit : Entity
{
    //Serialized Fields
    [SerializeField] private float damage = 1f;

    //Fields
    private Fog fog;
    private float healthLimit;
    private bool spill = false;

    [SerializeField] private float healthProgress = 0;
    [SerializeField] private float startHealth;
    [SerializeField] private float targetHealth;
    [SerializeField] private bool takingDamage = false;

    private bool lerp = false;
    private float start = 0f;
    private float end = 0.45f;


    //Properties
    public Fog Fog { get => fog; set => fog = value; }
    public float HealthLimit { get => healthLimit; set => healthLimit = value; }
    public bool Lerp { get => lerp; set => lerp = value; }
    public bool TakingDamage { get => takingDamage; }
    public bool Spill { get => spill; set => spill = value; }

    // Start is called before the first frame update
    void Start()
    {
        startHealth = base.Health;
        targetHealth = base.Health;
    }

    // Update is called once per frame
    void Update()
    {
        if (lerp)
        {
            if (takingDamage)
            {
                base.Health = Mathf.Lerp(startHealth, targetHealth, healthProgress);
                healthProgress += Time.deltaTime;

                if (base.Health <= targetHealth)
                {
                    base.Health = targetHealth;
                    takingDamage = false;
                }
            }

            gameObject.GetComponent<Renderer>().material.SetFloat("_Alpha", Mathf.Lerp(start, end, base.Health / HealthLimit));

            if (base.Health == HealthLimit)
            {
                lerp = false;
            }
            else if (base.Health <= 0)
            {
                ReturnToFogPool();
            }
        }
    }

    public void DamageBuilding()
    {
        if (Location.Building != null)
        {
            Location.Building.Health -= damage * (base.Health / HealthLimit);
        }
    }

    public void DealDamage(float damage)
    {
        takingDamage = true;
        lerp = true;

        startHealth = base.Health;
        targetHealth -= damage;
        healthProgress = 0;

        if (targetHealth < 0)
        {
            targetHealth = 0;
        }
    }

    public float FUHealth
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

                if (base.Health > healthLimit)
                {
                    base.Health = healthLimit;
                }
                else if (base.Health < 0)
                {
                    base.Health = 0;
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
