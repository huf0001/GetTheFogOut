using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogUnit : Entity
{
    //Fields
    private Fog fog;
    private float healthLimit;
    private bool spilled = false;

    //Properties
    public Fog Fog { get => fog; set => fog = value; }
    public float HealthLimit { get => healthLimit; set => healthLimit = value; }
    public bool Spilled { get => spilled; set => spilled = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override float Health
    {
        get
        {
            return base.Health;
        }

        set
        {
            //Set start and end points at start
            //float start = base.Health;
            //float end = value;
            //float progress = 0;

            //Lerp and increment progress in update
            //base.Health = Mathf.Lerp(start, end, progress);
            //progress += Time.deltaTime;

            base.Health = value;

            if (base.Health > healthLimit)
            {
                base.Health = healthLimit;
            }
            else if (base.Health <= 0)
            {
                ReturnToFogPool();
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
