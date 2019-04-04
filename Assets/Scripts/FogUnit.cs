using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogUnit : Entity
{
    //Fields
    private Fog fog;
    private float healthLimit;
    private bool inPlay = true;

    //Properties
    public Fog Fog { get => fog; set => fog = value; }
    public float HealthLimit { get => healthLimit; set => healthLimit = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            ReturnToFogPool();
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
            base.Health = value;

            if (base.Health > healthLimit)
            {
                base.Health = healthLimit;
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
