using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelNode : ResourceNode
{

    public FuelNode(float resMulti = 1f)
    {
        this.resource = Resource.Fuel;
        this.resMultiplier = resMulti;
    }

}
