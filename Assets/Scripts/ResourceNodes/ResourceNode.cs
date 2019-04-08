using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : Entity
{

    protected bool isHarvestable = true;
    [SerializeField] protected Resource resourceType;
    [SerializeField] protected float resMultiplier = 1f;

    public Resource ResourceType { get => resourceType; }
    public float ResMultiplier { get => resMultiplier; }
}
