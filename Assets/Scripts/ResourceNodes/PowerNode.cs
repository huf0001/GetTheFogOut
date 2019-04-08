using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerNode : ResourceNode
{
    public PowerNode(float resMulti = 1f)
    {
        this.resource = Resource.Power;
        this.resMultiplier = resMulti;
    }

}
