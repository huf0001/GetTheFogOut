using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : PowerSource
{
    private int maxPower = 100;
    private int storedPower = 0;
    // private Dictionary<Element, int> harvest = new Dictionary<Element, int>();

    // Start is called before the first frame update
    void Start()
    {
        powerSource = null;
        upkeep = +5;
        // costs 0 to build
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
