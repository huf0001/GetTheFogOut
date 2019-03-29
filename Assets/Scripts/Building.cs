using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : Entity
{
    protected int upkeep;
    protected PowerSource powerSource;
    protected BuildingType buildingType;
    public BuildingType BuildingType { get => buildingType; }

    void Awake()
    {
        Collider[] tiles = Physics.OverlapSphere(this.transform.position, 0.25f);

        if (tiles[0].gameObject.GetComponent<Tile>().PowerSource != null)
        {
            powerSource = tiles[0].gameObject.GetComponent<Tile>().PowerSource;
            powerSource.PlugIn(this);
        }
        else
        {
            Debug.Log("There is no power source supplying this space.");
            //Destroy(this.gameObject);
            //Self destruct code when no power source available has been commented out for now to make testing easier
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Component GetData()
    {
        // Returns the correct script for the building's type

        Component script = null;

        switch (buildingType)
        {
            case BuildingType.Battery:
                script = transform.GetComponentInChildren<Battery>();
                break;
            case BuildingType.Defence:
                script = transform.GetComponentInChildren<Defence>();
                break;
            case BuildingType.Generator:
                script = transform.GetComponentInChildren<Generator>();
                break;
            case BuildingType.Harvester:
                script = transform.GetComponentInChildren<Harvester>();
                break;
            case BuildingType.Hub:
                script = transform.GetComponentInChildren<Hub>();
                break;
            case BuildingType.Relay:
                script = transform.GetComponentInChildren<Relay>();
                break;
            default:
                Debug.Log("No matching script found");
                break;
        }

        return script;
    }

    private void OnDestroy()
    {
        if (powerSource != null)
        {
            powerSource.Unplug(this);
        }
    }

}
