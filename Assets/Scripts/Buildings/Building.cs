using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class Building : PlaneObject
{
    //Serialized fields
    [SerializeField] protected float visibilityRange;
    [SerializeField] protected int upkeep;
    [SerializeField] protected PowerSource powerSource = null;
    [SerializeField] protected bool powered = false;
    [SerializeField] protected bool placed = false;
    [SerializeField] protected BuildingType buildingType;
    [SerializeField] protected int mineralCost, powerCost, fuelCost, organicCost;
    [SerializeField] protected AudioClip audioSpawn;
    [SerializeField] protected AudioClip audioDamage;
    [SerializeField] protected AudioClip audioDestroy;
    [SerializeField] protected RectTransform healthBarCanvas;
    [SerializeField] protected Image healthBarImage;
    [SerializeField] protected Gradient healthGradient;
    //[SerializeField] private Shader hologramShader;
    //[SerializeField] private Shader buildingShader;

    //Non-serialized fields
    private Animator animator;
    private ResourceController resourceController = null;
    protected AudioSource audioSource;
    private bool damagingNotified = false;
    private bool damagedNotified = false;

    //Public properties
    //public ResourceController ResourceController { get => resourceController; set => resourceController = value; }
    public int Upkeep { get => upkeep; }
    public BuildingType BuildingType { get => buildingType; }
    public Animator Animator { get => animator; set => animator = value; }
    public bool Powered { get => powered; }
    public bool Placed { get => placed; }
    public int MineralCost { get => mineralCost; }
    public int PowerCost { get => powerCost; }
    public int FuelCost { get => fuelCost; }
    public int OrganicCost { get => organicCost; }
    public bool TakingDamage { get; private set; }

    //public Hub Hub
    //{
    //    get => resourceController;
    //    set
    //    {
    //        Debug.Log("Building.Hub changed");
    //        hub = value;
    //    }
    //}

    protected virtual void Awake()
    {
        //MakeTilesVisible();
        FindToolTip();

        audioSource = GetComponent<AudioSource>();

        resourceController = ResourceController.Instance;
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
        MaxHealth = Health;
        InvokeRepeating("CheckForDamage", 0.1f, 0.5f);
    }

    private void CheckForDamage()
    {
        if (Fog.Instance.DamageOn && Location.FogUnit != null)
        {
            Location.FogUnit.DealDamageToBuilding();
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (placed)
        {
            UpdateHealthBar();
        }
        if (GotNoHealth())
        {
            //Debug.Log(buildingType + " is being dismantled. Called from Building.Update() using Entity.GotNoHealth()");
            DismantleBuilding();
        }
        if (TakingDamage)
        {
            StartCoroutine(CheckStillDamaging());
        }
        else if (Health < MaxHealth && !damagedNotified)
        {
            damagedNotified = true;
            StartCoroutine(MouseController.Instance.WarningScript.ShowMessage(MouseController.Instance.WarningScript.Warning + $"A building is damaged"));
        }
    }

    private void UpdateHealthBar()
    {
        if (Health < MaxHealth)
        {
            if (!healthBarCanvas.gameObject.activeSelf)
            {
                healthBarCanvas.gameObject.SetActive(true);
            }

            healthBarImage.fillAmount = Health / MaxHealth;
            //healthBarImage.color = Color.Lerp(new Color32(255, 0, 0, 255), new Color32(0, 153, 0, 255), healthBarImage.fillAmount);
            healthBarImage.color = healthGradient.Evaluate(healthBarImage.fillAmount);
            healthBarCanvas.LookAt(Camera.main.transform);
        }
        else
        {
            if (healthBarCanvas.gameObject.activeSelf)
            {
                healthBarCanvas.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator CheckStillDamaging()
    {
        float buildHealth = Health;

        yield return new WaitForSeconds(1f);

        if (buildHealth == Health)
        {
            TakingDamage = false;
            damagingNotified = false;
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

        resourceController.AddBuilding(this);
        placed = true;

        /*
        if (this.buildingType != BuildingType.Hub)
        {
            audioSource.PlayOneShot(audioSpawn);
        }*/
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

        powerSource = location.GetClosestPowerSource(this.transform);
        //powerSource = location.PowerSource;

        if (powerSource != null)
        {
            //Debug.Log("Plugging In and Powering Up " + this.name);
            powerSource.PlugIn(this);
            PowerUp();

            // Create wires between buidings
            Wires wire = GetComponentInChildren<Wires>();
            if (wire)
            {
                // Destroy any already existing wires
                if (wire.transform.childCount > 0)
                {
                    for (int i = 0; i < wire.transform.childCount; i++)
                    {
                        Destroy(wire.transform.GetChild(i).gameObject);
                    }
                }

                Wires targetWire = powerSource.GetComponentInChildren<Wires>();
                if (targetWire)
                {
                    wire.next = targetWire.gameObject;
                    wire.CreateWire();
                }
            }
        }
        else
        {
            //Debug.Log("Trigger PowerDown for " + this.name + " from Building.SetPowerSource()");
            PowerDown();

            // Destroy wires
            Wires wire = GetComponentInChildren<Wires>();
            if (wire)
            {
                if (wire.transform.childCount > 0)
                {
                    Destroy(wire.transform.GetChild(0).gameObject);
                }
            }

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
        //Collider[] tilesToActivate = Physics.OverlapSphere(transform.position, visibilityRange);

        //foreach (Collider c in tilesToActivate)
        //{
        //    if (c.gameObject.GetComponent<TileData>() != null)
        //    {
        //        c.gameObject.GetComponent<TileData>().AddObserver(this as Building);
        //    }
        //}

        List<TileData> tiles = location.CollectTilesInRange(location.X, location.Z, (int)visibilityRange);

        foreach (TileData t in tiles)
        {
            t.AddObserver(this);
        }
    }

    private void MakeTilesNotVisible()
    {
        //Collider[] tilesToDeactivate = Physics.OverlapSphere(transform.position, visibilityRange);

        //foreach (Collider c in tilesToDeactivate)
        //{
        //    if (c.gameObject.GetComponent<TileData>() != null)
        //    {
        //        c.gameObject.GetComponent<TileData>().RemoveObserver(this as Building);
        //    }
        //}

        List<TileData> tiles = location.CollectTilesInRange(location.X, location.Z, (int)visibilityRange);

        foreach (TileData t in tiles)
        {
            t.RemoveObserver(this);
        }
    }

    public Component GetData()
    {
        // Returns the correct script for the building's type

        Component script = null;

        switch (buildingType)
        {
            case BuildingType.ArcDefence:
                script = transform.GetComponentInChildren<ArcDefence>();
                break;
            case BuildingType.Battery:
                script = transform.GetComponentInChildren<Battery>();
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
            case BuildingType.RepelFan:
                script = transform.GetComponentInChildren<RepelFan>();
                break;
            default:
                Debug.Log("No matching script found");
                break;
        }

        return script;
    }

    public void DismantleBuilding()
    {
        Debug.Log("Dismantling " + this.name);
        if (buildingType == BuildingType.Hub)
        {
            WorldController.Instance.HubDestroyed = true;
        }

        if (buildingType == BuildingType.Hub || buildingType == BuildingType.Relay)
        {
            PowerSource p = this as PowerSource;
            p.DismantlePowerSource();
        }

        if (powerSource != null)
        {
            //Debug.Log("Unplugging from " + powerSource.name);
            //  powerSource.SuppliedBuildings.Remove(this);
            powerSource.Unplug(this);
        }

        //MakeTilesNotVisible();

        if (Location != null)
        {
            //Debug.Log("Removing from tile");
            Location.RemoveObserver(this);
            Location.Building = null;
        }

        PowerDown();

        if (UIController.instance.buildingInfo.building == this)
        {
            UIController.instance.buildingInfo.HideInfo();
        }

        resourceController.RemoveBuilding(this);

        //Debug.Log("Should be removed from ResourceController's list of my building type");

        AudioSource.PlayClipAtPoint(audioDestroy, this.transform.position, 1f);

        if (this.transform.parent != null)
        {
            Destroy(this.transform.parent.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        Destroy(this);
    }

    //protected virtual void OnDestroy()
    //{
    //    //Debug.Log("Building.OnDestroy() called, attempting to destroy " + this.name);
    //}

    public override TileData Location
    {
        get => base.Location;

        set
        {
            //Debug.Log(this.name + "'s location has been set");
            base.Location = value;
        }
    }

    public IEnumerator DealDamageToBuilding(float damageVal)
    {
        if (damagedNotified)
        {
            damagedNotified = false;
        }
        Health -= damageVal;
        TakingDamage = true;

        if (!damagingNotified)
        {
            StartCoroutine(MouseController.Instance.WarningScript.ShowMessage(MouseController.Instance.WarningScript.Danger + $"A {BuildingType} is being damaged!"));
            audioSource.PlayOneShot(audioDamage);
            damagingNotified = true;
        }
        yield return new WaitForSeconds(0.1f);
    }
}
