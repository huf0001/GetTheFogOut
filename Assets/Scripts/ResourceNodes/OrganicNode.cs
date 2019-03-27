using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganicNode : ResourceNode
{

    public OrganicNode(float resMulti = 1f)
    {
        this.resource = Resource.Organic;
        this.resMultiplier = resMulti;
    }

}
