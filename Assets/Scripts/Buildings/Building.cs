using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Building : PlaneObject
{
    [SerializeField] protected float visibilityRange;
    [SerializeField] protected int upkeep;
    public int Upkeep { get => upkeep; }

    private Animator animator;

    [SerializeField] protected PowerSource powerSource;
    [SerializeField] protected bool powered = false;
    [SerializeField] protected bool placed = false;

    [SerializeField] protected BuildingType buildingType;
    public BuildingType BuildingType { get => buildingType; }
    public Animator Animator { get => animator; set => animator = value; }
    public bool Powered { get => powered; }
    public bool Placed { get => placed; }

    private AudioSource audioSource;

    protected virtual void Awake()
    {
        //MakeTilesVisible();
        FindToolTip();
        audioSource = GetComponent<AudioSource>();
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
            Debug.Log(buildingType + " has been destroyed!");
            Destroy(this.gameObject);
        }
    }

    public virtual void Place()
    {
        if (buildingType != BuildingType.Hub)
        {
            SetPowerSource();
            placed = true;
            audioSource.Play();
            if (powerSource == null)
            {
                SetPowerSource();
            }
        }

        placed = true;
    }

    public void SetPowerSource()
    {
        powerSource = location.PowerSource;

        if (powerSource != null)
        {
            powerSource.PlugIn(this);
            PowerUp();
        } else
        {
            PowerDown();
        }
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

    public virtual void PowerUp()
    {
        powered = true;
    }

    public virtual void PowerDown()
    {
        powered = false;
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
            placed = false;
        }
        PowerDown();
        //MakeTilesNotVisible();
    }
}
