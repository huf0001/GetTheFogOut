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

    [Header("Dialogue")]
    [SerializeField] private DialogueBox dialogueBox;

    //Non-Serialized Fields
    private bool dismantling = false;
    private GameObject currentModel;

    private float startingHealth;
    private bool hubDamaged = false;
    private bool hubDamagedLastUpdate = false;
    private bool alertedAboutDamage = false;
    private float lastDamage = -1f;
    private float lastDamageDialogue = -1f;
    private float hubDamagedAlertWait = 0f;
    private float hubDamagedAlertInitialWait = 10f;
    private float hubDamagedAlertCooldownWait = 30f;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    public static Hub Instance { get; protected set; }

    public GameObject CurrentModel { get => currentModel; }

    public List<Locatable> DamageMarkers { get => new List<Locatable>() { cockpitDamageMarker, enginesDamageMarker, leftWingDamageMarker, rightWingDamageMarker }; }
    public bool IsDestroyed { get => Health <= 0; }

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

        startingHealth = health;
        lastDamage = Time.fixedTime;
        powerSource = null;
        fireSound = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/3D-ShipFire");
        fireSound.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        fireSound.start();
        currentModel = brokenShip;
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

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
                leftWingDamageMarker.Location.Building = this;

                //Ensure they flash blue and white on the minimap synchronously
                cockpitDamageMarker.Location.MinimapTile.StopCheckColour();
                enginesDamageMarker.Location.MinimapTile.StopCheckColour();
                leftWingDamageMarker.Location.MinimapTile.StopCheckColour();
                rightWingDamageMarker.Location.MinimapTile.StopCheckColour();

                cockpitDamageMarker.Location.MinimapTile.StartCheckColour();
                enginesDamageMarker.Location.MinimapTile.StartCheckColour();
                leftWingDamageMarker.Location.MinimapTile.StartCheckColour();
                rightWingDamageMarker.Location.MinimapTile.StartCheckColour();
                break;
            default:
                Debug.Log($"Error: invalid ship model \"{model}\" submitted to Hub.CurrentModel().");
                break;
        }
    }

    protected override void CheckForDamage()
    {
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
            Location.FogUnit.DealDamageToBuilding();
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (IsDestroyed)
        {
            WorldController.Instance.HubDestroyed = true;
        }
        else
        {
            CheckDamaged();
        }
    }

    private void CheckDamaged()
    {
        hubDamaged = TakingDamage;

        //Hub damaged-ness changed
        if (hubDamaged != hubDamagedLastUpdate)
        {
            hubDamagedLastUpdate = !hubDamagedLastUpdate;

            if (hubDamaged)
            {
                lastDamage = Time.fixedTime;
                hubDamagedAlertWait = hubDamagedAlertInitialWait;
                alertedAboutDamage = false;
            }
        }

        //Damaged, health below 75%, and no dialogue up
        if (hubDamaged && health / startingHealth < 0.75 && !alertedAboutDamage && !dialogueBox.Activated && (Time.fixedTime - lastDamage) > hubDamagedAlertWait)
        {
            alertedAboutDamage = true;
            lastDamageDialogue = Time.fixedTime;
            dialogueBox.SubmitDialogueSet("hub damaged", 0f);
        }
        //Alert timed out or no longer taking damage
        else if (dialogueBox.Activated && dialogueBox.CurrentDialogueSet == "hub damaged" && (!hubDamaged || (Time.fixedTime - lastDamageDialogue) > 10f))
        {
            dialogueBox.SubmitDeactivation();
        }
        //Dialogue deactivating while damaged
        else if (hubDamaged && dialogueBox.Deactivating)
        {
            lastDamage = Time.fixedTime;
            alertedAboutDamage = false;

            //Alert timed out
            if (dialogueBox.LastDialogueSet == "hub damaged")
            {
                hubDamagedAlertWait = hubDamagedAlertCooldownWait;
            }
            //Other dialogue came up
            else
            {
                hubDamagedAlertWait = hubDamagedAlertInitialWait;
            }
        }
    }

    protected override void RepairBuilding()
    {
        health += 6f;

        if (health >= MaxHealth)
        {
            health = MaxHealth;
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
