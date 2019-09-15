using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using DG.Tweening;

public abstract class Building : Entity
{
    //Serialized fields
    [SerializeField] protected float shield;
    [SerializeField] protected float visibilityRange;
    [SerializeField] protected float upkeep;
    [SerializeField] protected PowerSource powerSource = null;
    [SerializeField] protected bool powered = false;
    [SerializeField] protected bool placed = false;
    [SerializeField] protected BuildingType buildingType;
    [SerializeField] protected int mineralCost, powerCost;
    [SerializeField] protected RectTransform healthBarCanvas;
    [SerializeField] protected Image healthBarImage;
    [SerializeField] protected Gradient healthGradient;
    [SerializeField, FormerlySerializedAs("shieldBarCanvas")] protected RectTransform shieldBarTransform;
    [SerializeField] protected Image shieldBarImage;
    [SerializeField] protected GameObject damageIndicatorPrefab;
    [SerializeField] protected GameObject shieldObj;

    //[SerializeField] private Shader hologramShader;
    //[SerializeField] private Shader buildingShader;

    //Non-serialized fields
    private Animator animator;

    protected float shieldTime;
    private bool damagingNotified = false;
    private bool damagedNotified = false;
    private float buildHealth;
    private float halfHealth;
    private float regenWait = 5;
    private float sirenWait = 0;
    private MeshRenderer rend;
    private GameObject damInd;
    private DamageIndicator damIndScript;
    public bool isShieldOn = false;
    protected bool isOverclockOn = false;
    public float overclockTimer;
    private Material shieldMat;
    private Wires wire;
    protected Camera cam;

    private float toLerp = 1f;
    private float ShieldCheck = 50f;
    //Public properties
    //public ResourceController ResourceController { get => resourceController; set => resourceController = value; }
    public float Upkeep { get => upkeep; }
    public BuildingType BuildingType { get => buildingType; }
    public Animator Animator { get => animator; set => animator = value; }
    public bool Powered { get => powered; }
    public bool Placed { get => placed; }
    public int MineralCost { get => mineralCost; }
    public int PowerCost { get => powerCost; }
    public float Shield { get => shield; set => shield = value; }
    public float ShieldTime { get => shieldTime; set => shieldTime = value; }

    public bool TakingDamage { get; private set; }

    public int OverclockValue
    {
        get { return IsOverclockOn ? 3 : 1; }
    }

    public virtual bool IsOverclockOn
    {
        get { return isOverclockOn; }
        set { isOverclockOn = value; }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        MaxHealth = Health;
        halfHealth = Health / 2;
        InvokeRepeating("CheckForDamage", 0.1f, 0.5f);
        buildHealth = health;
        InvokeRepeating("CheckStillDamaging", 1, 5);
        if (shieldObj != null)
        {
            shieldMat = shieldObj.GetComponent<Renderer>().material;

        }
        rend = GetComponentInChildren<MeshRenderer>();
        cam = Camera.main;
    }

    //TODO: says it's never used, double check
    private void CheckForDamage()
    {
        if (Fog.Instance.DamageOn && Location.FogUnitActive)
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

        if (!TakingDamage && health < maxHealth)
        {
            if (regenWait <= 0)
            {
                RepairBuilding();
                regenWait = 5;
            }
            else
            {
                regenWait -= Time.deltaTime;
            }
        }
        else
        {
            regenWait = 5;
        }

        if (health <= halfHealth && health > 0)
        {
            if (sirenWait <= 0)
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-BuildingSiren", GetComponent<Transform>().position);
                sirenWait = 1;
            }
            else
            {
                sirenWait -= Time.deltaTime;
            }
        }
        else
        {
            sirenWait = 0;
        }

        // Process shield decay
        if (isShieldOn)
        {
            checkShieldAbility();
        }

        if (GotNoHealth())
        {
            DismantleBuilding();
        }

    }

    private void checkShieldAbility()
    {
        if (shieldObj)
        {
            shieldTime -= Time.deltaTime;
            if (shield != 0 && shield >= 0)
            {
                if (toLerp == 0 || shield == 50f)
                {
                    toLerp = 1f;
                    ShieldCheck = 50f;
                }
                shieldMat.DOFloat(toLerp, "_LERP", 2f);
                if (shield < (ShieldCheck - 10f))
                {
                    toLerp -= 0.2f;
                    ShieldCheck -= 10f;
                }
            }
            if (shieldTime <= 0 || shield <= 0)
            {
                toLerp = 1f;
                ShieldCheck = 50f;
                shield = 0;
                isShieldOn = false;
                shieldMat.DOFloat(0.1f, "_LERP", 2f);
            }
        }
    }

    private void UpdateHealthBar()
    {
        if (isShieldOn)
        {
            FillHealthBar();

            if (buildingType != BuildingType.Hub)
            {
                shieldBarImage.fillAmount = (shield / 50) * 0.75f;
            }
            else
            {
                shieldBarImage.fillAmount = shield / 50;
            }
        }
        else if (Health < MaxHealth)
        {
            FillHealthBar();
            if (shieldBarImage.fillAmount > 0)
            {
                shieldBarImage.fillAmount = 0;
            }
        }
        else
        {
            if (healthBarCanvas.gameObject.activeSelf)
            {
                healthBarCanvas.gameObject.SetActive(false);
            }
            if (shieldBarImage.fillAmount > 0)
            {
                shieldBarImage.fillAmount = 0;
            }
        }
    }

    private void FillHealthBar()
    {
        if (!healthBarCanvas.gameObject.activeSelf)
        {
            healthBarCanvas.gameObject.SetActive(true);
        }

        healthBarImage.color = healthGradient.Evaluate(healthBarImage.fillAmount);
        if (buildingType != BuildingType.Hub)
        {
            healthBarImage.fillAmount = (Health / MaxHealth) * 0.75f;
            healthBarCanvas.LookAt(new Vector3(cam.transform.position.x, 0, cam.transform.position.z));
            healthBarCanvas.Rotate(0, 135, 0);
        }
        else
        {
            healthBarImage.fillAmount = Health / MaxHealth;
            healthBarCanvas.LookAt(cam.transform);
        }
    }

    private void RepairBuilding()
    {
        health += 2f;
        if (health >= MaxHealth)
        {
            health = MaxHealth;
        }
    }

    private void CheckStillDamaging()
    {
        if (TakingDamage)
        {
            if (buildHealth == Health)
            {
                TakingDamage = false;
                damagingNotified = false;
                if (damIndScript) damIndScript.On = false;
            }
            buildHealth = Health;
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

        ResourceController.Instance.AddBuilding(this);
        gameObject.layer = LayerMask.NameToLayer("Buildings");
        placed = true;

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
            wire = GetComponentInChildren<Wires>();
            if (wire)
            {
                // Destroy any already existing wires
                if (wire.transform.childCount > 0)
                {
                    for (int i = 0; i < wire.transform.childCount; i++)
                    {
                        if (wire.transform.GetChild(i).name != "Cable Energy")
                        {
                            Destroy(wire.transform.GetChild(i).gameObject);
                        }
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
        List<TileData> tiles = location.CollectTilesInRange((int)visibilityRange);

        foreach (TileData t in tiles)
        {
            t.AddObserver(this);
        }
    }

    private void MakeTilesNotVisible()
    {
        List<TileData> tiles = location.CollectTilesInRange((int)visibilityRange);

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
            case BuildingType.AirCannon:
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
            case BuildingType.Extender:
                script = transform.GetComponentInChildren<Relay>();
                break;
            case BuildingType.FogRepeller:
                script = transform.GetComponentInChildren<RepelFan>();
                break;
            default:
                Debug.Log("No matching script found");
                break;
        }

        return script;
    }

    public void ShutdownBuilding()
    {
        if (powerSource != null)
        {
            //Debug.Log("Unplugging from " + powerSource.name);
            //  powerSource.SuppliedBuildings.Remove(this);
            powerSource.Unplug(this);
        }

        PowerDown();
    }

    public void DismantleBuilding()
    {
        Debug.Log("Dismantling " + this.name);
        if (damInd) Destroy(damInd.gameObject);
        
        // Kill Cable Effect tween
        wire.sequence.Kill();
        
        if (buildingType == BuildingType.Hub)
        {
            WorldController.Instance.HubDestroyed = true;
        }

        if (buildingType == BuildingType.Hub || buildingType == BuildingType.Extender)
        {
            PowerSource p = this as PowerSource;
            p.DismantlePowerSource();
        }

        ShutdownBuilding();

        //MakeTilesNotVisible();

        if (Location != null)
        {
            //Debug.Log("Removing from tile");
            Location.RemoveObserver(this);
            Location.Building = null;
        }

        if (UIController.instance.buildingInfo.building == this)
        {
            UIController.instance.buildingInfo.HideInfo();
        }

        if (!GotNoHealth())
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-Sting_3", GetComponent<Transform>().position);
        }
        ResourceController.Instance.RemoveBuilding(this);

        //Debug.Log("Should be removed from ResourceController's list of my building type");

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

    public void DealDamageToBuilding(float damageVal)
    {
        if (damagedNotified)
        {
            damagedNotified = false;
        }

        // If shield is on, take damage from shield first.
        if (isShieldOn)
        {
            shield -= damageVal;
            if (shield <= 0f)
            {
                Health += shield;
                shield = 0f;
            }
        }
        else
        {
            Health -= damageVal;
        }

        TakingDamage = true;

        if (!damagingNotified)
        {
            if (BuildingType != BuildingType.Hub)
            {
                if (!damInd)
                {
                    damInd = Instantiate(damageIndicatorPrefab, GameObject.Find("Warnings").transform);
                    damIndScript = damInd.GetComponent<DamageIndicator>();

                    //RectTransform rect = damInd.GetComponent<RectTransform>();
                }
                else damIndScript.On = true;
                damIndScript.Locatable = this;
            }
            
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-BuildingDamaged", transform.position);
            damagingNotified = true;
        }
    }
}
