using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : PowerSource
{
    // Start is called before the first frame update
    void Start()
    {
        powerSource = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool SupplyingPower()
    {
        return true;
    }
}
