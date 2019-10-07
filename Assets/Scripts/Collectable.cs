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
    [SerializeField] private GameObject collectableObj;
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
        if (location.FogUnitActive)
        {
            // Debug.Log(location.FogUnit.name + ability);
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

        ShowWhiteMaterials();
        //CollectedCheck();
    }

    public void ShowWhiteMaterials()
    {
        if (TutorialController.Instance.Stage == TutorialStage.Finished || (TutorialController.Instance.Stage == TutorialStage.CollectSonar && ability.AbilityType == AbilityEnum.Sonar))
        {
            Renderer mr = collectableObj.GetComponent<Renderer>();
            Material[] Mats = mr.materials;

            if (!location.FogUnitActive)
            {
                if (location.PowerSource && !WorldController.Instance.IsPointerOverGameObject())
                {
                    if (ability.AbilityType != AbilityEnum.None)
                    {
                        foreach (Material m in Mats)
                        {
                            if (m.HasProperty("_Collectable"))
                            {
                                m.SetFloat("_Collectable", 1);
                            }
                        }
                    }
                }
                else
                {
                    foreach (Material m in Mats)
                    {
                        if (m.HasProperty("_Collectable"))
                        {
                            m.SetFloat("_Collectable", 0);
                        }
                    }
                }
            }

            if (location.FogUnitActive)
            {
                if (location.FogUnit.Health <= 50 && location.PowerSource && !WorldController.Instance.IsPointerOverGameObject())
                {
                    if (ability.AbilityType != AbilityEnum.None)
                    {
                        foreach (Material m in Mats)
                        {
                            if (m.HasProperty("_Collectable"))
                            {
                                m.SetFloat("_Collectable", 1);
                            }
                        }
                    }
                }
                else
                {
                    foreach (Material m in Mats)
                    {
                        if (m.HasProperty("_Collectable"))
                        {
                            m.SetFloat("_Collectable", 0);
                        }
                    }
                }
            }
        }
    }
    public void CollectAbility()
    {
        if (TutorialController.Instance.Stage == TutorialStage.Finished || (TutorialController.Instance.Stage == TutorialStage.CollectSonar && ability.AbilityType == AbilityEnum.Sonar))
        {
            if (!location.FogUnitActive && location.PowerSource && !WorldController.Instance.IsPointerOverGameObject())
            {
                if (ability.AbilityType != AbilityEnum.None)
                {
                    FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-Sting_1", GetComponent<Transform>().position);
                    AbilityController.Instance.AbilityCollected[ability.AbilityType] = true;
                    UIController.instance.AbilityUnlock(ability);
                    location.plane.GetComponent<MeshRenderer>().material = WorldController.Instance.normalTile;
                    Invoke(nameof(DestroyCollectable), 0.05f);
                }
            }

            if (location.FogUnitActive)
            {
                if (location.FogUnit.Health <= 50 && location.PowerSource && !WorldController.Instance.IsPointerOverGameObject())
                {
                    if (ability.AbilityType != AbilityEnum.None)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-Sting_1", GetComponent<Transform>().position);
                        AbilityController.Instance.AbilityCollected[ability.AbilityType] = true;
                        UIController.instance.AbilityUnlock(ability);
                        location.plane.GetComponent<MeshRenderer>().material = WorldController.Instance.normalTile;
                        Invoke(nameof(DestroyCollectable), 0.05f);
                    }
                }
            }
        }
    }

    private void DestroyCollectable()
    {
        location.buildingChecks.collectable = false;
        Destroy(gameObject);
    }

    private void CollectedCheck()
    {
        if (!(AbilityController.Instance.AbilityCollected[AbilityEnum.Sonar] && AbilityController.Instance.AbilityCollected[AbilityEnum.Artillery] && AbilityController.Instance.AbilityCollected[AbilityEnum.BuildingDefence]
            && AbilityController.Instance.AbilityCollected[AbilityEnum.FreezeFog] && AbilityController.Instance.AbilityCollected[AbilityEnum.Overclock]))
        {
            if (AbilityController.Instance.AbilityCollected[AbilityEnum.Sonar])
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(33, 19), false);
                //33,19
            }
            else
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(33, 19), true);
            }

            if (AbilityController.Instance.AbilityCollected[AbilityEnum.Artillery])
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(30, 11), false);
                //30,11
            }
            else
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(30, 11), true);
            }

            if (AbilityController.Instance.AbilityCollected[AbilityEnum.BuildingDefence])
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(20, 14), false);
                //20,14
            }
            else
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(20, 14), true);
            }

            if (AbilityController.Instance.AbilityCollected[AbilityEnum.FreezeFog])
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(12, 14), false);
                //12,14
            }
            else
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(12, 14), true);
            }

            if (AbilityController.Instance.AbilityCollected[AbilityEnum.Overclock])
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(32, 30), false);
                //32,30
            }
            else
            {
                WorldController.Instance.abilityTilestoggle(WorldController.Instance.GetTileAt(32, 30), true);
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