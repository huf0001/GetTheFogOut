using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : Entity
{
    [SerializeField] private int range;
    protected int upkeep;
    protected PowerSource powerSource;
    protected BuildingType buildingType;
    public BuildingType BuildingType { get => buildingType; }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int Range
    {
        get
        {
            return range;
        }
    }

}
