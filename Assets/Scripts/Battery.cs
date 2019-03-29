using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Building
{
    public Battery()
    {
        buildingType = BuildingType.Battery;
        // call Hub.AddBattery(); adds 10 to maxStorage
        // costs 30 to build
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

    private void OnDestroy()
    {
        //call Hub.RemoveBattery(); removes 10 from maxStorage
    }
}
