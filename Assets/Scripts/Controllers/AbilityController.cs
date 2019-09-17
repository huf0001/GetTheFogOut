using System;
using UnityEngine;
using System.Collections.Generic;
using Abilities;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum AbilityEnum
{
    None,
    BuildingDefence,
    Artillery,
    FreezeFog,
    Overclock,
    Sonar
}

public class AbilityController : MonoBehaviour
{
    // Private variables -----------------------------------------------------------------------------------------------
    private static AbilityController instance;
    private List<Collectable> collectedObjects = new List<Collectable>();
    private Ability selectedAbility;
    private bool isAbilitySelected = false;
    private Dictionary<AbilityEnum, bool> abilityCollected = new Dictionary<AbilityEnum, bool>();
    private Dictionary<AbilityEnum, bool> abilityTriggered = new Dictionary<AbilityEnum, bool>();
    private Dictionary<AbilityEnum, float> abilityCooldowns = new Dictionary<AbilityEnum, float>();
    private TileData selectedTile;
    private int cooldownsRunning = 0;
    private Camera cam;

    [SerializeField] private GameObject rangeIndicatorGO;
    [SerializeField] public GameObject AbilityDescGO;

    // Public Properties -----------------------------------------------------------------------------------------------
    public Button[] abilityButtons;

    public List<Collectable> CollectedObjects
    {
        get => collectedObjects;
        set
        {
            collectedObjects = value;
            UpdateButtons();
        }
    }

    public static AbilityController Instance
    {
        get { return instance; }
        set { instance = value; }
    }

    public bool IsAbilitySelected
    {
        get
        {
            return isAbilitySelected;
        }
        set
        {
            rangeIndicatorGO.SetActive(value);
            MouseController.Instance.isBuildAvaliable = !value;
            isAbilitySelected = value;
        }
    }

    public Dictionary<AbilityEnum, bool> AbilityCollected
    {
        get { return abilityCollected; }
        set
        {
            abilityCollected = value;
        }
    }

    public Dictionary<AbilityEnum, bool> AbilityTriggered
    {
        get { return abilityTriggered; }
    }

    // Start up --------------------------------------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more ability controllers.");
        }
        Instance = this;
    }

    private void Start()
    {
        abilityCooldowns[AbilityEnum.Overclock] = 0f;
        abilityCooldowns[AbilityEnum.Sonar] = 0f;
        abilityCooldowns[AbilityEnum.BuildingDefence] = 0f;
        abilityCooldowns[AbilityEnum.Artillery] = 0f;
        abilityCooldowns[AbilityEnum.FreezeFog] = 0f;

        abilityTriggered[AbilityEnum.Overclock] = false;
        abilityTriggered[AbilityEnum.Sonar] = false;
        abilityTriggered[AbilityEnum.BuildingDefence] = false;
        abilityTriggered[AbilityEnum.Artillery] = false;
        abilityTriggered[AbilityEnum.FreezeFog] = false;

        AbilityCollected[AbilityEnum.Overclock] = false;
        AbilityCollected[AbilityEnum.Sonar] = false;
        AbilityCollected[AbilityEnum.BuildingDefence] = false;
        AbilityCollected[AbilityEnum.Artillery] = false;
        AbilityCollected[AbilityEnum.FreezeFog] = false;

        cam = Camera.main;
    }

    // Update functions ------------------------------------------------------------------------------------------------
    void Update()
    {
        if (cooldownsRunning > 0)
        {
            UpdateButtonCooldowns();
        }

        ProcessInput();

        if (IsAbilitySelected)
        {
            DisplayTarget();
        }
    }

    void UpdateButtons()
    {
        // Update buttons when the related objects are collected, @Liam
        foreach (Collectable collectibleObject in collectedObjects)
        {
            switch (collectibleObject.CollectableName)
            {
                case "BuildingDefence":
                    // If button not enabled, enable
                    // Repeat for each below
                    break;
                case "DamageBlast":
                    break;
                case "FreezeFog":
                    break;
                case "Overclock":
                    break;
                case "Sonar":
                    break;
                default:
                    break;
            }
        }
    }

    void UpdateButtonCooldowns()
    {
        List<AbilityEnum> cooldownsToUpdate = new List<AbilityEnum>();

        // Loop through each ability and if it is triggered, run its cooldown
        foreach (KeyValuePair<AbilityEnum, bool> trigger in abilityTriggered)
        {
            if (trigger.Value == true)
            {
                abilityCooldowns[trigger.Key] -= Time.deltaTime;

                // TODO: Do visual cooldown stuff here

                if (abilityCooldowns[trigger.Key] <= 0f)
                {
                    cooldownsToUpdate.Add(trigger.Key);
                }
            }
        }

        foreach (AbilityEnum abilityEnum in cooldownsToUpdate)
        {
            abilityTriggered[abilityEnum] = false;
            cooldownsRunning--;
        }

        cooldownsToUpdate.Clear();
    }

    // Button Management -----------------------------------------------------------------------------------------------
    public void OnButtonClicked(Ability ability)
    {
        if (selectedAbility != ability)
        {
            selectedAbility = ability;
            IsAbilitySelected = true;
            MouseController.Instance.isBuildAvaliable = false;

            // Set correct range for the range indicator
            Vector3 scale = new Vector3(ability.targetRadius * 2, 0.01f, ability.targetRadius * 2);
            rangeIndicatorGO.transform.localScale = scale;
        }
        else
        {
            CancelAbility();
        }
    }

    private void ProcessInput()
    {
        if (WorldController.Instance.Inputs.InputMap.Ability.triggered)
        {
            AbilityMenu.Instance.ToggleMenu();
        }
        if (IsAbilitySelected)
        {
            // Use ability
            if (WorldController.Instance.Inputs.InputMap.Build.ReadValue<float>() > 0 && !WorldController.Instance.IsPointerOverGameObject())
            {
                if (!abilityTriggered[selectedAbility.AbilityType])
                {
                    // Power check is done on button click to assist in providing feedback to player
                    selectedAbility.TriggerAbility(selectedTile);
                    abilityTriggered[selectedAbility.AbilityType] = true;
                    abilityCooldowns[selectedAbility.AbilityType] = selectedAbility.baseCoolDown;
                    cooldownsRunning++;
                    ResourceController.Instance.StoredPower -= selectedAbility.powerCost;
                    // TODO: Play sound effect
                    // TODO: Start visual cooldown stuff
                    IsAbilitySelected = false;
                    selectedAbility = null;
                    MouseController.Instance.isBuildAvaliable = true;
                }
            }
            // Cancel ability
            else if (WorldController.Instance.Inputs.InputMap.Pause.triggered)
            {
                CancelAbility();
            }
        }
    }

    public void CancelAbility()
    {
        IsAbilitySelected = false;
        selectedAbility = null;
        MouseController.Instance.isBuildAvaliable = true;
    }

    // Visual Display --------------------------------------------------------------------------------------------------
    void DisplayTarget()
    {
        // Visually display the targeted area
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Tiles")) &&
            WorldController.Instance.TileExistsAt(hit.point))
        {
            TileData tile = WorldController.Instance.GetTileAt(hit.point);
            rangeIndicatorGO.transform.position = new Vector3(tile.X, 0.2f, tile.Z);
            selectedTile = tile;
        }
    }
}
