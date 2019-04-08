using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogUnit : Entity
{
    //Fields
    private Fog fog;
    private float healthLimit;
    private bool spilled = false;

    private bool lerp = false;
    private float start = 0.6f;
    private float end = -0.6f;

    //Properties
    public Fog Fog { get => fog; set => fog = value; }
    public float HealthLimit { get => healthLimit; set => healthLimit = value; }
    public bool Lerp { get => lerp; set => lerp = value; }
    public bool Spilled { get => spilled; set => spilled = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //public void Lerp()
    //{
    //    lerp = true;
    //}

    // Update is called once per frame
    void Update()
    {
        if (lerp)
        {
            gameObject.GetComponent<Renderer>().material.SetFloat("_FogFade", Mathf.Lerp(start, end, Health/HealthLimit));

            if (Health == HealthLimit)
            {
                lerp = false;
            }
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
