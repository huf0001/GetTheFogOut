﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : PowerSource
{
    private int maxPower = 100;
    private int storedPower = 0;

    private int storedOrganic = 0;
    private int storedMineral = 0;
    private int storedFuel = 0;

    public int StoredOrganic { get => storedOrganic; set => storedOrganic = value; }
    public int StoredMineral { get => storedMineral; set => storedMineral = value; }
    public int StoredFuel { get => storedFuel; set => storedFuel = value; }
    public int MaxPower { get => maxPower; set => maxPower = value; }
    public int StoredPower { get => storedPower; set => storedPower = value; }

    // private Dictionary<Element, int> harvest = new Dictionary<Element, int>();

    // Start is called before the first frame update
    void Start()
    {
        buildingType = BuildingType.Hub;
        
        powerSource = null;
        upkeep = +5;
        // costs 0 to build

        InvokeRepeating("Upkeep", 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Upkeep()
    {
        ChangePower(upkeep);
    }

    public override bool SupplyingPower()
    {
        return true;
    }

    public void AddBattery()
    {
        maxPower += 10;
    }

    public void RemoveBattery()
    {
        maxPower -= 10;
    }

    public void ChangePower(int change)
    {
        storedPower += change;

        if (storedPower > maxPower)
        {
            storedPower = maxPower;
        }
    }
}
