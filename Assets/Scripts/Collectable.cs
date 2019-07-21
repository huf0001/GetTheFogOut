
using System;
using Abilities;
using UnityEngine;

public class Collectable : Locatable, ICollectible
{
    public string collectibleName = "Collectable";
    public Ability ability;
    public string CollectableName { get => collectibleName; }

    private void Start()
    {
        collectibleName = ability.name;
    }

    private void OnMouseDown()
    {
        if (!location.FogUnit)
        {
            if (ability.AbilityType != AbilityEnum.None)
            {
                AbilityController.Instance.AbilityCollected[ability.AbilityType] = true;
            }
            UIController.instance.AbilityUnlock(ability);
            Destroy(gameObject);
        }
    }

    AbilityEnum ConvertCollectableName(string name)
    {
        switch (name)
        {
            case "Sonar":
                return AbilityEnum.Sonar;
            case "Artillery":
                return AbilityEnum.Artillery;
            case "BuildingDefence":
                return AbilityEnum.BuildingDefence;
            case "Overclock":
                return AbilityEnum.Overclock;
            case "FreezeFog":
                return AbilityEnum.FreezeFog;
            default:
                return AbilityEnum.None;
        }
    }
}