using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relay : PowerSource
{
    public Relay(PowerSource power)
    {
        buildingType = BuildingType.Relay;
        powerSource = power;
        upkeep = 0;
        // costs 10 to build
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PassResourceBackToBase()
    {

    }

    public override bool SupplyingPower()
    {
        return true;
    }
}
