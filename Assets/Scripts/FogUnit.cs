using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogUnit : Entity
{
    private float healthLimit;

    public float HealthLimit { get => healthLimit; set => healthLimit = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Health == 0)
        {
            //Destroy this fog unit
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

            //Debug.Log("Health update for fog unit. Health is " + Health);
        }
    }
}
