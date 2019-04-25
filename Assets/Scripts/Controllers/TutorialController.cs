using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class TutorialController : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] private bool skipTutorial = true;
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark relayLandmark;
    [SerializeField] private Landmark clusterFanLandmark;

    //Non-Serialized Fields
    [SerializeField] private TutorialStage tutorialStage = TutorialStage.CrashLanding;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;
    private TileData currentTile = null;

    [SerializeField] private int currentTileX = 0;
    [SerializeField] private int currentTileZ = 0;
    [SerializeField] private int subStage = 1;

    //Public Properties
    public TutorialStage TutorialStage { get => tutorialStage; }
    public BuildingType CurrentlyBuilding { get => currentlyBuilding; }
    public TileData CurrentTile { get => currentTile; }

    // Start is called before the first frame update
    void Start()
    {
        if (skipTutorial)
        {
            tutorialStage = TutorialStage.Finished;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckTutorialStage();
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
        //Assign AI dialogue to UI element
        //Move UI element to appropriate location
        //Display UI element

        tutorialStage = TutorialStage.BuildHarvester;
        currentlyBuilding = BuildingType.Harvester;
    }

    //AI explains how to build a harvester and how they work
    private void BuildHarvester()
    {
        if (subStage == 1)
        {
            //Get AI dialogue
            //Assign AI dialogue to UI element
            //Move UI element to appropriate location
            //Display UI element

            subStage += 1;
        }
        else if (subStage == 2)
        {
            //Display UI element prompting player to click the building selector button

            subStage += 1;
        }
        else if (subStage == 3)
        {
            //Display UI element prompting player to select the harvester

            subStage += 1;
        }
        else if (subStage == 4)
        {
            //Get location of resource node
            //currentTile = GetClosestResourceNode(Resource.Mineral);
            GetLocationOf(harvesterResource);

            //Display UI element prompting player to build a harvester on this resource node

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
            }
        }
    }

    //AI helps player build a power generator and explains how they work
    private void BuildGenerator()
    {
        //Get AI dialogue
        //Assign AI dialogue to UI element
        //Move UI element to appropriate location
        //Display UI element

        //Get tile

        //Display UI element prompting player to build a generator on this tile

        //if (tile.Building.BuildingType == BuildingType.Generator)
        //{
        tutorialStage = TutorialStage.BuildRelay;
        currentlyBuilding = BuildingType.Relay;
        //}
    }

    //AI helps player build a relay and explains how they work
    private void BuildRelay()
    {
        //Get AI dialogue
        //Assign AI dialogue to UI element
        //Move UI element to appropriate location
        //Display UI element

        //Get tile
        //Display UI element prompting player to build a relay on this tile

        //if (tile.Building.BuildingType == BuildingType.Harvester)
        //{
        tutorialStage = TutorialStage.FogIsHazard;
        currentlyBuilding = BuildingType.None;
        //}
    }

    //Tutorial Stage 3: AI Explains The Fog
    //AI realises the fog is a hazard
    private void FogIsHazard()
    {
        //Spawn fog units around hub
        //Get AI dialogue
        //Assign AI dialogue to UI element
        //Move UI element to appropriate location
        //Display UI element

        tutorialStage = TutorialStage.BuildClusterFan;
        currentlyBuilding = BuildingType.Defence;
    }

    //AI tells player to build a cluster fan and explains how they work
    private void BuildClusterFan()
    {
        //Get AI dialogue
        //Assign AI dialogue to UI element
        //Move UI element to appropriate location
        //Display UI element

        //Get tile
        //Display UI element prompting player to build a cluster fan on this tile

        //if (tile.Building.BuildingType == BuildingType.ClusterFan)
        //{
        tutorialStage = TutorialStage.Finished;
        currentlyBuilding = BuildingType.None;
        //}
    }

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

    private bool BuiltCurrentlyBuilding()
    {
        if (currentTile != null)
        {
            if (currentTile.Building != null)
            {
                if (currentTile.Building.BuildingType == BuildingType.Harvester)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
