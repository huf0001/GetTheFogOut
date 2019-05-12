using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : PlaneObject
{

    protected bool isHarvestable = true;
    [SerializeField] protected Resource resourceType;
    [SerializeField] protected float resMultiplier = 1.0f;
    [SerializeField] protected bool visable = true;

    public Resource ResourceType { get => resourceType; }
    public float ResMultiplier { get => resMultiplier; }
    public bool Visable { get => visable; set => visable = value; }

    void Awake()
    {
        FindToolTip();
    }
}
