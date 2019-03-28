using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defence : Building
{
    public Defence(PowerSource power)
    {
        buildingType = BuildingType.Defence;
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
}
