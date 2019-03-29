using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvester : Building
{
    private int harvestAmt = +5;
    private bool canHarvest = false;

    public int HarvestAmt { get => harvestAmt; }
    public bool CanHarvest { get => canHarvest; }

    // [SerializeField]? private Element element;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        upkeep = -5;
    }

    // Update is called once per frame
    void Update()
    {
        Harvest();
    }

    private void Upkeep()
    {
        // calls Hub.ChangePower(upkeep)
        // calls Hub.StoreElement(element, harvest) . . . or would it be powerSource.ReturnElement(element, harvest)?
    }

    private void Harvest()
    {
        if (location.Resource == Resource.Mineral)
        {
            if (powerSource != null)
            {
                Hub hub = powerSource as Hub;
                if (!hub.Harvesters.Contains(this))
                {
                    hub.Harvesters.Add(this);
                }
            }
        }
    }
}
