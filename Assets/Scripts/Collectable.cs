
using System;
using Abilities;
using UnityEngine;
using UnityEngine.EventSystems;

public class Collectable : Locatable, ICollectible
{
    public string collectibleName = "Collectable";
    public ParticleSystem sonarPing;
    public Ability ability;
    public MeshRenderer meshRenderer;
    public string CollectableName { get => collectibleName; }
    public bool isTriggered;
    public float pingTime;

    private void Start()
    {
        collectibleName = ability.name;
    }
    
    private void Update()
    {
        if (isTriggered)
        {
            pingTime -= Time.deltaTime;
            if (pingTime <= 0)
            {
                if (sonarPing.isPlaying)
                {
                    sonarPing.Stop(true);
                }

                isTriggered = false;
            }
        }

        if (location.FogUnit)
        {
            if (location.FogUnit.Health == 100)
            {
                meshRenderer.enabled = false;
            }
            else
            {
                meshRenderer.enabled = true;
            }
        } 
        else if (!meshRenderer.enabled)
        {
            meshRenderer.enabled = true;
        }
    }

    private void OnMouseDown()
    {
        if (!location.FogUnit && location.PowerSource && !EventSystem.current.IsPointerOverGameObject())
        {
            if (ability.AbilityType != AbilityEnum.None)
            {
                AbilityController.Instance.AbilityCollected[ability.AbilityType] = true;
            }
            UIController.instance.AbilityUnlock(ability);
            Destroy(gameObject);
        }
        
        if (location.FogUnit)
        {
            if (location.FogUnit.Health >= 0 && location.PowerSource && !EventSystem.current.IsPointerOverGameObject())
            {
                if (ability.AbilityType != AbilityEnum.None)
                {
                    AbilityController.Instance.AbilityCollected[ability.AbilityType] = true;
                }
                UIController.instance.AbilityUnlock(ability);
                Destroy(gameObject);
            }
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