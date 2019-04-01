using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : Entity
{
    [SerializeField] protected float visibilityRange;
    [SerializeField] protected int upkeep;
    public int Upkeep { get => upkeep; }

    private Animator animator;

    protected PowerSource powerSource;
    [SerializeField] protected BuildingType buildingType;
    public BuildingType BuildingType { get => buildingType; }
    public Animator Animator { get => animator; set => animator = value; }

    protected virtual void Awake()
    {
        MakeTilesVisible();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if(buildingType != BuildingType.Hub)
        {
            powerSource = location.PowerSource;
            powerSource.PlugIn
                (this);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void MakeTilesVisible()
    {
        Collider[] tilesToActivate = Physics.OverlapSphere(transform.position, visibilityRange);

        foreach (Collider c in tilesToActivate)
        {
            if (c.gameObject.GetComponent<Tile>() != null)
            {
                c.gameObject.GetComponent<Tile>().AddObserver(this as Building);
            }
        }
    }

    private void MakeTilesNotVisible()
    {
        Collider[] tilesToDeactivate = Physics.OverlapSphere(transform.position, visibilityRange);

        foreach (Collider c in tilesToDeactivate)
        {
            if (c.gameObject.GetComponent<Tile>() != null)
            {
                c.gameObject.GetComponent<Tile>().RemoveObserver(this as Building);
            }
        }
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

    protected virtual void OnDestroy()
    {
        if (powerSource != null)
        {
            powerSource.Unplug(this);
        }

        MakeTilesNotVisible();
    }

}
