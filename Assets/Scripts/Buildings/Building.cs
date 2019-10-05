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
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] protected float shield;
    [SerializeField] protected float visibilityRange;
    [SerializeField] protected float upkeep;
    [SerializeField] public PowerSource powerSource = null;
    [SerializeField] protected bool powered = false;
    [SerializeField] protected bool placed = false;
    [SerializeField] protected BuildingType buildingType;
    [SerializeField] protected int mineralCost, powerCost;
    [SerializeField] protected RectTransform healthBarCanvas;
    [SerializeField, FormerlySerializedAs("healthBarImage")] protected Image healthBarMask;
    [SerializeField] protected Image healthBarImage;
    [SerializeField, GradientUsage(true)] protected Gradient healthGradient;
    [SerializeField, ColorUsage(true, true)] protected Color redHDR;
    [SerializeField, FormerlySerializedAs("shieldBarCanvas")] protected RectTransform shieldBarTransform;
    [SerializeField] protected Image shieldBarImage;
    [SerializeField] protected GameObject damageIndicatorPrefab;
    [SerializeField] protected GameObject shieldObj;

    //[SerializeField] private Shader hologramShader;
    //[SerializeField] private Shader buildingShader;

    //Non-Serialized Fields--------------------------------------------------------------------------------------------------------------------------

    private Animator animator;

    protected float shieldTime;
    private bool damagingNotified = false;
    private bool damagedNotified = false;
    private float buildHealth;
    private float halfHealth;
    private float regenWait = 5;
    private bool sirenPlayed = false;
    private MeshRenderer rend;
    private GameObject damInd;
    private DamageIndicator damIndScript;
    public bool isShieldOn = false;
    protected bool isOverclockOn = false;
    public float overclockTimer;
    private Material shieldMat;
    public Wires wire;
    protected Camera cam;
    private bool hideHealthBar = false;

    private float toLerp = 1f;
    private float ShieldCheck = 50f;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    public Animator Animator { get => animator; set => animator = value; }
    public BuildingType BuildingType { get => buildingType; }
    public virtual bool IsOverclockOn { get => isOverclockOn; set => isOverclockOn = value; }
    public int MineralCost { get => mineralCost; }
    public bool Placed { get => placed; }
    public int PowerCost { get => powerCost; }
    public bool Powered { get => powered; }
    public float Shield { get => shield; set => shield = value; }
    public float ShieldTime { get => shieldTime; set => shieldTime = value; }
    public bool TakingDamage { get; private set; }
    public float Upkeep { get => upkeep; }

    public override TileData Location
    {
        get => base.Location;

        set
        {
            base.Location = value;
        }
    }

    public int OverclockValue
    {
        get
        {
            return isOverclockOn ? 3 : 1;
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        MaxHealth = Health;
        halfHealth = Health / 2;
        InvokeRepeating(nameof(CheckForDamage), 0.1f, 0.5f);
        buildHealth = health;
        InvokeRepeating(nameof(CheckStillDamaging), 1, 5);

        if (shieldObj != null)
        {
            shieldMat = shieldObj.GetComponent<Renderer>().material;

        }

        rend = GetComponentInChildren<MeshRenderer>();
        cam = Camera.main;
    }

    protected virtual void CheckForDamage()
    {
        if (Fog.Instance.DamageOn && Location.FogUnitActive)
        {
            Location.FogUnit.DealDamageToBuilding();
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
                if (BuildingType == BuildingType.Hub)
                {
                    if (Hub.Instance.ShipIsActive)
                    {
                        MeshRenderer[] meshs = Hub.Instance.CurrentModel.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer m in meshs)
                        {
                            Material[] ma = m.materials;
                            foreach (Material k in ma)
                            {
                                k.SetInt("_Damaged", 0);
                            }
                        }
                    }
                    UIController.instance.ChangeUIColour("normal");
                }
                else
                {
                    rend.material.SetInt("_Damaged", 0);
                };

                if (damIndScript) damIndScript.On = false;
            }
            buildHealth = Health;
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!hideHealthBar)
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
                sirenPlayed = false;
            }
        }
        else
        {
            regenWait = 5;
        }

        if (health <= halfHealth && health > 0 && TakingDamage)
        {
            if (!sirenPlayed)
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-BuildingSiren", GetComponent<Transform>().position);
                sirenPlayed = true;
            }
        }

        // Process shield decay
        if (isShieldOn)
        {
            checkShieldAbility();
        }

        CheckDismantle();

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
            shieldBarImage.fillAmount = shield / 50;
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

        if (health > halfHealth)
        {
            DOTween.Kill(healthBarImage);
            healthBarImage.color = healthGradient.Evaluate(healthBarMask.fillAmount);
        }
        else if (!DOTween.IsTweening(healthBarImage)) healthBarImage.DOColor(redHDR, 0.5f).SetLoops(-1, LoopType.Yoyo);

        healthBarMask.fillAmount = health / maxHealth;
        healthBarCanvas.LookAt(cam.transform);
    }

    public void HideHealthBar()
    {
        healthBarCanvas.gameObject.SetActive(false);
        hideHealthBar = true;
    }

    protected virtual void RepairBuilding()
    {
        health += 2f;
        if (health >= MaxHealth)
        {
            health = MaxHealth;
        }
    }   

    public virtual void Place()
    {
        //        if (buildingType != BuildingType.Hub)
        //        {
        //            if (powerSource == null)
        //            {
        //                SetPowerSource();
        //            }
        //        }

        CreateWire();
        ResourceController.Instance.AddBuilding(this);
        gameObject.layer = LayerMask.NameToLayer("Buildings");
        placed = true;
    }

    public void SetPowerSource()
    {
        powerSource = location.GetClosestPowerSource(this.transform);

        if (powerSource != null)
        {
            powerSource.PlugIn(this);
            PowerUp();

            // Create wires between buildings
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
    }

    public void CreateWire()
    {
        if (wire)
        {
            return;
        }

        // Create wires between buildings
        wire = GetComponentInChildren<Wires>();
        if (wire)
        {
            if (this != WorldController.Instance.Hub)
            {
                if (location.PowerSource != null)
                {
                    if (location.PowerSource.AreYouConnectedToHub() || location.PowerSource == WorldController.Instance.Hub)
                    {
                        Wires targetWire = location.PowerSource.GetComponentInChildren<Wires>();

                        location.PowerSource.PlugIn(this);
                        powerSource = location.PowerSource;

                        if (targetWire)
                        {
                            wire.next = targetWire.gameObject;
                            wire.CreateWire();
                        }

                    }
                }
                else
                {
                    wire = null;
                    PowerDown();
                }
            }
        }
    }

    public void DestroyWires()
    {
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

                powerSource?.Unplug(this);
                powerSource = null;
            }
            wire.sequence.Kill();
            wire.energyEffectParticle.Stop();
            wire.energyEffectParticle.transform.position = transform.position;
            wire.CleanUp();
            wire = null;
        }
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
        PowerDown();
    }

    protected virtual void CheckDismantle()
    {
        if (GotNoHealth())
        {
            DismantleBuilding();
        }
    }

    public virtual void DismantleBuilding()
    {
        Debug.Log("Dismantling " + this.name);
        if (damInd) Destroy(damInd.gameObject);
        DOTween.Kill(healthBarImage);

        // Kill Cable Effect tween
        if (wire)
        {
            wire.sequence.Kill();
            wire.isEffectOn = false;
            DestroyWires();
        }

        if (buildingType == BuildingType.Hub)
        {
            WorldController.Instance.HubDestroyed = true;
        }

        //        if (buildingType == BuildingType.Hub || buildingType == BuildingType.Extender)
        //        {
        //            PowerSource p = this as PowerSource;
        //            p.DismantlePowerSource();
        //        }

        ShutdownBuilding();

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
            if (!damInd)
            {
                damInd = Instantiate(damageIndicatorPrefab, GameObject.Find("Warnings").transform);
                damIndScript = damInd.GetComponent<DamageIndicator>();
                damIndScript.Locatable = this;
                damIndScript.On = true;
            }
            else damIndScript.On = true;

            if (BuildingType != BuildingType.Hub)
            {
                rend.material.SetInt("_Damaged", 1);
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-BuildingDamaged", transform.position);
            }
            else
            {
                if (damInd.transform.localScale.x == 1) damInd.transform.localScale *= 1.5f;
                if (Hub.Instance.ShipIsActive)
                {
                    MeshRenderer[] meshs = Hub.Instance.CurrentModel.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer m in meshs)
                    {
                        Material[] ma = m.materials;
                        foreach (Material k in ma)
                        {
                            k.SetInt("_Damaged", 1);
                        }
                    }
                }
                UIController.instance.ChangeUIColour("damage");
            }

            damagingNotified = true;
        }

        if (BuildingType == BuildingType.Hub)
        {
            WorldController.Instance.Hub.PlaySiren();
        }
    }
}
