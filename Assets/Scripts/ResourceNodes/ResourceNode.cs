using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourceNode : MonoBehaviour
{

    protected bool isHarvestable = true;
    protected Resource resource;
    protected float resMultiplier;

    public Resource Resource { get => resource; }
    public float ResMultiplier { get => resMultiplier; }
}
