using System;
using UnityEngine;
using System.Collections.Generic;
using Abilities;

public enum AbilityEnum
{
    BuildingDefence,
    DamageBlast,
    FreezeFog,
    Overclock,
    Sonar
}

public class AbilityController : MonoBehaviour
{
    // Private variables -----------------------------------------------------------------------------------------------
    private AbilityController instance;
    private List<CollectibleObject> collectedObjects = new List<CollectibleObject>();
    private Ability selectedAbility;
    private bool isAbilitySelected = false;
    private Dictionary<AbilityEnum, bool> abilityTriggered = new Dictionary<AbilityEnum, bool>();
    private Dictionary<AbilityEnum, float> abilityCooldowns = new Dictionary<AbilityEnum, float>();
    private TileData selectedTile;

    [SerializeField] private GameObject rangeIndicatorGO;

    // Public Properties -----------------------------------------------------------------------------------------------
    public List<CollectibleObject> CollectedObjects
    {
        get => collectedObjects;
        set
        {
            collectedObjects = value;
            UpdateButtons();
        }
    }

    public AbilityController Instance
    {
        get { return instance; }
        set { instance = value; }
    }

    private bool IsAbilitySelected
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
        abilityCooldowns[AbilityEnum.DamageBlast] = 0f;
        abilityCooldowns[AbilityEnum.FreezeFog] = 0f;

        abilityTriggered[AbilityEnum.Overclock] = false;
        abilityTriggered[AbilityEnum.Sonar] = false;
        abilityTriggered[AbilityEnum.BuildingDefence] = false;
        abilityTriggered[AbilityEnum.DamageBlast] = false;
        abilityTriggered[AbilityEnum.FreezeFog] = false;
    }

    // Update functions ------------------------------------------------------------------------------------------------
    void Update()
    {
        UpdateButtonCooldowns();
        ProcessInput();
        if (IsAbilitySelected)
        {
            DisplayTarget();
        }
    }

    void UpdateButtons()
    {
        // Update buttons when the related objects are collected, @Liam
        foreach (CollectibleObject collectibleObject in collectedObjects)
        {
            switch (collectibleObject.collectible.CollectibleName)
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
        // Loop through each ability and if it is triggered, run its cooldown
        foreach (KeyValuePair<AbilityEnum, bool> trigger in abilityTriggered)
        {
            if (trigger.Value == true)
            {
                abilityCooldowns[trigger.Key] -= Time.deltaTime;
                
                // TODO: Do visual cooldown stuff here
                
                if (abilityCooldowns[trigger.Key] <= 0f)
                {
                    abilityTriggered[trigger.Key] = false;
                }
            }   
        }
    }

    // Button Management -----------------------------------------------------------------------------------------------
    public void OnButtonClicked(Ability ability)
    {
        selectedAbility = ability;
        IsAbilitySelected = true;

        // Set correct range for the range indicator
        Vector3 scale = new Vector3(ability.targetRadius * 2, 0.01f, ability.targetRadius * 2);
        rangeIndicatorGO.transform.localScale = scale;
    }

    private void ProcessInput()
    {
        if (IsAbilitySelected)
        {
            // Use ability
            if (Input.GetMouseButtonDown(0))
            {
                if (!abilityTriggered[selectedAbility.AbilityType])
                {
                    selectedAbility.TriggerAbility(selectedTile);
                    abilityTriggered[selectedAbility.AbilityType] = true;
                    abilityCooldowns[selectedAbility.AbilityType] = selectedAbility.baseCoolDown;
                    ResourceController.Instance.StoredPower -= selectedAbility.powerCost;
                    // TODO: Play sound effect
                    // TODO: Start visual cooldown stuff
                    IsAbilitySelected = false;
                    selectedAbility = null;
                }
            } 
            // Cancel ability
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                IsAbilitySelected = false;
                selectedAbility = null;
            }
        }
    }

    // Visual Display --------------------------------------------------------------------------------------------------
    void DisplayTarget()
    {
        // Visually display the targeted area
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Tiles")) &&
            WorldController.Instance.TileExistsAt(hit.point))
        {
            TileData tile = WorldController.Instance.GetTileAt(hit.point);
            rangeIndicatorGO.transform.position = new Vector3(tile.X, 0.2f, tile.Z);
            selectedTile = tile;
        }
    } 
}
