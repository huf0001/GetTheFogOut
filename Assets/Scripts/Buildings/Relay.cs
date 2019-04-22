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
    protected override void Awake()
    {
        base.Awake();
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

    public override void Place()
    {
        base.Place();
    }
    
    public override void PowerUp()
    {
        base.PowerUp();
    }

    public override bool SupplyingPower()
    {
        return true;
    }
}
