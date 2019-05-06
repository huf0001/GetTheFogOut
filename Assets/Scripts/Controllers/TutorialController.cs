using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.UI;

//Unnecessary: ExplainSituation and ExplainBuildingPlacement
public enum TutorialStage
{
    None,
    CrashLanding,
    ShipPartsCrashing,
    ZoomBackToShip,
    ExplainSituation,
    ExplainBuildingPlacement,
    BuildHarvester,
    BuildGenerator,
    BuildRelay,
    FogIsHazard,
    BuildArcDefence,
    BuildRepelFan,
    Finished
}

public enum ButtonType
{
    None,
    ArcDefence,
    BuildSelect,
    Battery,
    Generator,
    Harvester,
    Relay,
    RepelFan,
    Destroy
}

public class TutorialController : DialogueBoxController
{
    //Fields---------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private bool skipTutorial = true;

    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark relayLandmark;
    [SerializeField] private Landmark arcDefenceLandmark;
    [SerializeField] private Landmark repelFanLandmark;
    [SerializeField] private Locatable buildingTarget;

    //Note: if new UI buttons will be used, they need to have btnTutorial added
    //and have the ReportClick method added to their list of OnClick methods
    [SerializeField] private btnTutorial btnBuildSelect;
    [SerializeField] private btnTutorial btnBuildArcDefence;
    [SerializeField] private btnTutorial btnBuildGenerator;
    [SerializeField] private btnTutorial btnBuildHarvester;
    [SerializeField] private btnTutorial btnBuildRelay;
    [SerializeField] private btnTutorial btnBuildRepelFan;

    [SerializeField] private Color uiNormalColour;
    [SerializeField] private Color uiHighlightColour;

    //Non-Serialized Fields
    [SerializeField] private TutorialStage tutorialStage = TutorialStage.CrashLanding;
    [SerializeField] private int subStage = 1;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;

    [SerializeField] private TileData currentTile = null;

    private btnTutorial btnCurrent;
    private ButtonType currentlyLerping;

    private DecalProjectorComponent targetDecal = null;
    private float decalMin = 1.5f;
    private float decalMax = 3f;
    private float lerpMultiplier = 1f;
    private float lerpProgress = 0f;
    private bool lerpForward = true;

    private bool fogSpawned = false;

    private bool atReworkLimit = false;

    //Public Properties
    // public static TutorialController used to get the instance of the WorldManager from anywhere.
    public static TutorialController Instance { get; protected set; }
    public TutorialStage TutorialStage { get => tutorialStage; }
    public TileData CurrentTile { get => currentTile; }
    public BuildingType CurrentlyBuilding { get => currentlyBuilding; }
    public ButtonType CurrentlyLerping { get => currentlyLerping; }
    public Color UINormalColour { get => uiNormalColour; }
    public Color UIHighlightColour { get => uiHighlightColour; }

    //Start-Up Methods-----------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more tutorial managers.");
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (skipTutorial)
        {
            GetComponent<Fog>().SpawnStartingFog();
            tutorialStage = TutorialStage.Finished;
            ObjectiveController.Instance.IncrementStage();
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
        if (tutorialStage != TutorialStage.Finished)
        {
            if (atReworkLimit)
            {
                if (dialogueRead)
                {
                    DismissDialogue();
                }

                if (buttonClicked)
                {
                    DismissButton();
                }
            }

            if (targetDecal.enabled)
            {
                LerpDecal();
            }
        }

        CheckTutorialStage();
    }

    private void CheckTutorialStage()
    {
        switch (tutorialStage)
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
            //case TutorialStage.ExplainSituation:
            //    ExplainSituation();
            //    break;
            //case TutorialStage.ExplainBuildingPlacement:
            //    ExplainBuildingPlacement();
            //    break;
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
            case TutorialStage.BuildArcDefence:
                BuildArcDefence();
                break;
            case TutorialStage.BuildRepelFan:
                BuildRepelFan();
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
        // change stage to get top down view
        tutorialStage = TutorialStage.ZoomBackToShip;
    }

    private void DroneLeavesShipToGetTopDownView()
    {
        //Run animation / camera movement for watching a drone leavig the ship
        tutorialStage = TutorialStage.ZoomBackToShip;
    }

    //Zoom back to ship
    private void ZoomBackToShip()
    {
        //Run camera movement to move camera back to the hub

        //tutorialStage = TutorialStage.ExplainSituation;
        tutorialStage = TutorialStage.BuildHarvester;
        currentlyBuilding = BuildingType.Harvester;
    }

    //Tutorial Stage 2: AI Explains Basic Building Placement
    //AI explains player's situation
    //private void ExplainSituation()
    //{
    //    if (subStage == 1)
    //    {
    //        SendDialogue("explain situation", 2);
    //    }
    //    else if (subStage == 2)
    //    {
    //        tutorialStage = TutorialStage.ExplainBuildingPlacement;
    //        ResetSubStage();
    //    }
    //}

    //AI explains how to place buildings
    //private void ExplainBuildingPlacement()
    //{
    //    if (subStage == 1)
    //    {
    //    }
    //    else if (subStage == 2)
    //    {
    //        tutorialStage = TutorialStage.BuildHarvester;
    //        currentlyBuilding = BuildingType.Harvester;
    //        ResetSubStage();
    //    }
    //}

    private void BuildHarvester()
    {
        if (subStage == 1)
        {
            if (!instructionsSent)
            {
                //Get location of resource node
                GetLocationOf(harvesterResource);

                //Display UI element prompting player to build a harvester on this resource node
                ActivateTarget(harvesterResource);

                MouseController.Instance.ReportTutorialClick = true;

                //AI explains player's situation
                SendDialogue("explain situation", 2);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                DeactivateTarget();
                SkipTutorialAhead(5);
            }
        }
        else if (subStage == 2)
        {
            if (!instructionsSent)
            {
                //AI explains how to place buildings
                SendDialogue("explain building placement", 1);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                DeactivateTarget();
                SkipTutorialAhead(5);
            }
        }
        else if (subStage == 3)
        {
            if (!instructionsSent)
            {
                //AI explains how to build a harvester and how they work
                SendDialogue("build harvester menu icon", 1);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                DeactivateTarget();
                SkipTutorialAhead(5);
            }
        }
        else if (subStage == 4 && buttonClicked)
        {
            DeactivateTarget();
            DismissButton();
        }
        else if (subStage == 5)
        {
            if (!instructionsSent)
            {
                //Display UI element prompting player to select the harvester
                currentlyLerping = ButtonType.Harvester;
                btnCurrent = btnBuildHarvester;
                btnCurrent.ReportClick = true;

                SendDialogue("build harvester harvester icon", 0);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                SkipTutorialAhead(7);
            }
        }
        else if (subStage == 6 && buttonClicked)
        {
            DismissButton();
        }
        else if (subStage == 7)
        {
            if (BuiltCurrentlyBuilding())
            {
                //Turn off UI element prompting player to build a harvester on the resource node
                tutorialStage = TutorialStage.BuildGenerator;
                currentlyBuilding = BuildingType.Generator;
                //tutorialStage = TutorialStage.Finished;
                //currentlyBuilding = BuildingType.None;
                ResetSubStage();
                //DeactivateTarget();
            }
            else
            {
                Debug.Log("You built a thing. You built the wrong thing. Something is broken in the scene or TutorialController.");
            }
        }
    }

    private void BuildGenerator()
    {
        if (subStage == 1)
        {
            if (!instructionsSent)
            {
                //Get location of resource node
                GetLocationOf(generatorLandmark);

                //Display UI element prompting player to build a harvester on this resource node
                ActivateTarget(generatorLandmark);

                MouseController.Instance.ReportTutorialClick = true;

                //AI explains player's situation
                SendDialogue("build generator", 2);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                DeactivateTarget();
                SkipTutorialAhead(3);
            }
        }
        else if (subStage == 2 && buttonClicked)
        {
            DeactivateTarget();
            DismissButton();
        }
        else if (subStage == 3)
        {
            if (!instructionsSent)
            {
                //Display UI element prompting player to click the generator button
                currentlyLerping = ButtonType.Generator;
                btnCurrent = btnBuildGenerator;
                btnCurrent.ReportClick = true;

                instructionsSent = true;
            }
            else if (buttonClicked)
            {
                DismissButton();

                instructionsSent = false;
            }
        }
        else if (subStage == 4 && BuiltCurrentlyBuilding())
        {
            //tutorialStage = TutorialStage.Finished;
            //currentlyBuilding = BuildingType.None;
            tutorialStage = TutorialStage.BuildRelay;
            currentlyBuilding = BuildingType.Relay;
            ResetSubStage();
            //DeactivateTarget();
        }
    }

    //AI helps player build a relay and explains how they work
    private void BuildRelay()
    {
        if (subStage == 1)
        {
            if (!instructionsSent)
            {
                //Get location of relay landmark
                GetLocationOf(relayLandmark);

                //Display UI element prompting player to build a relay at the indicated location
                ActivateTarget(relayLandmark);

                MouseController.Instance.ReportTutorialClick = true;

                //AI prompts player to build a relay
                SendDialogue("build relay", 2);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                DeactivateTarget();
                SkipTutorialAhead(3);
            }
        }
        else if (subStage == 2 && buttonClicked)
        {
            DeactivateTarget();
            DismissButton();
        }
        else if (subStage == 3)
        {
            if (!instructionsSent)
            {
                //Display UI element prompting player to click the relay button
                currentlyLerping = ButtonType.Relay;
                btnCurrent = btnBuildRelay;
                btnCurrent.ReportClick = true;

                instructionsSent = true;
            }
            else if (buttonClicked)
            {
                DismissButton();

                instructionsSent = false;
            }
        }
        else if (subStage == 4 && BuiltCurrentlyBuilding())
        {
            //tutorialStage = TutorialStage.Finished;
            //currentlyBuilding = BuildingType.None;
            tutorialStage = TutorialStage.FogIsHazard;
            currentlyBuilding = BuildingType.None;
            ResetSubStage();
            //DeactivateTarget();
        }
    }

    private void FogIsHazard()
    {
        if (subStage == 1)
        {
            if (!instructionsSent)
            {
                SendDialogue("fog hazard detect", 2);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
        }
        else if (subStage == 2)
        {
            if (!fogSpawned)
            {
                //Spawn fog units around hub
                GetComponent<Fog>().SpawnStartingFog(StartConfiguration.SurroundingHub);
                Invoke("IncrementSubStage", 2);
                fogSpawned = true;
            }
        }
        else if (subStage == 3)
        {
            tutorialStage = TutorialStage.BuildArcDefence;
            currentlyBuilding = BuildingType.ArcDefence;
            ResetSubStage();
        }
    }
    
    //AI tells player to build an arc defence and explains how they work
    private void BuildArcDefence()
    {
        if (subStage == 1)
        {
            if (!instructionsSent)
            {
                //Get location of the landmark
                GetLocationOf(arcDefenceLandmark);

                //Display UI element prompting player to build an arc defence at this landmark
                ActivateTarget(arcDefenceLandmark);

                MouseController.Instance.ReportTutorialClick = true;

                //AI explains player's situation
                SendDialogue("fog hazard will kill you", 1);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                DeactivateTarget();
                SkipTutorialAhead(4);
            }
        }
        else if (subStage == 2)
        {
            if (!instructionsSent)
            {
                //AI explains how to place buildings
                SendDialogue("build arc defence", 1);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                DeactivateTarget();
                SkipTutorialAhead(4);
            }
        }
        else if (subStage == 3 && buttonClicked)
        {
            DeactivateTarget();
            DismissButton();
        }
        else if (subStage == 4)
        {
            if (!instructionsSent)
            {
                //Display UI element prompting player to click the generator button
                currentlyLerping = ButtonType.ArcDefence;
                btnCurrent = btnBuildArcDefence;
                btnCurrent.ReportClick = true;

                instructionsSent = true;
            }
            else if (buttonClicked)
            {
                DismissButton();

                instructionsSent = false;
            }
        }
        else if (subStage == 5 && BuiltCurrentlyBuilding())
        {
            tutorialStage = TutorialStage.BuildRepelFan;
            currentlyBuilding = BuildingType.RepelFan;
            ResetSubStage();
        }
    }
       
    // AI tells player to build a repel fan and explains how they work ...
    private void BuildRepelFan()
    {
        if (subStage == 1)
        {
            if (!instructionsSent)
            {
                //Get location of resource node
                GetLocationOf(repelFanLandmark);

                //Display UI element prompting player to build a harvester on this resource node
                ActivateTarget(repelFanLandmark);

                MouseController.Instance.ReportTutorialClick = true;

                //AI explains player's situation
                SendDialogue("build repel fan", 2);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
            else if (buttonClicked)
            {
                DeactivateTarget();
                SkipTutorialAhead(3);
            }
        }
        else if (subStage == 2 && buttonClicked)
        {
            DeactivateTarget();
            DismissButton();
        }
        else if (subStage == 3)
        {
            if (!instructionsSent)
            {
                //Display UI element prompting player to click the generator button
                currentlyLerping = ButtonType.RepelFan;
                btnCurrent = btnBuildRepelFan;
                btnCurrent.ReportClick = true;

                instructionsSent = true;
            }
            else if (buttonClicked)
            {
                DismissButton();

                instructionsSent = false;
            }
        }
        else if (subStage == 4 && BuiltCurrentlyBuilding())
        {
            if (!instructionsSent)
            {
                DeactivateTarget();
                SendDialogue("gloat", 5);
            }
            else if (dialogueRead)
            {
                DismissDialogue();
            }
        }
        else if (subStage == 5)
        {
            tutorialStage = TutorialStage.Finished;
            currentlyBuilding = BuildingType.None;
            ResetSubStage();
            ObjectiveController.Instance.IncrementStage();
            GetComponent<Fog>().enabled = true;
        }
    }

    //Utility Methods------------------------------------------------------------------------------

    public bool ButtonAllowed(ButtonType button)
    {
        if (tutorialStage == TutorialStage.Finished || button == currentlyLerping)
        {
            return true;
        }

        return false;
    }

    public bool TileAllowed(TileData tile)
    {
        if (tutorialStage == TutorialStage.Finished || tile == currentTile)
        {
            return true;
        }

        return false;
    }

    private void IncrementSubStage()
    {
        subStage++;
    }

    private void ResetSubStage()
    {
        subStage = 1;
    }

    private void DismissButton()
    {
        if (btnCurrent != null)
        {
            btnCurrent.ReportClick = false;
            btnCurrent = null;
        }
        else
        {
            MouseController.Instance.ReportTutorialClick = false;
        }

        buttonClicked = false;
        currentlyLerping = ButtonType.None;

        IncrementSubStage();
    }

    private void DismissDialogue()
    {
        instructionsSent = false;
        ResetDialogueRead();

        IncrementSubStage();
    }

    private void SkipTutorialAhead(int nextSubStage)
    {
        if (btnCurrent != null)
        {
            btnCurrent.ReportClick = false;
            btnCurrent = null;
        }
        else
        {
            MouseController.Instance.ReportTutorialClick = false;
        }

        buttonClicked = false;
        instructionsSent = false;

        subStage = nextSubStage;
        ResetDialogueRead();
    }

    private void GetLocationOf(Locatable l)
    {
        if (l != null)
        {
            currentTile = l.Location;
        }
        else
        {
            Debug.Log("Locatable l in TutorialController.GetLocationOf(Locatable l) is null");
        }

        if (currentTile == null)
        {
            Debug.Log("TutorialController.CurrentTile is null");
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

