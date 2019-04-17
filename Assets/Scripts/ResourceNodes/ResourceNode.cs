using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : PlaneObject
{

    protected bool isHarvestable = true;
    [SerializeField] protected Resource resourceType;
    protected float resMultiplier;

    public Resource ResourceType { get => resourceType; }
    public float ResMultiplier { get => resMultiplier; }
}
