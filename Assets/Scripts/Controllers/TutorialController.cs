using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public enum TutorialStage
{
    None,
    CrashLanding,
    ShipPartsCrashing,
    ZoomBackToShip,
    ExplainSituation,
    BuildHarvester,
    BuildGenerator,
    BuildRelay,
    FogIsHazard,
    BuildClusterFan,
    Finished
}

public class TutorialController : DialogueBoxController
{
    //Fields---------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private bool skipTutorial = true;
    [SerializeField] private DialogueBox aiText;
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark relayLandmark;
    [SerializeField] private Landmark clusterFanLandmark;
    [SerializeField] private Locatable buildingTarget;

    //Non-Serialized Fields
    [SerializeField] private TutorialStage tutorialStage = TutorialStage.CrashLanding;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;

    private TileData currentTile = null;
    [SerializeField] private int currentTileX = 0;
    [SerializeField] private int currentTileZ = 0;
    [SerializeField] private int subStage = 1;

    private DecalProjectorComponent targetDecal = null;
    private bool expandDecal = true;
    private float decalProgress = 0f;
    private float decalMin = 1.5f;
    private float decalMax = 3f;
    private float lerpMultiplier = 1f;

    //Public Properties
    public TutorialStage TutorialStage { get => tutorialStage; }
    public BuildingType CurrentlyBuilding { get => currentlyBuilding; }
    public TileData CurrentTile { get => currentTile; }

    //Start-Up Methods-----------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        if (skipTutorial)
        {
            tutorialStage = TutorialStage.Finished;
        }
        else
        {
            targetDecal = buildingTarget.GetComponent<DecalProjectorComponent>();
        }
    }

    //Tutorial Stage Management Methods------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        CheckTutorialStage();

        if (tutorialStage != TutorialStage.Finished)
        {
            if (dialogueRead)
            {
                subStage += 1;
                ResetDialogueRead();
            }

            if (targetDecal.enabled)
            {
                LerpDecal();
            }
        }        
    }

    private void CheckTutorialStage()
    {
        switch(tutorialStage)
        {
            case TutorialStage.CrashLanding:
                CrashLanding();
                break;
            case TutorialStage.ShipPartsCrashing:
                ShipPartsCrashing();
                break;
            case TutorialStage.ZoomBackToShip:
                ZoomBackToShip();
                break;
            case TutorialStage.ExplainSituation:
                ExplainSituation();
                break;
            case TutorialStage.BuildHarvester:
                BuildHarvester();
                break;
            case TutorialStage.BuildGenerator:
                BuildGenerator();
                break;
            case TutorialStage.BuildRelay:
                BuildRelay();
                break;
            case TutorialStage.FogIsHazard:
                FogIsHazard();
                break;
            case TutorialStage.BuildClusterFan:
                BuildClusterFan();
                break;
            case TutorialStage.Finished:
                //End tutorial, game is fully responsive to player's input.
                break;
        }
    }

    //Tutorial Stage 1: Start Scene Animation
    //Ship crash lands, intro scene
    private void CrashLanding()
    {
        //Run animation for the ship crash landing

        tutorialStage = TutorialStage.ShipPartsCrashing;
    }

    //Ship parts crash, x3 different scenes
    private void ShipPartsCrashing()
    {
        //Run animation / camera movement for showing the different ship parts crashing

        tutorialStage = TutorialStage.ZoomBackToShip;
    }

    //Zoom back to ship
    private void ZoomBackToShip()
    {
        //Run camera movement to move camera back to the hub

        tutorialStage = TutorialStage.ExplainSituation;
    }

    //Tutorial Stage 2: AI Explains Basic Building Placement
    //AI explains player's situation
    private void ExplainSituation()
    {
        //Get AI dialogue
        //Activate DialogueBox, passing dialogue to it

        tutorialStage = TutorialStage.BuildHarvester;
        currentlyBuilding = BuildingType.Harvester;
    }

    //AI explains how to build a harvester and how they work
    private void BuildHarvester()
    {
        if (subStage == 1)
        {
            //Get AI dialogue
            string text = "Build Harvester (Click to Continue)";

            //Activate DialogueBox, passing dialogue to it
            aiText.ActivateDialogueBox(text);
        }
        else if (subStage == 2)
        {
            //Display UI element prompting player to click the building selector button

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 3)
        {
            //Display UI element prompting player to select the harvester

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 4)
        {
            //Turn off DialogueBox
            aiText.DeactivateDialogueBox();

            //Get location of resource node
            GetLocationOf(harvesterResource);

            //Display UI element prompting player to build a harvester on this resource node
            ActivateTarget(harvesterResource);

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 5)
        {
            //Check if the player has built the harvester
            if (BuiltCurrentlyBuilding())
            { 
                //Turn off UI element prompting player to build a harvester on the resource node
                tutorialStage = TutorialStage.BuildGenerator;
                currentlyBuilding = BuildingType.Generator;
                subStage = 1;
                DeactivateTarget();
            }
        }
    }

    //AI helps player build a power generator and explains how they work
    private void BuildGenerator()
    {
        if (subStage == 1)
        {
            //Get AI dialogue
            string text = "Build Generator (Click to Continue)";

            //Activate DialogueBox, passing dialogue to it
            aiText.ActivateDialogueBox(text);
        }
        else if (subStage == 2)
        {
            //Turn off DialogueBox
            aiText.DeactivateDialogueBox();

            //Get tile
            GetLocationOf(generatorLandmark);

            //Display UI element prompting player to build a generator on this tile
            ActivateTarget(generatorLandmark);

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 3)
        {
            if (BuiltCurrentlyBuilding())
            {
                tutorialStage = TutorialStage.BuildRelay;
                currentlyBuilding = BuildingType.Relay;
                subStage = 1;
                DeactivateTarget();
            }
        }
    }

    //AI helps player build a relay and explains how they work
    private void BuildRelay()
    {
        if (subStage == 1)
        {
            //Get AI dialogue
            string text = "Build Relay (Click to Continue)";

            //Activate DialogueBox, passing dialogue to it
            aiText.ActivateDialogueBox(text);
        }
        else if (subStage == 2)
        {
            //Turn off DialogueBox
            aiText.DeactivateDialogueBox();

            //Get tile
            GetLocationOf(relayLandmark);

            //Display UI element prompting player to build a relay on this tile
            ActivateTarget(relayLandmark);

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 3)
        {
            if (BuiltCurrentlyBuilding())
            {
                tutorialStage = TutorialStage.FogIsHazard;
                currentlyBuilding = BuildingType.None;
                subStage = 1;
                DeactivateTarget();
            }
        }
    }

    //Tutorial Stage 3: AI Explains The Fog
    //AI realises the fog is a hazard
    private void FogIsHazard()
    {
        //Spawn fog units around hub

        //Get AI dialogue
        //Activate DialogueBox, passing dialogue to it

        tutorialStage = TutorialStage.BuildClusterFan;
        currentlyBuilding = BuildingType.Defence;
    }

    //AI tells player to build a cluster fan and explains how they work
    private void BuildClusterFan()
    {
        if (subStage == 1)
        {
            //Get AI dialogue
            string text = "Build Cluster Fan (Click to Continue)";

            //Activate DialogueBox, passing dialogue to it
            aiText.ActivateDialogueBox(text);
        }
        else if (subStage == 2)
        {
            //Turn off DialogueBox
            aiText.DeactivateDialogueBox();

            //Get tile
            GetLocationOf(clusterFanLandmark);

            //Display UI element prompting player to build a cluster fan on this tile
            ActivateTarget(clusterFanLandmark);

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 3)
        {
            if (BuiltCurrentlyBuilding())
            {
                tutorialStage = TutorialStage.Finished;
                currentlyBuilding = BuildingType.None;
                subStage = 1;
                DeactivateTarget();
            }
        }
    }

    //Utility Methods------------------------------------------------------------------------------

    private void GetLocationOf(Locatable l)
    {
        currentTile = l.Location;

        if (currentTile == null)
        {
            Debug.Log("TutorialController.CurrentTile is null");
        }
        else
        {
            currentTileX = currentTile.X;
            currentTileZ = currentTile.Z;
        }
    }

    private void ActivateTarget(Locatable l)
    {
        buildingTarget.Location = currentTile;
        buildingTarget.transform.position = l.transform.position;
        targetDecal.enabled = true;

        decalProgress = 0f;
        expandDecal = true;
    }

    private void LerpDecal()
    {
        float lerped = Mathf.Lerp(decalMin, decalMax, decalProgress);
        //buildingTarget.transform.lossyScale.Set(lerped, 1, lerped);
        targetDecal.m_Size.Set(lerped, 1, lerped);

        if (expandDecal)
        {
            decalProgress += Time.deltaTime * lerpMultiplier;
        }
        else
        {
            decalProgress -= Time.deltaTime * lerpMultiplier;
        }

        if (decalProgress > 1)
        {
            decalProgress = 1;
            expandDecal = false;
        }
        else if (decalProgress < 0)
        {
            decalProgress = 0;
            expandDecal = true;
        }

        //Forces the decal to show the lerping
        targetDecal.enabled = false;
        targetDecal.enabled = true;
    }

    private bool BuiltCurrentlyBuilding()
    {
        if (currentTile != null)
        {
            if (currentTile.Building != null)
            {
                if (currentTile.Building.BuildingType == currentlyBuilding)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void DeactivateTarget()
    {
        targetDecal.enabled = false;
    }
}
