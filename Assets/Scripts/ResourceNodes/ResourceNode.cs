using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourceNode : Entity
{

    protected bool isHarvestable = true;
    [SerializeField] protected Resource resource;
    protected float resMultiplier;

    public Resource Resource { get => resource; }
    public float ResMultiplier { get => resMultiplier; }
}
