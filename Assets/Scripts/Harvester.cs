using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvester : Building
{
    private int harvest = +5;
    // [SerializeField]? private Element element;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        buildingType = BuildingType.Harvester;
        upkeep = -5;
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

    private void Mine()
    {
        // alternate or compainion function to Upkeep()

        //if (Upkeep())
        //{
        //    ResourceNode res = this.location.getResource();
        //    if (res != null)
        //    {
        //        Hub.StoreResource(res.Resource, harvest);
        //    }
        //}
    }
}
