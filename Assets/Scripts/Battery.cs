﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Building
{
    public Battery()
    {
        // call Hub.AddBattery(); adds 10 to maxStorage
        // costs 30 to build
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
