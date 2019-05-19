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
    CameraControls,
    BuildGenerator,
    BuildRelay,
    BuildBattery,
    IncreasePowerGeneration,
    BuildHarvesters,
    BuildArcDefence,
    BuildRepelFan,
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

    [SerializeField] private Transform camera;
    [SerializeField] private Landmark arcDefenceLandmark;
    [SerializeField] private Landmark batteryLandmark;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark relayLandmark;
    [SerializeField] private Landmark repelFanLandmark;
    [SerializeField] private Locatable buildingTarget;

    //Note: if new UI buttons will be used, they need to have btnTutorial added

    [SerializeField] private Color uiNormalColour;
    [SerializeField] private Color uiHighlightColour;

    [SerializeField] private int powerGainGoal;
    [SerializeField] private int mineralHarvestingGoal;

    //Non-Serialized Fields
    [SerializeField] private TutorialStage tutorialStage = TutorialStage.CrashLanding;
    [SerializeField] private int subStage = 1;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;

    [SerializeField] private TileData currentTile = null;
    private TileData lastTileChecked;

    private ButtonType currentlyLerping;

    private DecalProjectorComponent targetDecal = null;
    private float decalMin = 1.5f;
    private float decalMax = 3f;
    private float lerpMultiplier = 1f;
    private float lerpProgress = 0f;
    private bool lerpForward = true;

    private Vector3 cameraStartPosition;
    private Quaternion cameraStartRotation;

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
            Fog.Instance.enabled = true;
            Fog.Instance.SpawnStartingFog();
            Fog.Instance.ActivateFog();
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
        if (tutorialStage != TutorialStage.Finished && targetDecal.enabled)
        {
            LerpDecal();
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
            case TutorialStage.CameraControls:
                CameraControls();
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
            case TutorialStage.BuildHarvesters:
                BuildHarvesters();
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
        tutorialStage = TutorialStage.CameraControls;
    }

    //Tutorial Stage 2: AI Walks Player Through How to do Stuff
    private void CameraControls()
    {
        switch (subStage)
        {
            case 1:
                Fog.Instance.enabled = true;
                Fog.Instance.SpawnStartingFog();
                SendDialogue("explain situation", 2);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 3:
                SendDialogue("move camera", 1);
                cameraStartPosition = camera.position;
                break;
            case 4:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (CameraMoved(cameraStartPosition, camera.position))
                {
                    SkipTutorialAhead(6);
                }

                break;
            case 5:
                if (CameraMoved(cameraStartPosition, camera.position))
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                SendDialogue("rotate camera", 1);
                cameraStartRotation = camera.rotation;
                break;
            case 7:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (CameraRotated(cameraStartRotation.eulerAngles, camera.rotation.eulerAngles))
                {
                    SkipTutorialAhead(9);
                }

                break;
            case 8:
                if (CameraRotated(cameraStartRotation.eulerAngles, camera.rotation.eulerAngles))
                {
                    IncrementSubStage();
                }

                break;
            case 9:
                tutorialStage = TutorialStage.BuildGenerator;
                currentlyBuilding = BuildingType.Generator;
                ResetSubStage();

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Inaccurate sub stage");
                break;
        }
    }

    private void BuildGenerator()
    {
        switch (subStage)
        {
            case 1:
                GetLocationOf(generatorLandmark);
                ActivateTarget(generatorLandmark);
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build generator decal", 1);
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
                SendDialogue("build generator icon", 1);
                break;
            case 5:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (BuiltCurrentlyBuilding())
                {
                    SkipTutorialAhead(7);
                }

                break;
            case 6:
                if (BuiltCurrentlyBuilding())
                {
                    IncrementSubStage();
                }

                break;
            case 7:
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
                IncrementSubStage();
                break;
            case 5:
                if (BuiltCurrentlyBuilding())
                {
                    IncrementSubStage();
                }

                break;
            case 6:
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
                IncrementSubStage();
                break;
            case 5:
                if (ResourceController.Instance.PowerChange >= powerGainGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                tutorialStage = TutorialStage.BuildHarvesters;
                currentlyBuilding = BuildingType.Harvester;
                ResetSubStage();
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    private void BuildHarvesters()
    {
        switch (subStage)
        {
            case 1:
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build harvesters", 1);
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
                currentlyLerping = ButtonType.Harvester;
                IncrementSubStage();
                break;
            case 5:
                if (Hub.Instance.GetHarvesters().Count == 3)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                tutorialStage = TutorialStage.BuildArcDefence;
                currentlyBuilding = BuildingType.ArcDefence;
                ResetSubStage();
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    private void BuildArcDefence()
    {
        switch (subStage)
        {
            case 1:
                Fog.Instance.ActivateFog();
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build arc defence", 1);
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
                currentlyLerping = ButtonType.ArcDefence;
                IncrementSubStage();
                break;
            case 5:
                if (Hub.Instance.GetArcDefences().Count == 1)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                tutorialStage = TutorialStage.BuildRepelFan;
                currentlyBuilding = BuildingType.RepelFan;
                ResetSubStage();
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    private void BuildRepelFan()
    {
        switch (subStage)
        {
            case 1:
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build repel fan", 1);
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
                currentlyLerping = ButtonType.RepelFan;
                IncrementSubStage();
                break;
            case 5:
                if (Hub.Instance.GetRepelFans().Count == 1)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                currentlyBuilding = BuildingType.None;
                SendDialogue("finished", 1);
                break;
            case 7:
                if (dialogueRead)
                {
                    tutorialStage = TutorialStage.Finished;
                    ResetSubStage();
                    ObjectiveController.Instance.IncrementStage();
                    MusicController.Instance.StartStage1();
                }

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Utility Methods------------------------------------------------------------------------------

    private bool CameraMoved(Vector3 startPosition, Vector3 currentPosition)
    {
        if (currentPosition.x + 5 < startPosition.x ||
            currentPosition.x - 5 > startPosition.x ||
            currentPosition.z + 5 < startPosition.z ||
            currentPosition.z - 5 > startPosition.z)
        {
            return true;
        }

        return false;
    }

    private bool CameraRotated(Vector3 startRotation, Vector3 currentRotation)
    {
        Debug.Log(currentRotation.x + ", " + currentRotation.y + ", " + currentRotation.z);
        if (currentRotation.y + 5 < startRotation.y ||
            currentRotation.y - 5 > startRotation.y)
        {
            Debug.Log("Camera Rotated");
            return true;
        }

        return false;
    }

    public bool TileAllowed(TileData tile)
    {
        lastTileChecked = tile;

        if (tutorialStage >= TutorialStage.IncreasePowerGeneration  || tile == currentTile)
        {
            return true;
        }

        return false;
    }

    public bool ButtonAllowed(ButtonType button)
    {
        if ((tutorialStage == TutorialStage.Finished || button == currentlyLerping) && ButtonsNormallyAllowed(lastTileChecked).Contains(button))
        {
            return true;
        }

        return false;
    }

    private List<ButtonType> ButtonsNormallyAllowed(TileData tile)
    {
        List<ButtonType> result = new List<ButtonType>();

        if (tile.Resource != null)
        {
            result.Add(ButtonType.Harvester);
        }
        else
        {
            if (ResourceController.Instance.Generators.Count < ObjectiveController.Instance.GeneratorLimit)
            {
                result.Add(ButtonType.Generator);
            }

            result.Add(ButtonType.ArcDefence);
            result.Add(ButtonType.Battery);
            result.Add(ButtonType.Relay);
            result.Add(ButtonType.RepelFan);
        }

        return result;
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
        MouseController.Instance.ReportTutorialClick = false;

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
        MouseController.Instance.ReportTutorialClick = false;

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

