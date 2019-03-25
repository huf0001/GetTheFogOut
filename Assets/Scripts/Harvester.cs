using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvester : Building
{
    private int harvest = +5;
    // [SerializeField]? private Element element;

    public Harvester(PowerSource power)
    {
        powerSource = power;
        upkeep = -5;
        // costs 50 to build
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Upkeep()
    {
        // calls Hub.ChangePower(upkeep)
        // calls Hub.StoreElement(element, harvest) . . . or would it be powerSource.ReturnElement(element, harvest)?
    }
}
