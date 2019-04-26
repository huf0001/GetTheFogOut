using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : PowerSource
{ 
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        powerSource = null;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override bool SupplyingPower()
    {
        return true;
    }

    public bool isDestroyed()
    {
        if (Health <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
