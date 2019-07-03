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
    private Dictionary<AbilityEnum, bool> abilityTriggered = new Dictionary<AbilityEnum, bool>();
    private Dictionary<AbilityEnum, float> abilityCooldowns = new Dictionary<AbilityEnum, float>();

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
    }

    // Update functions ------------------------------------------------------------------------------------------------
    void Update()
    {
        UpdateButtonCooldowns();
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
        foreach (KeyValuePair<AbilityEnum,bool> trigger in abilityTriggered)
        {
            if (trigger.Value == true)
            {
                abilityCooldowns[trigger.Key] -= Time.deltaTime;
                
                // Do visual cooldown stuff here
                
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
        if (!abilityTriggered[ability.AbilityType])
        {
            ability.TriggerAbility();
            abilityTriggered[ability.AbilityType] = true;
            abilityCooldowns[ability.AbilityType] = ability.baseCoolDown;
            // Play sound effect
            // Start visual cooldown stuff
        }
    }

    // Visual Display --------------------------------------------------------------------------------------------------
    void DisplayTarget()
    {
        // Visually display the targeted area
    } 
}
