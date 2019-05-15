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
    BuildGenerator,
    BuildRelay,
    BuildBattery,
    IncreasePowerGeneration,

    Finished
}

public enum ButtonType
{
    None,
    ArcDefence,
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

    [SerializeField] private Landmark arcDefenceLandmark;
    [SerializeField] private Landmark batteryLandmark;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark relayLandmark;
    [SerializeField] private Landmark repelFanLandmark;
    [SerializeField] private Locatable buildingTarget;

    //Note: if new UI buttons will be used, they need to have btnTutorial added
    //and have the ReportClick method added to their list of OnClick methods
    //[SerializeField] private btnTutorial btnBuildSelect;
    //[SerializeField] private btnTutorial btnBuildArcDefence;
    //[SerializeField] private btnTutorial btnBuildBattery;
    //[SerializeField] private btnTutorial btnBuildGenerator;
    //[SerializeField] private btnTutorial btnBuildHarvester;
    //[SerializeField] private btnTutorial btnBuildRelay;
    //[SerializeField] private btnTutorial btnBuildRepelFan;

    [SerializeField] private Color uiNormalColour;
    [SerializeField] private Color uiHighlightColour;

    //Non-Serialized Fields
    [SerializeField] private TutorialStage tutorialStage = TutorialStage.CrashLanding;
    [SerializeField] private int subStage = 1;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;

    [SerializeField] private TileData currentTile = null;

    //private bool waitingForMouseClick = false;
    //private btnTutorial btnCurrent;
    private ButtonType currentlyLerping;

    private DecalProjectorComponent targetDecal = null;
    private float decalMin = 1.5f;
    private float decalMax = 3f;
    private float lerpMultiplier = 1f;
    private float lerpProgress = 0f;
    private bool lerpForward = true;

    private bool fogSpawned = false;

    //private bool atReworkLimit = false;

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
            MusicController.Instance.SkipTutorial();
            tutorialStage = TutorialStage.Finished;
            GetComponent<Fog>().enabled = true;
            GetComponent<Fog>().SpawnStartingFog();
            //ObjectiveController.Instance.IncrementStage();
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
            //if (atReworkLimit)
            //{
            //    if (dialogueRead)
            //    {
            //        DismissDialogue();
            //    }

            //    if (buttonClicked) // || Input.GetButtonDown("Xbox_A")
            //    {
            //        DismissButton();
            //    }
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
            case TutorialStage.BuildGenerator:
                BuildGenerator();
                break;
            case TutorialStage.BuildRelay:
                BuildRelay();
                break;
            case TutorialStage.BuildBattery:
                BuildBattery();
                break;
            case TutorialStage.IncreasePowerGeneration:
                IncreasePowerGeneration();
                break;
            case TutorialStage.Finished:
                //End tutorial, game is fully responsive to player's input.
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Erroneous stage called.");
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
        tutorialStage = TutorialStage.BuildGenerator;
        currentlyBuilding = BuildingType.Generator;
    }

    //Tutorial Stage 2: AI Explains Basic Building Placement
    private void BuildGenerator()
    {
        switch (subStage)
        {
            case 1:
                SendDialogue("explain situation", 2);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 3:
                GetLocationOf(generatorLandmark);
                ActivateTarget(generatorLandmark);
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build generator decal", 1);
                break;
            case 4:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    SkipTutorialAhead(6);
                }

                break;
            case 5:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 6:
                //Display UI element prompting player to select the generator
                currentlyLerping = ButtonType.Generator;
                //btnCurrent = btnBuildGenerator;
                //btnCurrent.ReportClick = true;

                SendDialogue("build generator icon", 1);
                break;
            case 7:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (BuiltCurrentlyBuilding())
                {
                    SkipTutorialAhead(9);
                }

                break;
            case 8:
                if (BuiltCurrentlyBuilding())
                {
                    IncrementSubStage();
                }

                break;
            case 9:
                tutorialStage = TutorialStage.BuildRelay;
                currentlyBuilding = BuildingType.Relay;
                currentlyLerping = ButtonType.None;
                ResetSubStage();
                DeactivateTarget();

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Inaccurate sub stage");
                break;
        }
    }

    private void BuildRelay()
    {
        switch (subStage)
        {
            case 1:
                GetLocationOf(relayLandmark);
                ActivateTarget(relayLandmark);
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build relay decal", 1);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    SkipTutorialAhead(4);
                }

                break;
            case 3:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 4:
                currentlyLerping = ButtonType.Relay;
                //btnCurrent = btnBuildRelay;
                //btnCurrent.ReportClick = true;
                IncrementSubStage();
                break;
            case 5:
                if (BuiltCurrentlyBuilding())
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                //Turn off UI element prompting player to build a relay on the prompted tile
                tutorialStage = TutorialStage.BuildBattery;
                currentlyBuilding = BuildingType.Battery;
                ResetSubStage();
                DeactivateTarget();

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    private void BuildBattery()
    {
        switch (subStage)
        {
            case 1:
                GetLocationOf(batteryLandmark);
                ActivateTarget(batteryLandmark);
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build battery decal", 1);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    SkipTutorialAhead(4);
                }

                break;
            case 3:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 4:
                currentlyLerping = ButtonType.Battery;
                //btnCurrent = btnBuildRelay;
                //btnCurrent.ReportClick = true;
                IncrementSubStage();
                break;
            case 5:
                if (BuiltCurrentlyBuilding())
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                //Turn off UI element prompting player to build a relay on the prompted tile
                tutorialStage = TutorialStage.IncreasePowerGeneration;
                currentlyBuilding = BuildingType.Generator;
                ResetSubStage();
                DeactivateTarget();
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    private void IncreasePowerGeneration()
    {
        switch (subStage)
        {
            case 1:
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("increase power generation", 1);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    SkipTutorialAhead(4);
                }

                break;
            case 3:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 4:
                currentlyLerping = ButtonType.Generator;
                //btnCurrent = btnBuildRelay;
                //btnCurrent.ReportClick = true;
                IncrementSubStage();
                break;
            case 5:
                if (ResourceController.Instance.PowerChange >= 15)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                //Turn off UI element prompting player to build a relay on the prompted tile
                tutorialStage = TutorialStage.Finished;
                currentlyBuilding = BuildingType.None;
                ResetSubStage();
                DeactivateTarget();
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //private void FogIsHazard()
    //{
    //    if (subStage == 1)
    //    {
    //        if (!instructionsSent)
    //        {
    //            SendDialogue("fog hazard detect", 2);
    //        }
    //        else if (dialogueRead)
    //        {
    //            DismissDialogue();
    //        }
    //    }
    //    else if (subStage == 2)
    //    {
    //        if (!fogSpawned)
    //        {
    //            //Spawn fog units around hub
    //            GetComponent<Fog>().SpawnStartingFog(StartConfiguration.SurroundingHub);
    //            Invoke("IncrementSubStage", 2);
    //            fogSpawned = true;
    //        }
    //    }
    //    else if (subStage == 3)
    //    {
    //        tutorialStage = TutorialStage.BuildArcDefence;
    //        currentlyBuilding = BuildingType.ArcDefence;
    //        ResetSubStage();
    //    }
    //}

    //AI tells player to build an arc defence and explains how they work
    //private void BuildArcDefence()
    //{
    //    if (subStage == 1)
    //    {
    //        if (!instructionsSent)
    //        {
    //            //Get location of the landmark
    //            GetLocationOf(arcDefenceLandmark);

    //            //Display UI element prompting player to build an arc defence at this landmark
    //            ActivateTarget(arcDefenceLandmark);

    //            MouseController.Instance.ReportTutorialClick = true;

    //            //AI explains player's situation
    //            SendDialogue("fog hazard will kill you", 1);
    //        }
    //        else if (dialogueRead)
    //        {
    //            DismissDialogue();
    //        }
    //        else if (tileClicked)
    //        {

    //            SkipTutorialAhead(4);
    //        }
    //    }
    //    else if (subStage == 2)
    //    {
    //        if (!instructionsSent)
    //        {
    //            //AI explains how to place buildings
    //            SendDialogue("build arc defence", 1);
    //        }
    //        else if (dialogueRead)
    //        {
    //            DismissDialogue();
    //        }
    //        else if (tileClicked)
    //        {

    //            SkipTutorialAhead(4);
    //        }
    //    }
    //    else if (subStage == 3 && tileClicked)
    //    {
    //        DismissMouse();
    //    }
    //    else if (subStage == 4)
    //    {
    //        if (!instructionsSent)
    //        {
    //            //Display UI element prompting player to click the generator button
    //            currentlyLerping = ButtonType.ArcDefence;
    //            //btnCurrent = btnBuildArcDefence;
    //            //btnCurrent.ReportClick = true;

    //            instructionsSent = true;
    //        }
    //        else if (tileClicked)
    //        {
    //            DismissMouse();

    //            instructionsSent = false;
    //        }
    //    }
    //    else if (subStage == 5 && BuiltCurrentlyBuilding())
    //    {
    //        tutorialStage = TutorialStage.BuildRepelFan;
    //        currentlyBuilding = BuildingType.RepelFan;
    //        ResetSubStage();
    //        DeactivateTarget();
    //    }
    //}

    // AI tells player to build a repel fan and explains how they work ...
    //private void BuildRepelFan()
    //{
    //    if (subStage == 1)
    //    {
    //        if (!instructionsSent)
    //        {
    //            //Get location of resource node
    //            GetLocationOf(repelFanLandmark);

    //            //Display UI element prompting player to build a harvester on this resource node
    //            ActivateTarget(repelFanLandmark);

    //            MouseController.Instance.ReportTutorialClick = true;

    //            //AI explains player's situation
    //            SendDialogue("build repel fan", 2);
    //        }
    //        else if (dialogueRead)
    //        {
    //            DismissDialogue();
    //        }
    //        else if (tileClicked)
    //        {
    //            SkipTutorialAhead(3);
    //        }
    //    }
    //    else if (subStage == 2 && tileClicked)
    //    {
    //        DismissMouse();
    //    }
    //    else if (subStage == 3)
    //    {
    //        if (!instructionsSent)
    //        {
    //            //Display UI element prompting player to click the generator button
    //            currentlyLerping = ButtonType.RepelFan;
    //            //btnCurrent = btnBuildRepelFan;
    //            //btnCurrent.ReportClick = true;

    //            instructionsSent = true;
    //        }
    //        else if (tileClicked)
    //        {
    //            DismissMouse();

    //            instructionsSent = false;
    //        }
    //    }
    //    else if (subStage == 4 && BuiltCurrentlyBuilding())
    //    {
    //        if (!instructionsSent)
    //        {
    //            DeactivateTarget();
    //            SendDialogue("gloat", 5);
    //        }
    //        else if (dialogueRead)
    //        {
    //            DismissDialogue();
    //        }
    //    }
    //    else if (subStage == 5)
    //    {
    //        tutorialStage = TutorialStage.Finished;
    //        currentlyBuilding = BuildingType.None;
    //        ResetSubStage();
    //        //ObjectiveController.Instance.IncrementStage();
    //        GetComponent<Fog>().enabled = true;
    //        MusicController.Instance.StartStage1();
    //    }
    //}

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
        if (tutorialStage == TutorialStage.Finished || tutorialStage == TutorialStage.IncreasePowerGeneration || tile == currentTile)
        {
            return true;
        }

        return false;
    }

    protected override void SendDialogue(string dialogueKey, float invokeDelay)
    {
        base.SendDialogue(dialogueKey, invokeDelay);
        IncrementSubStage();
    }

    private void IncrementSubStage()
        {
            subStage++;
        }

    private void ResetSubStage()
    {
        subStage = 1;
    }

    private void DismissMouse()
    {
        //if (btnCurrent != null)
        //{
        //    btnCurrent.ReportClick = false;
        //    btnCurrent = null;
        //}
        //else
        //if (waitingForMouseClick)
        //{
        MouseController.Instance.ReportTutorialClick = false;
        //}

        tileClicked = false;
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
        //if (btnCurrent != null)
        //{
        //    btnCurrent.ReportClick = false;
        //    btnCurrent = null;
        //}
        //else
        //{
        MouseController.Instance.ReportTutorialClick = false;
        //}

        tileClicked = false;
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

