using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relay : PowerSource
{
    public Relay(PowerSource power)
    {
        powerSource = power;
        // costs 10 to build
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public void PassResourceBackToBase()
    {

    }

    public override bool SupplyingPower()
    {
        return true;
    }
}
