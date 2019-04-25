using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Building : PlaneObject
{
    //Serialized fields
    [SerializeField] protected float visibilityRange;
    [SerializeField] protected int upkeep;
    [SerializeField] protected PowerSource powerSource;
    [SerializeField] protected bool powered = false;
    [SerializeField] protected bool placed = false;
    [SerializeField] protected BuildingType buildingType;
    [SerializeField] protected int mineralCost, powerCost, fuelCost, organicCost;
    [SerializeField] protected AudioSource audioSource;
    //[SerializeField] private Shader hologramShader;
    //[SerializeField] private Shader buildingShader;

    //Non-serialized fields
    private Animator animator;

    //Public properties
    public int Upkeep { get => upkeep; }
    public BuildingType BuildingType { get => buildingType; }
    public Animator Animator { get => animator; set => animator = value; }
    public bool Powered { get => powered; }
    public bool Placed { get => placed; }
    public int MineralCost { get => mineralCost; }
    public int PowerCost { get => powerCost; }
    public int FuelCost { get => fuelCost; }
    public int OrganicCost { get => organicCost; }


    protected virtual void Awake()
    {
        //MakeTilesVisible();
        FindToolTip();
        audioSource = GetComponent<AudioSource>();

        //if (placed)
        //{
        //    GetComponent<Renderer>().material.shader = buildingShader;
        //}
        //else
        //{
        //    GetComponent<Renderer>().material.shader = hologramShader;
        //}
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        CheckHealth();
    }

    //Checks if it's dead or not, and applies the appropriate actions if it is.
    private void CheckHealth()
    {
        if (Health <= 0)
        {
            //Debug.Log(buildingType + " has been destroyed! Called from Building.CheckHealth()");
            DismantleBuilding();
        }
    }

    public virtual void Place()
    {
        if (buildingType != BuildingType.Hub)
        {
            if (powerSource == null)
            {
                SetPowerSource();
            }
        }

        FindObjectOfType<Hub>().AddBuilding(this);
        placed = true;
        audioSource.Play();
        //GetComponent<Renderer>().material.shader = buildingShader;
    }

    public void SetPowerSource()
    {
        //Debug.Log("Setting Power Source");

        //if (location == null)
        //{
        //    Debug.Log("Location of " + this.name + " is null");
        //}

        //if (location.PowerSource == null)
        //{
        //    Debug.Log("Power Source is null");
        //}

        powerSource = location.PowerSource;

        if (powerSource != null)
        {
            //Debug.Log("Plugging In and Powering Up " + this.name);
            powerSource.PlugIn(this);
            PowerUp();
        }
        else
        {
            //Debug.Log("Trigger PowerDown for " + this.name + " from Building.SetPowerSource()");
            PowerDown();
        }

        //Debug.Log(this.name + ": power source is " + powerSource.name + ". Location is (" + location.X + "," + location.Z + "). Powered up is" + powered);
    }

    public virtual void PowerUp()
    {
        powered = true;
    }

    public virtual void PowerDown()
    {
        //Debug.Log("Powering down " + this.name);
        powered = false;
    }

    private void MakeTilesVisible()
    {
        Collider[] tilesToActivate = Physics.OverlapSphere(transform.position, visibilityRange);

        foreach (Collider c in tilesToActivate)
        {
            if (c.gameObject.GetComponent<TileData>() != null)
            {
                c.gameObject.GetComponent<TileData>().AddObserver(this as Building);
            }
        }
    }

    private void MakeTilesNotVisible()
    {
        Collider[] tilesToDeactivate = Physics.OverlapSphere(transform.position, visibilityRange);

        foreach (Collider c in tilesToDeactivate)
        {
            if (c.gameObject.GetComponent<TileData>() != null)
            {
                c.gameObject.GetComponent<TileData>().RemoveObserver(this as Building);
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

    public void DismantleBuilding()
    {
        //Debug.Log("Dismantling " + this.name);

        if (buildingType == BuildingType.Hub || buildingType == BuildingType.Relay)
        {
            PowerSource p = this as PowerSource;
            p.DismantlePowerSource();
        }

        if (powerSource != null)
        {
            powerSource.Unplug(this);
        }

        FindObjectOfType<Hub>().RemoveBuilding(this);
        MakeTilesNotVisible();
        Destroy(this.gameObject);
    }

    //protected virtual void OnDestroy()
    //{
    //    //Debug.Log("Building.OnDestroy() called, attempting to destroy " + this.name);
    //}
}
