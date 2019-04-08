using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralNode : ResourceNode
{

    public MineralNode(float resMulti = 1f)
    {
        this.resource = Resource.Mineral;
        this.resMultiplier = resMulti;
    }

}
