using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.UI;

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

public enum ButtonType
{
    None,
    BuildSelect,
    Battery,
    ClusterFan,
    Generator,
    Harvester,
    Relay,
    Destroy
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

    [SerializeField] private Color uiNormalColour;
    [SerializeField] private Color uiHighlightColour;
    [SerializeField] private btnTutorial btnBuildSelect;
    [SerializeField] private btnTutorial btnBuildHarvester;
    [SerializeField] private btnTutorial btnBuildGenerator;
    [SerializeField] private btnTutorial btnBuildRelay;
    [SerializeField] private btnTutorial btnBuildClusterFan;

    [SerializeField] private TutorialStage tutorialStage = TutorialStage.CrashLanding;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;
    [SerializeField] private int currentTileX = 0;
    [SerializeField] private int currentTileZ = 0;
    [SerializeField] private int subStage = 1;

    //Non-Serialized Fields
    private bool buttonClicked = false;
    private btnTutorial btnCurrent;
    private ButtonType currentlyLerping;

    private TileData currentTile = null;
    private DecalProjectorComponent targetDecal = null;
    private float decalMin = 1.5f;
    private float decalMax = 3f;

    private float lerpMultiplier = 1f;
    private float lerpProgress = 0f;
    private bool lerpForward = true;

    private Dictionary<string, List<string>> dialogue = new Dictionary<string, List<string>>();
    private bool dialogueSent = false;

    //Public Properties
    public TutorialStage TutorialStage { get => tutorialStage; }
    public BuildingType CurrentlyBuilding { get => currentlyBuilding; }
    public ButtonType CurrentlyLerping { get => currentlyLerping; }
    public TileData CurrentTile { get => currentTile; }
    public Color UINormalColour { get => uiNormalColour; }
    public Color UIHighlightColour { get => uiHighlightColour; }

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
            SetupDialogue();
        }
    }

    private void SetupDialogue()
    {
        dialogue["build cluster fan"] = new List<string>();
        dialogue["build cluster fan"].Add("Build Cluster Fan (Click to Continue)");
        
        dialogue["build generator"] = new List<string>();
        dialogue["build generator"].Add("Build Generator (Click to Continue)");

        dialogue["build harvester"] = new List<string>();
        dialogue["build harvester"].Add("Build Harvester (Click to Continue)");

        dialogue["build relay"] = new List<string>();
        dialogue["build relay"].Add("Build Relay (Click to Continue)");
    }

    //Tutorial Stage Management Methods------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        if (tutorialStage != TutorialStage.Finished)
        {
            if (dialogueRead)
            {
                dialogueSent = false;
                subStage += 1;
                ResetDialogueRead();
            }

            if (buttonClicked)
            {
                buttonClicked = false;
                btnCurrent.ReportClick = false;
                subStage += 1;
            }

            //if (lerpUI)
            //{
            //    btnCurrent.LerpButtonColour();
            //}

            if (targetDecal.enabled)
            {
                LerpDecal();
            }
        }

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
        //Activate DialogueBox, passing dialogue to it

        tutorialStage = TutorialStage.BuildHarvester;
        currentlyBuilding = BuildingType.Harvester;
    }

    //AI explains how to build a harvester and how they work
    private void BuildHarvester()
    {
        if (subStage == 1)
        {
            SendDialogue("build harvester");
        }
        else if (subStage == 2)
        {
            //Display UI element prompting player to click the building selector button
            //lerpUI = true;
            btnCurrent = btnBuildSelect;
            btnCurrent.ReportClick = true;
            currentlyLerping = ButtonType.BuildSelect;
        }
        else if (subStage == 3)
        {
            //Reset UI lerping
            btnCurrent.ReportClick = false;

            //Display UI element prompting player to select the harvester
            btnCurrent = btnBuildHarvester;
            btnCurrent.ReportClick = true;
            currentlyLerping = ButtonType.Harvester;
        }
        else if (subStage == 4)
        {
            //Turn off UI lerping
            btnCurrent.ReportClick = false;
            currentlyLerping = ButtonType.None;

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
            SendDialogue("build generator");
        }
        else if (subStage == 2)
        {
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
            SendDialogue("build relay");
        }
        else if (subStage == 2)
        {
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
            SendDialogue("build cluster fan");
        }
        else if (subStage == 2)
        {
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

    private void SendDialogue(string dialogueKey)
    {
        if (!dialogueSent)
        {
            //Activate DialogueBox, passing dialogue to it
            aiText.ActivateDialogueBox(dialogue[dialogueKey]);

            //Set dialogueSent to true so that the dialogue box isn't being repeatedly activated
            dialogueSent = true;
        }
    }

    public void RegisterButtonClicked()
    {
        buttonClicked = true;
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

    private void ActivateTarget(Locatable l)
    {
        buildingTarget.Location = currentTile;
        buildingTarget.transform.position = l.transform.position;
        targetDecal.enabled = true;

        lerpProgress = 0f;
        lerpForward = true;
    }

    private void LerpDecal()
    {
        float lerped = Mathf.Lerp(decalMin, decalMax, lerpProgress);
        targetDecal.m_Size.Set(lerped, 1, lerped);

        UpdateLerpValues();

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

    private void UpdateLerpValues()
    {
        if (lerpForward)
        {
            lerpProgress += Time.deltaTime * lerpMultiplier;
        }
        else
        {
            lerpProgress -= Time.deltaTime * lerpMultiplier;
        }

        if (lerpProgress > 1)
        {
            lerpProgress = 1;
            lerpForward = false;
        }
        else if (lerpProgress < 0)
        {
            lerpProgress = 0;
            lerpForward = true;
        }
    }
}
