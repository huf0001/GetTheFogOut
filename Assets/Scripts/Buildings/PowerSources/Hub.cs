using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hub : PowerSource
{ 
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields

    [Header("Models")]
    [SerializeField] private GameObject brokenShip;
    [SerializeField] private GameObject repairedShip;
    [SerializeField] private GameObject attachedWing;

    [Header("Damage Markers")]
    [SerializeField] private Locatable cockpitDamageMarker;
    [SerializeField] private Locatable enginesDamageMarker;
    [SerializeField] private Locatable leftWingDamageMarker;
    [SerializeField] private Locatable rightWingDamageMarker;

    [Header("Fire")]
    [SerializeField] private GameObject Fires;
    [SerializeField] private Light fireLight;
    private float tick = 0;
    private bool stillFire = true;
    private bool isPlayingSiren;
    private FMOD.Studio.EventInstance fireSound;

    //Non-Serialized Fields
    private bool dismantling = false;
    private GameObject currentModel;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    public static Hub Instance { get; protected set; }

    public GameObject CurrentModel { get => currentModel; }

    public List<Locatable> DamageMarkers { get => new List<Locatable>() { cockpitDamageMarker, enginesDamageMarker, leftWingDamageMarker, rightWingDamageMarker }; }
    public bool ShipIsActive { get => brokenShip.activeInHierarchy || repairedShip.activeInHierarchy || attachedWing.activeInHierarchy; }

    public override bool SupplyingPower()
    {
        return true;
    }

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    protected void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There should only be one hub in the scene, right?");
        }

        Instance = this;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        powerSource = null;
        fireSound = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/3D-ShipFire");
        fireSound.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        fireSound.start();
        currentModel = brokenShip;
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (isDestroyed())
        {
            WorldController.Instance.HubDestroyed = true;
        }
    }

    public void SetCurrentModel(string model)
    {
        currentModel.SetActive(false);

        switch (model)
        {
            case "broken":
                currentModel = brokenShip;
                currentModel.SetActive(true);
                break;
            case "repaired":
                currentModel = repairedShip;
                currentModel.SetActive(true);
                break;
            case "attached":
                currentModel = attachedWing;
                currentModel.SetActive(true);
                break;
            default:
                Debug.Log($"Error: invalid ship model \"{model}\" submitted to Hub.CurrentModel().");
                break;
        }
    }

    protected override void CheckForDamage()
    {
        //Debug.Log("Hub.CheckForDamage");
        if (Fog.Instance.DamageOn)
        {
            CheckForDamage(cockpitDamageMarker);
            CheckForDamage(enginesDamageMarker);
            CheckForDamage(rightWingDamageMarker);

            if (currentModel == attachedWing)
            {
                CheckForDamage(leftWingDamageMarker);
            }
        }
    }

    private void CheckForDamage(Locatable damageMarker)
    {
        if (damageMarker.Location.FogUnitActive)
        {
            //Debug.Log($"Hub taking damage to {damageMarker.name}");
            Location.FogUnit.DealDamageToBuilding();
        }
    }

    protected override void RepairBuilding()
    {
        //Debug.Log("Hub.RepairBuilding");
        health += 6f;

        if (health >= MaxHealth)
        {
            health = MaxHealth;
        }
    }

    public bool isDestroyed()
    {
        if (Health <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PlaySiren()
    {
        if (!isPlayingSiren)
        {
            isPlayingSiren = true;
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-BuildingSirenHub", transform.position);
            Invoke(nameof(StopSirenNoise), 1f);
        }
    }

    private void StopSirenNoise()
    {
        isPlayingSiren = false;
    }

    public void extinguishingFire()
    {
        if (stillFire)
        {
            ParticleSystem[] ps = Fires.GetComponentsInChildren<ParticleSystem>();
            // delay 6s - 100 particle so 60s (reparing time) = 1000 particles but because i roundtoint so it faster so 7s
            if (delay(6))
            {
                foreach (ParticleSystem p in ps)
                {
                    ParticleSystem.MainModule pe = p.main;
                    if (pe.maxParticles > 0)
                    {
                        
                        pe.maxParticles -= 95  ;
                    }
                    if (pe.maxParticles <= 1)  // particle left
                    {
                            fireLight.enabled = false;
                            stillFire = false;
                            Fires.SetActive(false);
                            fireSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    }
                    tick = 0;
                }
            }
        }
    }

    private bool delay(int wait)
    {
        tick += Time.deltaTime;
        
        if (Mathf.RoundToInt(tick) == wait)
        {
            return true;
        }
        else
            return false;
    }

    protected override void CheckDismantle()
    {
        if (GotNoHealth() && !dismantling)
        {
            dismantling = true;
            Invoke(nameof(DismantleBuilding), 0.5f);
        }
    }
}
