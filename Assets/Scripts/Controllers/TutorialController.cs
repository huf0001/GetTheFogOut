using System;
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
    MoveCamera,
    RotateCamera,
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
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

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

    [SerializeField] private CameraKey wKey;
    [SerializeField] private CameraKey aKey;
    [SerializeField] private CameraKey sKey;
    [SerializeField] private CameraKey dKey;
    [SerializeField] private CameraKey qKey;
    [SerializeField] private CameraKey eKey;

    [SerializeField] private Color uiNormalColour;
    [SerializeField] private Color uiHighlightColour;

    [SerializeField] private int powerGainGoal = 15;
    [SerializeField] private int builtHarvestersGoal = 3;

    //Non-Serialized Fields
    private TutorialStage tutorialStage = TutorialStage.CrashLanding;
    private int subStage = 1;
    private BuildingType currentlyBuilding = BuildingType.None;

    private TileData currentTile = null;
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

    public int PowerGainGoal { get => powerGainGoal; }
    public int BuiltHarvestersGoal { get => builtHarvestersGoal; }

    //Start-Up Methods-------------------------------------------------------------------------------------------------------------------------------

    //Ensures singleton-ness
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more tutorial managers.");
        }

        Instance = this;
    }

    //Method called by WorldController to set up the tutorial's stuff; also organises the setup of the fog
    public void StartTutorial()
    {
        Fog.Instance.enabled = true;
        Fog.Instance.PopulateFogPool();

        if (skipTutorial)
        {
            Fog.Instance.SpawnStartingFog();
            Fog.Instance.InvokeActivateFog(2);
            tutorialStage = TutorialStage.Finished;
            ObjectiveController.Instance.IncrementStage();
            MusicController.Instance.SkipTutorial();
        }
        else
        {
            targetDecal = buildingTarget.GetComponent<DecalProjectorComponent>();
        }
    }

    //General Recurring Methods----------------------------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        CheckTutorialStage();

        if (tutorialStage != TutorialStage.Finished)
        {
            if (tutorialStage == TutorialStage.BuildRepelFan && subStage > 5)
            {
                UIController.instance.UpdateObjectiveText(TutorialStage.None);
            }
            else
            { 
                UIController.instance.UpdateObjectiveText(tutorialStage);
            }

            if (targetDecal.enabled)
            {
                LerpDecal();
            }
        }
    }

    //Calls the appropriate stage method depending on the current stage
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
            case TutorialStage.ExplainSituation:
            case TutorialStage.MoveCamera:
            case TutorialStage.RotateCamera:
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

    //Stage-Specific Recurring Methods - Animations--------------------------------------------------------------------------------------------------

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

    //Drone zooms out of ship to get a top-down view of it
    private void DroneLeavesShipToGetTopDownView()
    {
        //Run animation / camera movement for watching a drone leavig the ship
        tutorialStage = TutorialStage.ZoomBackToShip;
    }

    //Zoom back to ship
    private void ZoomBackToShip()
    {
        //Run camera movement to move camera back to the hub

        tutorialStage = TutorialStage.ExplainSituation;
    }

    //Stage-Specific Recurring Methods - Player Completes Tutorial-----------------------------------------------------------------------------------

    //Player learns camera controls
    private void CameraControls()
    {
        switch (subStage)
        {
            case 1:
                Fog.Instance.SpawnStartingFog();
                SendDialogue("explain situation", 2);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    tutorialStage = TutorialStage.MoveCamera;
                }

                break;
            case 3:
                SendDialogue("move camera", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                cameraStartPosition = camera.position;
                break;
            case 4:
                //GetCameraMovementInput();

                if (dialogueRead)
                {
                    DismissDialogue();
                }
                //else if (wKey.Finished && aKey.Finished && sKey.Finished && dKey.Finished)
                else if (CameraMoved(cameraStartPosition, camera.position))
                {
                    SkipTutorialAhead(6);
                    tutorialStage = TutorialStage.RotateCamera;
                }

                break;
            case 5:
                //GetCameraMovementInput();
                
                //if (wKey.Finished && aKey.Finished && sKey.Finished && dKey.Finished)
                if (CameraMoved(cameraStartPosition, camera.position))
                {
                    IncrementSubStage();
                    tutorialStage = TutorialStage.RotateCamera;
                }

                break;
            case 6:
                SendDialogue("rotate camera", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                cameraStartRotation = camera.rotation;
                break;
            case 7:
                //GetCameraRotationInput();

                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (CameraRotated(cameraStartRotation.eulerAngles, camera.rotation.eulerAngles))
                //else if (qKey.Finished && eKey.Finished)
                {
                    SkipTutorialAhead(9);
                }

                break;
            case 8:
                //GetCameraRotationInput();

                if (CameraRotated(cameraStartRotation.eulerAngles, camera.rotation.eulerAngles))
                //if (qKey.Finished && eKey.Finished)
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

    //Player learns about and places a generator
    private void BuildGenerator()
    {
        switch (subStage)
        {
            case 1:
                GetLocationOf(generatorLandmark);
                ActivateTarget(generatorLandmark);
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build generator decal", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

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

    //Player learns about and places a relay
    private void BuildRelay()
    {
        switch (subStage)
        {
            case 1:
                GetLocationOf(relayLandmark);
                ActivateTarget(relayLandmark);
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build relay decal", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

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

    //Player learns about and places a battery
    private void BuildBattery()
    {
        switch (subStage)
        {
            case 1:
                GetLocationOf(batteryLandmark);
                ActivateTarget(batteryLandmark);
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build battery decal", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

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

    //Player increases their power generation
    private void IncreasePowerGeneration()
    {
        switch (subStage)
        {
            case 1:
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("increase power generation", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

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

    //Player learns about and builds a harvester
    private void BuildHarvesters()
    {
        switch (subStage)
        {
            case 1:
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build harvesters", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

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
                if (ResourceController.Instance.Harvesters.Count == builtHarvestersGoal)
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

    //Player learns about and builds an arc defence
    private void BuildArcDefence()
    {
        switch (subStage)
        {
            case 1:
                Fog.Instance.ActivateFog();
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build arc defence", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

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

    //Player learns about and builds a repel fan
    private void BuildRepelFan()
    {
        switch (subStage)
        {
            case 1:
                MouseController.Instance.ReportTutorialClick = true;
                SendDialogue("build repel fan", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

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

    //Tutorial Utility Methods - Camera--------------------------------------------------------------------------------------------------------------

    //Checks inputs for camera movement part of the tutorial
    private void GetCameraMovementInput()
    {
        GetButtonInput("Vertical", sKey, wKey);
        GetButtonInput("Horizontal", aKey, dKey);
    }

    //Checks inputs for camera rotation part of the tutorial
    private void GetCameraRotationInput()
    {
        GetButtonInput("Q", eKey, qKey);
    }

    //Checks a specific camera input's input
    private void GetButtonInput(string input, CameraKey negativeKey, CameraKey positiveKey)
    {
        if (Input.GetAxis(input) > 0.001)
        {
            if (!positiveKey.LerpInCalled)
            {
                positiveKey.LerpIn();
            }
            else if (!positiveKey.LerpOutCalled)
            {
                positiveKey.LerpOut();
            }
        }
        else if (Input.GetAxis(input) < -0.001)
        {
            if (!negativeKey.LerpInCalled)
            {
                negativeKey.LerpIn();
            }
            else if (!negativeKey.LerpOutCalled)
            {
                negativeKey.LerpOut();
            }
        }
    }

    //Checks if the camera has moved significantly
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

    //Checks if the camera has been rotated significantly
    private bool CameraRotated(Vector3 startRotation, Vector3 currentRotation)
    {
        if (currentRotation.y + 5 < startRotation.y ||
            currentRotation.y - 5 > startRotation.y)
        {
            return true;
        }

        return false;
    }

    //Tutorial Utility Methods - Checking if X is allowed--------------------------------------------------------------------------------------------

    //Checking if a tile is acceptable
    public bool TileAllowed(TileData tile)
    {
        lastTileChecked = tile;

        if (tutorialStage >= TutorialStage.IncreasePowerGeneration  || tile == currentTile)
        {
            return true;
        }

        return false;
    }

    //Checking if a button is acceptable
    public bool ButtonAllowed(ButtonType button)
    {
        if ((tutorialStage == TutorialStage.Finished || button == currentlyLerping || button == ButtonType.Destroy) && ButtonsNormallyAllowed(lastTileChecked).Contains(button))
        {
            return true;
        }

        return false;
    }

    //Getting the normally acceptable buttons for a tile
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

        result.Add(ButtonType.Destroy);

        return result;
    }

    //Tutorial Utility Methods - Stage Progression---------------------------------------------------------------------------------------------------

    //Override of SendDialogue that calls IncrementSubStage once dialogue is sent
    protected override void SendDialogue(string dialogueKey, float invokeDelay)
    {
        base.SendDialogue(dialogueKey, invokeDelay);
        IncrementSubStage();
    }

    //Dismisses dialgue and the mouse and skips the tutorial ahead appropriately
    private void SkipTutorialAhead(int nextSubStage)
    {
        MouseController.Instance.ReportTutorialClick = false;
        tileClicked = false;
        ResetDialogueRead();
        subStage = nextSubStage;
    }

    //Dismisses the dialogue and increments the substage
    private void DismissDialogue()
    {
        ResetDialogueRead();
        IncrementSubStage();
    }

    //Dismisses the mouse and increments the substage
    private void DismissMouse()
    {
        MouseController.Instance.ReportTutorialClick = false;
        tileClicked = false;
        currentlyLerping = ButtonType.None;
        IncrementSubStage();
    }

    //Increments the substage by 1
    private void IncrementSubStage()
    {
        subStage++;
    }

    //Resets the substage to 1
    private void ResetSubStage()
    {
        subStage = 1;
    }

    //Tutorial Utility Methods - (Targeted) Building-------------------------------------------------------------------------------------------------

    //Get location of a locatable object
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

    //Activate the building target at the locatable's location
    private void ActivateTarget(Locatable l)
    {
        buildingTarget.Location = currentTile;
        buildingTarget.transform.position = l.transform.position;
        targetDecal.enabled = true;

        lerpProgress = 0f;
        lerpForward = true;
    }

    //Lerp the target decal
    private void LerpDecal()
    {
        float lerped = Mathf.Lerp(decalMin, decalMax, lerpProgress);
        targetDecal.m_Size.Set(lerped, 1, lerped);

        UpdateLerpValues();

        //Forces the decal to show the lerping
        targetDecal.enabled = false;
        targetDecal.enabled = true;
    }

    //Update the decal's lerping values
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

    //Check if the building currently being built at a specific location has been built
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

    //Deactivates the building target
    private void DeactivateTarget()
    {
        targetDecal.enabled = false;
    }
}

