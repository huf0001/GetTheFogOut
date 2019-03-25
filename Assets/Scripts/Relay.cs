using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relay : PowerSource
{
    public Relay(PowerSource power)
    {
        powerSource = power;
    }

    // Start is called before the first frame update
    void Start()
    {

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
