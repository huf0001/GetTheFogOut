using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum TutorialStage
{
    None,
    ExplainSituation,
    CameraControls,
    BuildHarvesters,
    BuildExtender,
    BuildHarvestersExtended,
    WaitingForPowerDrop,
    MouseOverPowerDiagram,
    BuildGenerator,
    BuildMoreGenerators,
    CollectMinerals,
    CollectSonar,
    ActivateSonar,
    SonarActivated,
    BuildExtenderInFog,
    BuildMortar,
    BuildPulseDefence,
    DefenceActivation,
    DontBuildInFog,
    BuildDefencesInRange,
    Finished
}

public enum ButtonType
{
    None,
    AirCannon,
    Generator,
    Harvester,
    Extender,
    FogRepeller,
    Destroy,
    Upgrades
}

public class TutorialController : DialogueBoxController
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Skip Tutorial")]
    [SerializeField] private bool skipTutorial = false;

    [Header("Tutorial Game State")]
    [SerializeField] private TutorialStage stage = TutorialStage.ExplainSituation;
    [SerializeField] private int subStage = 1;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;
    [SerializeField] private ButtonType currentlyLerping;
    [SerializeField] private TileData targetTile = null;
    [SerializeField] private TileData lastTileChecked;
    [SerializeField] private GameObject objectiveCompletePrefab;

    [Header("Goals")]
    [SerializeField] private int builtHarvestersGoal;
    [SerializeField] private int builtHarvestersExtendedGoal;
    [SerializeField] private int builtGeneratorsGoal;
    [SerializeField] private int collectedMineralsGoal;

    [Header("Cameras")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CinemachineVirtualCamera sonarCamera;
    [SerializeField] private CinemachineVirtualCamera activateSonarCamera;
    [SerializeField] private CinemachineVirtualCamera artilleryCamera;
    [SerializeField] private CinemachineVirtualCamera thrusterCamera;

    [Header("Game Objects")]
    [SerializeField] private Hub hub;
    [SerializeField] public GameObject thruster;
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark extenderLandmark;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark sonarLandmark;
    [SerializeField] private Landmark activateSonarLandmark;
    [SerializeField] private Landmark fogExtenderNWLandmark;
    [SerializeField] private Landmark mortarNWLandmark;
    [SerializeField] private Landmark pulseDefenceNWLandmark;
    [SerializeField] private Landmark fogExtenderSLandmark;
    [SerializeField] private Landmark mortarSLandmark;
    [SerializeField] private Landmark pulseDefenceSLandmark;
    [SerializeField] private Locatable buildingTarget;
    [SerializeField] private DamageIndicator arrowToTargetPrefab;

    [Header("Fog Extender Landmark Constraints")]
    [SerializeField] private float felNorthWestInitialXMin;
    [SerializeField] private float felNorthWestBackupXMin;
    [SerializeField] private float felSouthXMax;
    [SerializeField] private float felSouthZMax;

    [Header("UI Elements")]
    [SerializeField] private CameraInput wKey;
    [SerializeField] private CameraInput aKey;
    [SerializeField] private CameraInput sKey;
    [SerializeField] private CameraInput dKey;
    [SerializeField] private CameraInput rmb;
    [SerializeField] private CameraInput mouse;
    [SerializeField] private CameraInput zoomIn;
    [SerializeField] private CameraInput zoomOut;
    [SerializeField] private CanvasGroup buildMenuCanvasGroup;
    [SerializeField] private Image harvesterHighlight;
    [SerializeField] private Image extenderHighlight;
    [SerializeField] private Image generatorHighlight;
    [SerializeField] private Image mortarHighlight;
    [SerializeField] private Image pulseDefenceHighlight;
    [SerializeField] private Image sonarHighlight;
    [SerializeField] private Image powerDiagram;
    [SerializeField] private Image batteryIcon;
    [SerializeField] private GameObject abilityUnlockCanvas;
    [SerializeField] private Image abilityMenu;
    [SerializeField] private RadialMenu abilitySelectorRadialMenu;
    [SerializeField] private Image uiLerpTarget;
    [SerializeField] private Slider tutProgressSlider;

    [Header("UI Lerp Ranges")]
    [SerializeField] private float decalMinLerp;
    [SerializeField] private float decalMaxLerp;
    [SerializeField] private float largeLerpMultiplier;
    [SerializeField] private float batteryIconMinLerp;
    [SerializeField] private bool multipleLerpRingsForBattery;
    [SerializeField] private float abilityMenuMinLerp;

    [Header("UI Lerp Colours")]
    [SerializeField] private Color minHarvesterColour;
    [SerializeField] private Color maxHarvesterColour;
    [SerializeField] private Color minPowerBuildingColour;
    [SerializeField] private Color maxPowerBuildingColour;
    [SerializeField] private Color minDefencesColour;
    [SerializeField] private Color maxDefencesColour;
    [SerializeField] private Color batteryEmptyColour;
    [SerializeField] private Color batteryLowColour;
    [SerializeField] private Color batteryHalfColour;
    [SerializeField] private Color batteryHighColour;
    [SerializeField] private Color batteryFullColour;
    [SerializeField] private Color abilityMenuColour;
    [SerializeField] private Color minSonarColour;
    [SerializeField] private Color maxSonarColour;

    //Non-Serialized Fields
    private MeshRenderer targetRenderer = null;
    private float lerpMultiplier = 1f;
    private float tileTargetLerpProgress = 0f;
    private bool tileTargetLerpForward = true;

    private int extendersGoal;
    private bool defencesOn = false;
    private int mortarsGoal;
    private int pulseDefencesGoal;

    private TutorialStage savedTutorialStage;
    private int savedSubStage;

    private TileData sonarLandmarkTile;
    private DamageIndicator arrowToTarget = null;

    private bool lerpUIScalingTarget = false;
    private Image currentUILerpFocus;
    private float uiMinLerp;

    private bool lerpUIColourTarget = false;
    private Image uiColourLerpTarget = null;
    private Color minLerpColour;
    private Color maxLerpColour;

    private float uiTargetLerpProgress = 0f;
    private bool uiTargetLerpForward = true;
    private bool lerpPaused = false;

    private List<Locatable> lerpTargetsRemaining;
    private bool lerpTargetLock = false;

    private Landmark fogExtenderSelectedLandmark = null;
    private Landmark mortarSelectedLandmark = null;
    private Landmark pulseDefenceSelectedLandmark = null;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public static TutorialController Instance { get; protected set; }
    public int BuiltGeneratorsGoal { get => builtGeneratorsGoal; }
    public int BuiltHarvestersGoal { get => builtHarvestersGoal; }
    public int BuiltHarvestersExtendedGoal { get => builtHarvestersExtendedGoal; }
    public int CollectedMineralsGoal { get => collectedMineralsGoal; }
    public TileData TargetTile { get => targetTile; }
    public BuildingType CurrentlyBuilding { get => currentlyBuilding; }
    public ButtonType CurrentlyLerping { get => currentlyLerping; }
    public bool DefencesOn { get => defencesOn; }
    public bool SkipTutorial { get => skipTutorial; }
    public TutorialStage Stage { get => stage; }
    public Image UILerpTarget { get => uiLerpTarget; }

    //Start-Up Methods-------------------------------------------------------------------------------------------------------------------------------

    //Ensures singleton-ness
    private void Awake()
    {
        objectiveCompletePrefab = new GameObject();

        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more tutorial managers.");
        }

        Instance = this;

        if (GlobalVars.LoadedFromMenu)
        {
            skipTutorial = GlobalVars.SkipTut;
        }

        wKey = GameObject.Find("WKey").GetComponent<CameraInput>();
        aKey = GameObject.Find("AKey").GetComponent<CameraInput>();
        sKey = GameObject.Find("SKey").GetComponent<CameraInput>();
        dKey = GameObject.Find("DKey").GetComponent<CameraInput>();
        zoomIn = GameObject.Find("ZoomInInput").GetComponent<CameraInput>();
        zoomOut = GameObject.Find("ZoomOutInput").GetComponent<CameraInput>();

        decalMinLerp = 1.5f;
        decalMaxLerp = 3f;
        batteryIconMinLerp = 0.5f;

        arrowToTarget = Instantiate(arrowToTargetPrefab, GameObject.Find("Warnings").transform).GetComponent<DamageIndicator>();
    }

    //Method called by WorldController to set up the tutorial's stuff; also organises the setup of the fog
    public void StartTutorial()
    {
        //Setup fog
        Fog.Instance.enabled = true;
        Fog.Instance.SpawnStartingFog();

        arrowToTarget.Colour = Color.cyan;
        arrowToTarget.Locatable = buildingTarget;
        arrowToTarget.On = false;

        if (skipTutorial)
        {
            stage = TutorialStage.Finished;
            defencesOn = true;
            Fog.Instance.WakeUpFog(5);
            Fog.Instance.BeginUpdatingDamage(5);
            tutProgressSlider.gameObject.SetActive(false);
            ObjectiveController.Instance.GeneratorLimit = ObjectiveController.Instance.EarlyGameGeneratorLimit;

            //For playing straight from the game scene rather than from main menu
            if (ObjectiveController.Instance.CurrStage == (int)ObjectiveStage.None)
            {
                ObjectiveController.Instance.IncrementStage();
            }
        }
        else
        {
            sonarLandmarkTile = WorldController.Instance.GetTileAt(sonarLandmark.transform.position);
            targetRenderer = buildingTarget.GetComponent<MeshRenderer>();
            ObjectiveController.Instance.GeneratorLimit = builtGeneratorsGoal;

            if (builtGeneratorsGoal > ObjectiveController.Instance.EarlyGameGeneratorLimit)
            {
                Debug.Log("Warning: TutorialController.builtGeneratorsGoal > ObjectiveController.Instance.EarlyGameGeneratorLimit. Shouldn't it be <=?");
            }

            lerpTargetsRemaining = new List<Locatable>();
            lerpTargetsRemaining.Add(harvesterResource);
            lerpTargetsRemaining.Add(extenderLandmark);
            lerpTargetsRemaining.Add(generatorLandmark);
            lerpTargetsRemaining.Add(sonarLandmark);
            lerpTargetsRemaining.Add(activateSonarLandmark);
            lerpTargetsRemaining.Add(fogExtenderNWLandmark);
            lerpTargetsRemaining.Add(mortarNWLandmark);
            lerpTargetsRemaining.Add(pulseDefenceNWLandmark);
            lerpTargetsRemaining.Add(fogExtenderSLandmark);
            lerpTargetsRemaining.Add(mortarSLandmark);
            lerpTargetsRemaining.Add(pulseDefenceSLandmark);
        }
    }

    //General Recurring Methods----------------------------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        CheckTutorialStage();

        if (stage != TutorialStage.Finished)
        {
            if (stage == TutorialStage.DefenceActivation && subStage > 5)
            {
                UIController.instance.UpdateObjectiveText(TutorialStage.None);
            }
            else
            {
                UIController.instance.UpdateObjectiveText(stage);
            }

            if (targetRenderer.enabled)
            {
                LerpDecal();
            }

            if (lerpUIScalingTarget)
            {
                LerpUIScalingTarget();
            }

            if (lerpUIColourTarget && !lerpPaused)
            {
                LerpUIColourTarget();
            }
        }
    }

    //Calls the appropriate stage method depending on the current stage
    private void CheckTutorialStage()
    {
        switch (stage)
        {
            case TutorialStage.ExplainSituation:
                ExplainSituation();
                break;
            case TutorialStage.CameraControls:
                CameraControls();
                break;
            case TutorialStage.BuildHarvesters:
                BuildHarvesters();
                break;
            case TutorialStage.BuildExtender:
                BuildExtender();
                break;
            case TutorialStage.BuildHarvestersExtended:
                BuildHarvestersExtended();
                break;
            case TutorialStage.WaitingForPowerDrop:
            case TutorialStage.MouseOverPowerDiagram:
            case TutorialStage.BuildGenerator:
            case TutorialStage.BuildMoreGenerators:
                BuildGenerator();
                break;
            case TutorialStage.CollectMinerals:
                CollectMinerals();
                break;
            case TutorialStage.CollectSonar:
            case TutorialStage.ActivateSonar:
            case TutorialStage.SonarActivated:
                CollectSonar();
                break;
            case TutorialStage.BuildExtenderInFog:
                BuildExtenderInFog();
                break;
            case TutorialStage.BuildMortar:
                BuildMortar();
                break;
            case TutorialStage.BuildPulseDefence:
                BuildPulseDefence();
                break;
            case TutorialStage.DefenceActivation:
                DefenceActivation();
                break;
            //case TutorialStage.CollectMineralsForUpgrades:
            //    CollectMineralsForUpgrades();
            //    break;
            //case TutorialStage.Upgrades:
            //    Upgrades();
            //    break;
            case TutorialStage.DontBuildInFog:
                DontBuildInFog();
                break;
            case TutorialStage.BuildDefencesInRange:
                BuildDefencesInRange();
                break;
            case TutorialStage.Finished:
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Erroneous stage called.");
                break;
        }
    }

    //Recurring Methods - Tutorial Stage Methods-----------------------------------------------------------------------------------------------------

    //Nexy explains the situation to the player
    private void ExplainSituation()
    {
        switch (subStage)
        {
            case 1:
                UIController.instance.UpdateObjectiveText(stage);
                IncrementSubStage();
                break;
            case 2:
                if (cameraController.FinishedOpeningCameraPan)
                {
                    SendDialogue("explain situation", 2);
                }

                break;
            case 3:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ResetSubStage();

                    stage = TutorialStage.CameraControls;

                    tutProgressSlider.value++;
                }

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Inaccurate sub stage");
                break;
        }
    }

    //Player learns camera controls
    private void CameraControls()
    {
        switch (subStage)
        {
            case 1:
                SendDialogue("move camera", 1);
                UIController.instance.UpdateObjectiveText(stage);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                wKey.LerpIn();
                aKey.LerpIn();
                sKey.LerpIn();
                dKey.LerpIn();
                rmb.LerpIn();
                break;
            case 2:
                GetCameraInputs();

                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (wKey.Finished && aKey.Finished && sKey.Finished && dKey.Finished)
                {
                    if (!rmb.LerpOutCalled)
                    {
                        rmb.LerpOut();
                    }

                    GoToSubStage(4);
                }
                else if (rmb.Finished)
                {
                    if (!wKey.LerpOutCalled)
                    {
                        wKey.LerpOut();
                    }

                    if (!aKey.LerpOutCalled)
                    {
                        aKey.LerpOut();
                    }

                    if (!sKey.LerpOutCalled)
                    {
                        sKey.LerpOut();
                    }

                    if (!dKey.LerpOutCalled)
                    {
                        dKey.LerpOut();
                    }

                    GoToSubStage(4);
                }

                break;
            case 3:
                GetCameraInputs();

                if (wKey.Finished && aKey.Finished && sKey.Finished && dKey.Finished)
                {
                    if (!rmb.LerpOutCalled)
                    {
                        rmb.LerpOut();
                    }

                    IncrementSubStage();
                }
                else if (rmb.Finished)
                {
                    if (!wKey.LerpOutCalled)
                    {
                        wKey.LerpOut();
                    }

                    if (!aKey.LerpOutCalled)
                    {
                        aKey.LerpOut();
                    }

                    if (!sKey.LerpOutCalled)
                    {
                        sKey.LerpOut();
                    }

                    if (!dKey.LerpOutCalled)
                    {
                        dKey.LerpOut();
                    }

                    IncrementSubStage();
                }

                break;
            case 4:
                SendDialogue("zoom camera", 1);
                zoomIn.LerpIn();
                zoomOut.LerpIn();
                mouse.LerpIn();
                break;
            case 5:
                GetCameraInputs();

                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (zoomIn.Finished && zoomOut.Finished)
                {
                    mouse.LerpOut();
                    GoToSubStage(7);
                }

                break;
            case 6:
                GetCameraInputs();

                if (zoomIn.Finished && zoomOut.Finished)
                {
                    mouse.LerpOut();
                    IncrementSubStage();
                }

                break;
            case 7:
                if (mouse.Finished)
                {
                    IncrementSubStage();
                }

                break;
            case 8:
                wKey.transform.parent.gameObject.SetActive(false);
                ResetSubStage();

                stage = TutorialStage.BuildHarvesters;

                tutProgressSlider.value++;

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Inaccurate sub stage");
                break;
        }
    }

    //Player learns about and builds a harvester
    private void BuildHarvesters()
    {
        switch (subStage)
        {
            case 1:
                ResourceController.Instance.PausePowerChange(75);
                currentlyBuilding = BuildingType.Harvester;
                SendDialogue("build harvester target", 1);
                ActivateMouse();

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ActivateTarget(harvesterResource);
                }
                else if (tileClicked)       //Player hasn't waited for tutorial help, is going ahead on their own
                {
                    ActivateUIColourLerpTarget(harvesterHighlight, minHarvesterColour, maxHarvesterColour);
                    GoToSubStage(5);
                }

                break;
            case 3:
                if (dialogueRead)           //In case the player opens the build menu, gets the glow icon dialogue, and then jumps back out of the build menu
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    DismissMouse();
                    DeactivateTarget();
                    ActivateUIColourLerpTarget(harvesterHighlight, minHarvesterColour, maxHarvesterColour);
                    IncrementSubStage();//Need to get to substage 5, DismissMouse() already increments substage by 1
                }

                break;
            case 4:
                if (tileClicked)
                {
                    DismissMouse();
                    DeactivateTarget();
                    ActivateUIColourLerpTarget(harvesterHighlight, minHarvesterColour, maxHarvesterColour);
                }

                break;
            case 5:
                currentlyLerping = ButtonType.Harvester;
                SendDialogue("build harvester icon", 1);
                break;
            case 6:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (ResourceController.Instance.Harvesters.Count == 1)
                {
                    GoToSubStage(8);
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(3);

                    //if (harvesterResource.Location != lastTileChecked)
                    //{
                    //    harvesterResource = lastTileChecked.Resource;
                    //}

                    ActivateTarget(harvesterResource);
                }

                break;
            case 7:
                if (ResourceController.Instance.Harvesters.Count == 1)
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(3);
                    ActivateTarget(harvesterResource);
                }

                break;
            case 8:
                lerpTargetLock = false;
                lerpTargetsRemaining.Remove(harvesterResource);
                DeactivateUIColourLerpTarget();
                Destroy(harvesterHighlight);
                harvesterHighlight = null;

                SendDialogue("build more harvesters", 1);
                ActivateMouse();
                break;
            case 9:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    GoToSubStage(11);
                }

                break;
            case 10:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 11:
                currentlyLerping = ButtonType.Harvester;
                IncrementSubStage();
                break;
            case 12:
                if (ResourceController.Instance.Harvesters.Count == builtHarvestersGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 13:
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                ResetSubStage();

                stage = TutorialStage.BuildExtender;

                tutProgressSlider.value++;
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player learns about and builds a power extender to reach additional mineral nodes
    private void BuildExtender()
    {
        switch (subStage)
        {
            case 1:
                currentlyBuilding = BuildingType.Extender;
                SendDialogue("build extender target", 1);
                UIController.instance.UpdateObjectiveText(stage);
                ActivateMouse();

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ActivateTarget(extenderLandmark);
                }
                else if (tileClicked)       //Handles if player ignores dialogue and goes straight to building the extender
                {
                    GoToSubStage(5);
                }

                break;
            case 3:
                if (dialogueRead)           //Handles if player opens then closes build menu; they may have not read the dialogue. Also separates from previous sub stage to ensure accurate tile availability in case the player ignored dialogue originally
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    DeactivateTarget();
                    GoToSubStage(5);
                }

                break;
            case 4:
                if (tileClicked)
                {
                    DeactivateTarget();
                    DismissMouse();
                }

                break;
            case 5:
                ActivateUIColourLerpTarget(extenderHighlight, minPowerBuildingColour, maxPowerBuildingColour);
                currentlyLerping = ButtonType.Extender;
                IncrementSubStage();
                break;
            case 6:
                if (ResourceController.Instance.Extenders.Count == 1)
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    GoToSubStage(3);    //2 rather than 3 in case the player hasn't read any of this stage's dialogue
                    DeactivateUIColourLerpTarget();

                    //if (extenderLandmark.Location != lastTileChecked)
                    //{
                    //    extenderLandmark.Location = lastTileChecked;
                    //    extenderLandmark.transform.position = lastTileChecked.Position;
                    //}

                    ActivateTarget(extenderLandmark);
                }

                break;
            case 7:
                //Turn off UI element prompting player to build a relay on the prompted tile
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                DeactivateTarget();
                DeactivateUIColourLerpTarget();
                lerpTargetLock = false;
                lerpTargetsRemaining.Remove(extenderLandmark);
                Destroy(extenderHighlight);
                extenderHighlight = null;
                ResetSubStage();

                stage = TutorialStage.BuildHarvestersExtended;

                tutProgressSlider.value++;
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player learns about and builds a harvester
    private void BuildHarvestersExtended()
    {
        switch (subStage)
        {
            case 1:
                builtHarvestersExtendedGoal = builtHarvestersGoal;

                foreach (TileData t in WorldController.Instance.ActiveTiles)
                {
                    if (t.Resource != null && t.PowerSource != null && t.Building == null && !t.FogUnitActive)
                    {
                        builtHarvestersExtendedGoal++;
                    }
                }

                currentlyBuilding = BuildingType.Harvester;
                SendDialogue("build more harvesters extended", 1);
                UIController.instance.UpdateObjectiveText(stage);
                ActivateMouse();

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
                    GoToSubStage(4);
                }

                break;
            case 3:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 4:
                if (ResourceController.Instance.Harvesters.Count == builtHarvestersExtendedGoal)
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    GoToSubStage(2);
                    ActivateMouse();
                }

                break;
            case 5:
                lerpTargetLock = false;
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                ResetSubStage();

                stage = TutorialStage.WaitingForPowerDrop;
                UIController.instance.UpdateObjectiveText(stage);

                tutProgressSlider.value++;
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player learns about the power system and builds generators
    private void BuildGenerator()
    {
        switch (subStage)
        {
            case 1:
                if (ResourceController.Instance.StoredPower <= 75)
                {
                    currentlyBuilding = BuildingType.Generator;
                    SendDialogue("explain power", 1);

                    if (!objWindowVisible)
                    {
                        ToggleObjWindow();
                    }
                }

                break;
            case 2:
                if (dialogueBox.DialogueIndex == 4)
                {
                    //DismissDialogue();
                    IncrementSubStage();
                }
                break;
            case 3:
                stage = TutorialStage.MouseOverPowerDiagram;
                //SendDialogue("mouse over power", 0);  //If switching back, remove IncrementSubstage() call
                UIController.instance.UpdateObjectiveText(stage);
                ActivateUIScalingLerpTarget(batteryIcon, batteryIconMinLerp, Color.clear);
                IncrementSubStage();
                break;
            case 4:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (powerDiagram.fillAmount == 1)
                {
                    GoToSubStage(6);
                }

                break;
            case 5:
                if (powerDiagram.fillAmount == 1)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                DeactivateUIScalingLerpTarget();

                stage = TutorialStage.BuildGenerator;
                SendDialogue("build generator target", 1);
                UIController.instance.UpdateObjectiveText(stage);
                ActivateMouse();

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 7:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ActivateTarget(generatorLandmark);
                }
                else if (tileClicked)       //If the player ignores the dialogue
                {
                    GoToSubStage(10);
                }

                break;
            case 8:
                if (dialogueRead)           //If the player opens and closes the build menu; they may not have read the dialogue
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    GoToSubStage(10);
                }

                break;
            case 9:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 10:
                DeactivateTarget();

                currentlyLerping = ButtonType.Generator;
                ActivateUIColourLerpTarget(generatorHighlight, minPowerBuildingColour, maxPowerBuildingColour);
                IncrementSubStage();
                break;
            case 11:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (ResourceController.Instance.Generators.Count == 1)
                {
                    GoToSubStage(13);
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(8);

                    //if (generatorLandmark.Location != lastTileChecked)
                    //{
                    //    generatorLandmark.Location = lastTileChecked;
                    //    generatorLandmark.transform.position = lastTileChecked.Position;
                    //}

                    ActivateTarget(generatorLandmark);
                }

                break;
            case 12:
                if (ResourceController.Instance.Generators.Count == 1)
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(8);
                    ActivateTarget(generatorLandmark);
                }

                break;
            case 13:
                ResourceController.Instance.UnPausePowerChange();
                DeactivateUIColourLerpTarget();
                lerpTargetLock = false;
                lerpTargetsRemaining.Remove(generatorLandmark);
                Destroy(generatorHighlight);
                generatorHighlight = null;

                stage = TutorialStage.BuildMoreGenerators;
                SendDialogue("build more generators", 1);
                UIController.instance.UpdateObjectiveText(stage);
                ActivateMouse();

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 14:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    GoToSubStage(16);
                }

                break;
            case 15:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 16:
                currentlyLerping = ButtonType.Generator;
                IncrementSubStage();
                break;
            case 17:
                if (ResourceController.Instance.Generators.Count == builtGeneratorsGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 18:
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                ResetSubStage();

                stage = TutorialStage.CollectMinerals;
                ObjectiveController.Instance.GeneratorLimit = ObjectiveController.Instance.EarlyGameGeneratorLimit;
                tutProgressSlider.value++;
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Inaccurate sub stage");
                break;
        }
    }

    //Checks that player has collected the required minerals
    private void CollectMinerals()
    {
        switch (subStage)
        {
            case 1:
                UIController.instance.UpdateObjectiveText(stage);
                SendDialogue("collect minerals", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 2:
                // Update objective window with 0-300 mineral gauge, and button for fix hull when gauge filled
                if (ResourceController.Instance.StoredMineral >= collectedMineralsGoal)
                {
                    GoToSubStage(4);
                }

                if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 3:
                if (ResourceController.Instance.StoredMineral >= collectedMineralsGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 4:
                if (UIController.instance.buttonClosed)
                {
                    UIController.instance.ShowRepairButton("T");
                    IncrementSubStage();
                }

                break;
            case 5:
                if (ResourceController.Instance.StoredMineral < collectedMineralsGoal)
                {
                    UIController.instance.CloseButton();
                    SendDialogue("maintain minerals", 1);
                    GoToSubStage(2);
                }
                break;
            case 6:
                // Update Hub model to fixed ship without thrusters / Particle effects
                hub.Animator.enabled = false;   //Apparently interferes with setting the repaired ship to active=true unless you disable it.
                hub.SetCurrentModel("repaired");
                StartCoroutine(CompleteTutorialObjective("You repaired damage to your ship!"));
                IncrementSubStage();
                break;
            case 7:
                break;
            case 8:
                lerpTargetLock = false;
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                ResetSubStage();

                stage = TutorialStage.CollectSonar;

                tutProgressSlider.value++;
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player collects the sonar ability canister, uses the sonar and is prompted to collect more ability canisters and the thruster
    private void CollectSonar()
    {
        switch (subStage)
        {
            case 1:
                cameraController.MovementEnabled = false;
                sonarCamera.gameObject.SetActive(true);
                SendDialogue("collect sonar", 1);
                UIController.instance.UpdateObjectiveText(stage);
                break;
            case 2:
                if (AbilityController.Instance.AbilityCollected[AbilityEnum.Sonar])
                {
                    cameraController.MovementEnabled = true;
                    sonarCamera.gameObject.SetActive(false);
                    GoToSubStage(4);
                }
                else if (dialogueRead)
                {
                    cameraController.MovementEnabled = true;
                    sonarCamera.gameObject.SetActive(false);
                    DismissDialogue();
                }

                break;
            case 3:
                if (AbilityController.Instance.AbilityCollected[AbilityEnum.Sonar])
                {
                    IncrementSubStage();
                }

                break;
            case 4:
                stage = TutorialStage.ActivateSonar;
                SendDialogue("select sonar", 1);
                ActivateUIScalingLerpTarget(abilityMenu, abilityMenuMinLerp, abilityMenuColour);
                break;
            case 5:
                if (dialogueRead)
                {
                    if (abilityUnlockCanvas.activeSelf)
                    {
                        Debug.Log("CollectSonar, substage 5, dialogue read: finishing ability unlock");
                        UIController.instance.FinishAbilityUnlock();
                    }

                    DismissDialogue();
                }
                else if (abilitySelectorRadialMenu.Radius > 0)
                {
                    if (abilityUnlockCanvas.activeSelf)
                    {
                        Debug.Log("CollectSonar, substage 5, opening ability menu: finishing ability unlock");
                        UIController.instance.FinishAbilityUnlock();
                    }

                    GoToSubStage(7);
                }

                break;
            case 6:
                if (abilitySelectorRadialMenu.Radius > 0)
                {
                    IncrementSubStage();
                }

                break;
            case 7:
                DeactivateUIScalingLerpTarget();

                ActivateUIColourLerpTarget(sonarHighlight, minSonarColour, maxSonarColour);
                IncrementSubStage();
                break;
            case 8:
                if (abilitySelectorRadialMenu.Radius == 0)
                {
                    DeactivateUIColourLerpTarget();

                    ActivateUIScalingLerpTarget(abilityMenu, abilityMenuMinLerp, abilityMenuColour);
                    GoToSubStage(5);
                }
                else if (AbilityController.Instance.IsAbilitySelected)
                {
                    GoToSubStage(10);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 9:
                if (abilitySelectorRadialMenu.Radius == 0)
                {
                    DeactivateUIColourLerpTarget();

                    ActivateUIScalingLerpTarget(abilityMenu, abilityMenuMinLerp, abilityMenuColour);
                    GoToSubStage(6);
                }
                else if (AbilityController.Instance.IsAbilitySelected)
                {
                    IncrementSubStage();
                }

                break;
            case 10:
                DeactivateUIColourLerpTarget();

                SendDialogue("activate sonar", 1);
                ActivateTarget(activateSonarLandmark);
                activateSonarCamera.gameObject.SetActive(true);
                break;
            case 11:
                if (AbilityController.Instance.AbilityTriggered[AbilityEnum.Sonar])
                {
                    GoToSubStage(13);
                }
                else if (dialogueRead)
                {
                    IncrementSubStage();
                }

                break;
            case 12:
                if (AbilityController.Instance.AbilityTriggered[AbilityEnum.Sonar])
                {
                    IncrementSubStage();
                }

                break;
            case 13:
                DeactivateTarget();
                cameraController.MovementEnabled = false;

                stage = TutorialStage.SonarActivated;
                SendDialogue("explain abilities", 1);
                activateSonarCamera.gameObject.SetActive(false);
                artilleryCamera.gameObject.SetActive(true);
                thruster.SetActive(true);
                break;
            case 14:
                if (dialogueRead)
                {
                    DismissDialogue();
                    artilleryCamera.gameObject.SetActive(false);
                    lerpTargetLock = false;
                    lerpTargetsRemaining.Remove(sonarLandmark);
                    lerpTargetsRemaining.Remove(activateSonarLandmark);

                    base.SendDialogue("detecting thruster", 0.5f);       //Using base so as to not double up on IncrementSubStage() calls
                }

                break;
            case 15:
                if (dialogueRead)
                {
                    DismissDialogue();

                    thrusterCamera.gameObject.SetActive(true);
                    base.SendDialogue("explain thruster", 0.5f);
                }

                break;
            case 16:
                if (dialogueRead)
                {
                    thrusterCamera.gameObject.SetActive(false);
                    DismissDialogue();                    
                    ResetSubStage();

                    stage = TutorialStage.BuildExtenderInFog;
                    cameraController.MovementEnabled = true;
                    tutProgressSlider.value++;
                }

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player builds an extender in the fog to see what happens
    private void BuildExtenderInFog()
    {
        switch (subStage)
        {
            case 1:
                currentlyBuilding = BuildingType.Extender;
                SendDialogue("build extender in fog", 1);
                UIController.instance.UpdateObjectiveText(stage);
                ActivateMouse();

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                //TODO: flip a coin and select NW or SSW
                //TODO: by default, programmatically select a position for the fog extender landmark that is towards the selected direction -> closest to a matching landmark?
                //Wait . . . a set of landmarks for each direction, and only reselect when that tile is occupied, keeping the new selection as close to the original as possible?
                //TODO: once fog extender is sorted for this, also need to do mortar and pulse defence to keep defences stuff consistent and not give the player whiplash
                //if (fogExtenderSelectedLandmark.Location.PowerSource == null)
                //{
                    //Debug.Log("Case 1: landmark not powered. Finding new tile");
                    PositionFogExtenderLandmark();
                //}

                //Debug.Log("Finished BuildExtenderInFog case 1");

                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    //Debug.Log("Case 2: dialogue read, activating target");
                    ActivateTarget(fogExtenderSelectedLandmark);
                }
                else if (lastTileChecked.FogUnitActive && tileClicked)
                {
                    DismissMouse();
                    GoToSubStage(5);
                }
                else if (tileClicked && buildMenuCanvasGroup.alpha == 0)
                {
                    DismissMouse();

                    //if (fogExtenderSelectedLandmark.Location.PowerSource == null || !fogExtenderSelectedLandmark.Location.FogUnitActive)
                    //{
                        //Debug.Log("Case 2: landmark not powered, last tile clicked didn't have an active fog unit. finding new tile for landmark");
                        PositionFogExtenderLandmark();
                    //}

                    //if (fogExtenderLandmark.Location != lastTileChecked && lastTileChecked.FogUnitActive)
                    //{
                    //    fogExtenderLandmark.Location = lastTileChecked;
                    //    fogExtenderLandmark.transform.position = lastTileChecked.Position;
                    //}

                    ActivateTarget(fogExtenderSelectedLandmark);
                }                

                break;
            case 3:
                if (dialogueRead)       //In case the player ignores the dialogue, but then opens and closes the build menu on a fog-covered tile
                {
                    DismissDialogue();
                }
                else if (/*lastTileChecked.FogUnitActive && */tileClicked)
                {
                    DeactivateTarget();
                    GoToSubStage(5);
                }

                break;
            case 4:
                if (/*lastTileChecked.FogUnitActive && */tileClicked)
                {
                    DeactivateTarget();
                    DismissMouse();
                }

                break;
            case 5:
                foreach (Relay e in ResourceController.Instance.Extenders)
                {
                    if (e.Location.FogUnitActive)
                    {
                        IncrementSubStage();

                        //TODO: if the call to PositionFogExtenderLandmark() in case 1 always executes, remove this call.
                        if (fogExtenderSelectedLandmark == null)
                        {
                            PositionFogExtenderLandmark();  //Ensures mortarSelectedLandmark and pulseDefenceSelectedLandmark are set
                        }
                        return;
                    }
                }

                if (buildMenuCanvasGroup.alpha == 0)
                {
                    GoToSubStage(3);

                    //if (fogExtenderLandmark.Location != lastTileChecked && lastTileChecked.FogUnitActive)
                    //{
                    //    fogExtenderLandmark.Location = lastTileChecked;
                    //    fogExtenderLandmark.transform.position = lastTileChecked.Position;
                    //}

                    //If it reaches this, then it can automatically be assumed that the extender placed was on a tile without fog, or there was no extender placed.
                    PositionFogExtenderLandmark();
                    ActivateTarget(fogExtenderSelectedLandmark);
                }

                break;

            case 6:
                foreach (Relay e in ResourceController.Instance.Extenders)
                {
                    if (e.TakingDamage)
                    {
                        Fog.Instance.Intensity = 0;
                        Fog.Instance.WakeUpFog(0);
                        IncrementSubStage();
                        return;
                    }
                }

                break;
            case 7:
                lerpTargetLock = false;
                lerpTargetsRemaining.Remove(fogExtenderSelectedLandmark);
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                ResetSubStage();

                stage = TutorialStage.BuildMortar;

                tutProgressSlider.value++;
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player learns about and builds a mortar
    private void BuildMortar()
    {
        switch (subStage)
        {
            case 1:
                currentlyBuilding = BuildingType.AirCannon;
                SendDialogue("build mortar", 1);
                UIController.instance.UpdateObjectiveText(stage);
                ActivateMouse();

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ActivateTarget(mortarSelectedLandmark);
                }
                else if (tileClicked)       //If player ignores dialogue and speedruns
                {
                    DismissMouse();
                    GoToSubStage(5);
                }

                break;
            case 3:
                if (dialogueRead)           //If player ignores dialogue, speedruns, then closes the build menu before building the thing
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    DismissMouse();
                    GoToSubStage(5);
                }

                break;
            case 4:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 5:
                DeactivateTarget();

                currentlyLerping = ButtonType.AirCannon;
                ActivateUIColourLerpTarget(mortarHighlight, minDefencesColour, maxDefencesColour);
                IncrementSubStage();
                break;
            case 6:
                if (Hub.Instance.GetMortars().Count == 1)
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(3);

                    //if (mortarLandmark.Location != lastTileChecked)
                    //{
                    //    mortarLandmark.Location = lastTileChecked;
                    //    mortarLandmark.transform.position = lastTileChecked.Position;
                    //}

                    ActivateTarget(mortarSelectedLandmark);
                }

                break;
            case 7:
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                DeactivateTarget();
                lerpTargetLock = false;
                lerpTargetsRemaining.Remove(mortarSelectedLandmark);
                DeactivateUIColourLerpTarget();
                Destroy(mortarHighlight);
                mortarHighlight = null;
                ResetSubStage();

                stage = TutorialStage.BuildPulseDefence;

                tutProgressSlider.value++;
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player learns about and builds a pulse defence
    private void BuildPulseDefence()
    {
        switch (subStage)
        {
            case 1:
                currentlyBuilding = BuildingType.FogRepeller;
                SendDialogue("build pulse defence", 1);
                UIController.instance.UpdateObjectiveText(stage);
                ActivateMouse();

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                //In case of no extenders reaching the original landmark location
                //All powered tiles will be > 0 distance away from the hub. At the very least, one around the edge of the hub's power supply range will be selected.
                if (pulseDefenceSelectedLandmark.Location.PowerSource == null)
                {
                    RepositionPulseDefenceLandmark();
                }

                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ActivateTarget(pulseDefenceSelectedLandmark);
                }
                else if (tileClicked)       //If player ignores dialogue
                {
                    foreach (TileData t in lastTileChecked.AllAdjacentTiles)
                    {
                        if (t.FogUnitActive)
                        {
                            GoToSubStage(5);
                            return;
                        }

                        foreach (TileData a in t.AllAdjacentTiles)
                        {
                            if (a.FogUnitActive)
                            {
                                GoToSubStage(5);
                                return;
                            }
                        }
                    }

                    tileClicked = false;
                }

                break;
            case 3:
                if (dialogueRead)           //If player ignores dialogue, goes to build, then closes build menu.
                {
                    DismissDialogue();
                }
                if (tileClicked && lastTileChecked == pulseDefenceSelectedLandmark.Location)
                {
                    GoToSubStage(5);
                }
                else
                {
                    tileClicked = false;
                }

                break;
            case 4:
                if (tileClicked && lastTileChecked == pulseDefenceSelectedLandmark.Location)
                {
                    DismissMouse();
                }
                else
                {
                    tileClicked = false;
                }

                break;
            case 5:
                DeactivateTarget();

                currentlyLerping = ButtonType.FogRepeller;
                ActivateUIColourLerpTarget(pulseDefenceHighlight, minDefencesColour, maxDefencesColour);
                IncrementSubStage();
                break;
            case 6:
                if (Hub.Instance.GetPulseDefences().Count == 1)
                {
                    currentlyBuilding = BuildingType.None;
                    currentlyLerping = ButtonType.None;
                    DeactivateUIColourLerpTarget();
                    lerpTargetLock = false;
                    lerpTargetsRemaining.Remove(pulseDefenceSelectedLandmark);
                    Destroy(pulseDefenceHighlight);
                    pulseDefenceHighlight = null;
                    ResetSubStage();

                    stage = TutorialStage.DefenceActivation;

                    tutProgressSlider.value++;
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(3);

                    //if (pulseDefenceLandmark.Location != lastTileChecked)
                    //{
                    //    pulseDefenceLandmark.Location = lastTileChecked;
                    //    pulseDefenceLandmark.transform.position = lastTileChecked.Position;
                    //}

                    ActivateTarget(pulseDefenceSelectedLandmark);
                }

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player turns the defences on, waking the fog
    private void DefenceActivation()
    {
        switch (subStage)
        {
            case 1:
                if (UIController.instance.buttonClosed)
                {
                    Debug.Log("Showing Activate Defences Button");
                    UIController.instance.ShowActivateButton();
                }

                SendDialogue("activate defences", 1);
                UIController.instance.UpdateObjectiveText(stage);
                break;
            case 2:
                if (dialogueRead)
                {
                    Debug.Log("Dismissing 'Activate Defences' dialogue");
                    DismissDialogue();
                }

                break;
            case 3:
                //Waiting for the big button to be pressed
                break;
            case 4:
                Debug.Log("Turning on pulse defence pulses, waking up fog.");
                foreach (RepelFan pd in ResourceController.Instance.PulseDefences)
                {
                    pd.GetComponentInChildren<ParticleSystem>().Play();
                }

                defencesOn = true;
                Fog.Instance.BeginUpdatingDamage();
                IncrementSubStage();
                break;
            case 5:
                foreach (FogUnit f in Fog.Instance.FogUnitsInPlay)
                {
                    if (f.TakingDamage)
                    {
                        //Fog.Instance.WakeUpFog();
                        Fog.Instance.Intensity = 1;
                        IncrementSubStage();
                        break;
                    }
                }

                break;
            case 6:
                StartCoroutine(CompleteTutorialObjective("You've activated your defences!"));
                IncrementSubStage();
                break;
            case 7:
                break;
            case 8:
                lerpTargetLock = false;
                tutProgressSlider.gameObject.SetActive(false);
                ResetSubStage();

                stage = TutorialStage.Finished;
                GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>().StageTwoMusic();
                SendDialogue("finished", 1);
                ObjectiveController.Instance.IncrementStage();
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Tutorial Utility Methods - "Error" Stages------------------------------------------------------------------------------------------------------

    //Reprimands the player if they try and build something in the fog after they find out it's dangerous.
    private void DontBuildInFog()
    {
        switch (subStage)
        {
            case 1:
                dialogueRead = false;

                if (savedTutorialStage == TutorialStage.CollectMinerals)
                {
                    SendDialogue("maybe dont build in fog", 0);
                }
                else
                {

                    SendDialogue("definitely dont build in fog", 0);
                }
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    stage = savedTutorialStage;
                    subStage = savedSubStage;
                }

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Checks that the defences built will be able to damage the fog and wake it up
    public bool DefencesOperable()
    {
        bool result = false;
        List<Defence> connectedDefences = new List<Defence>();
        connectedDefences.AddRange(Hub.Instance.GetMortars());
        connectedDefences.AddRange(Hub.Instance.GetPulseDefences());

        foreach (Defence d in connectedDefences)
        {
            if (d.TargetInRange())
            {
                result = true;
                break;
            }
        }

        if (!result)
        {
            savedTutorialStage = stage;
            savedSubStage = subStage;

            stage = TutorialStage.BuildDefencesInRange;
            subStage = 1;
        }

        return result;
    }

    //Tells the player they need to have defences positioned so they can damage the fog
    private void BuildDefencesInRange()
    {
        switch (subStage)
        {
            case 1:
                UIController.instance.UpdateObjectiveText(stage);
                SendDialogue("need defences", 0);
                mortarsGoal = Hub.Instance.GetMortars().Count + 1;
                pulseDefencesGoal = Hub.Instance.GetPulseDefences().Count + 1;
                break;
            case 2:
                if (DefencesInRange())
                {
                    GoToSubStage(4);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 3:
                if (DefencesInRange())
                {
                    IncrementSubStage();
                }

                break;
            case 4:
                SendDialogue("activate defences in range", 1);
                stage = TutorialStage.DefenceActivation;
                GoToSubStage(2);
                UIController.instance.UpdateObjectiveText(stage);

                if (UIController.instance.buttonClosed)
                {
                    UIController.instance.ShowActivateButton();
                }
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Checks if the player has built defences that are within striking distance of the fog
    private bool DefencesInRange()
    {
        List<ArcDefence> connectedMortars = Hub.Instance.GetMortars();
        List<RepelFan> connectedPulseDefences = Hub.Instance.GetPulseDefences();

        if (connectedMortars.Count >= mortarsGoal)
        {
            foreach (ArcDefence m in connectedMortars)
            {
                if (m.TargetInRange())
                {
                    return true;
                }
            }

            //Since none were built in range of the fog, increment goal
            mortarsGoal = connectedMortars.Count + 1;
        }
        else if (connectedMortars.Count < mortarsGoal - 1)
        {
            mortarsGoal = connectedMortars.Count + 1;
        }

        if (connectedPulseDefences.Count >= pulseDefencesGoal)
        {
            foreach (RepelFan pd in connectedPulseDefences)
            {
                if (pd.TargetInRange())
                {
                    return true;
                }
            }

            //Since none were built in range of the fog, increment goal
            pulseDefencesGoal = connectedPulseDefences.Count + 1;
        }
        else if (connectedPulseDefences.Count < pulseDefencesGoal - 1)
        {
            pulseDefencesGoal = connectedPulseDefences.Count + 1;
        }

        return false;
    }

    //Tutorial Utility Methods - Input---------------------------------------------------------------------------------------------------------------

    //Checks inputs for camera movement part of the tutorial
    private void GetCameraInputs()
    {
        if (subStage < 4)
        {
            GetPositiveNegativeInput("Vertical", sKey, wKey);
            GetPositiveNegativeInput("Horizontal", aKey, dKey);
            GetRMBInput();
        }
        else
        {
            GetPositiveNegativeInput("Zoom", zoomOut, zoomIn);
        }
    }

    //Checks a specific WASD / Scroll input's input
    private void GetPositiveNegativeInput(string input, CameraInput negativeInput, CameraInput positiveInput)
    {
        float inputValue;

        switch (input)
        {
            case "Vertical":
                inputValue = cameraController.Move.y;
                break;
            case "Horizontal":
                inputValue = cameraController.Move.x;
                break;
            case "Zoom":
                inputValue = cameraController.ZoomVal;
                break;
            default:
                inputValue = 0;
                break;
        }

        if (inputValue > 0 && !positiveInput.LerpOutCalled)
        {
            positiveInput.LerpOut();
        }
        else if (inputValue < 0 && !negativeInput.LerpOutCalled)
        {
            negativeInput.LerpOut();
        }
    }

    //Checks RMB input
    private void GetRMBInput()
    {
        Vector3 cameraMovement = WorldController.Instance.Inputs.InputMap.CameraDrag.ReadValue<Vector2>();

        if ((Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed) 
            && (cameraMovement.x != 0 || cameraMovement.y != 0)
            && !rmb.LerpOutCalled)
        {
            rmb.LerpOut();
        }
    }

    //Tutorial Utility Methods - Checking if X is allowed--------------------------------------------------------------------------------------------

    //Checking if a tile is acceptable
    public bool TileAllowed(TileData tile/*, bool saveTile*/)
    {
        bool tileOkay;

        if (!cameraController.FinishedOpeningCameraPan || (stage <= TutorialStage.CollectSonar && tile == sonarLandmarkTile))
        {
            return false;
        }

        switch (stage)
        {
            case TutorialStage.BuildHarvesters:
                if (subStage < 3 || subStage > 6 || !lerpTargetLock)
                {
                    tileOkay = tile.Resource != null && !tile.FogUnitActive;
                }
                else
                {
                    tileOkay = tile == targetTile;
                }

                break;
            case TutorialStage.BuildExtender:
            case TutorialStage.BuildMortar:
                if (subStage < 3 || !lerpTargetLock)
                {
                    tileOkay = tile.Resource == null && !tile.FogUnitActive;
                }
                else
                {
                    tileOkay = tile == targetTile;
                }

                break;
            case TutorialStage.BuildHarvestersExtended:
                tileOkay = tile.Resource != null && !tile.FogUnitActive;
                break;
            case TutorialStage.BuildGenerator:
                if (subStage == 7 || !lerpTargetLock)
                {
                    tileOkay = tile.Resource == null && !tile.FogUnitActive;
                }
                else
                {
                    tileOkay = tile == targetTile;
                }

                break;
            case TutorialStage.BuildMoreGenerators:
                tileOkay = tile.Resource == null && !tile.FogUnitActive;
                break;
            case TutorialStage.CollectMinerals:
            case TutorialStage.DefenceActivation:
            case TutorialStage.BuildDefencesInRange:
                tileOkay = !tile.FogUnitActive || tile.Building != null;

                if (!tileOkay && tile.FogUnitActive && !dialogueBox.Activated)
                {
                    savedTutorialStage = stage;
                    savedSubStage = subStage;

                    stage = TutorialStage.DontBuildInFog;
                    subStage = 1;
                }

                break;
            case TutorialStage.CollectSonar:
                tileOkay = !tile.FogUnitActive;
                break;
            case TutorialStage.ActivateSonar:
                tileOkay = Vector3.Distance(tile.Position, buildingTarget.transform.position) < targetRenderer.bounds.extents.x;  //.x or .z will work perfectly fine here, they'll have the radius (orthogonal extent) of the lerp target
                break;
            case TutorialStage.BuildExtenderInFog:
                tileOkay = tile.Resource == null;   //TODO: add extra substages if power doesn't reach fog? Would need to be accounted for here.
                break;
            case TutorialStage.BuildPulseDefence:
                tileOkay = (tile.Resource == null && !tile.FogUnitActive) || tile.Building != null;

                if (!tileOkay && !dialogueBox.Activated)
                {
                    savedTutorialStage = stage;
                    savedSubStage = subStage;

                    stage = TutorialStage.DontBuildInFog;
                    subStage = 1;
                }

                break;
            case TutorialStage.Finished:
                tileOkay = true;
                break;
            default:
                Debug.Log("TutorialController.TileAllowed().default case");
                tileOkay = tile == targetTile;
                break;
        }

        //if (tileOkay && (saveTile /*|| !lerpTargetLock*/) && (stage != TutorialStage.BuildExtenderInFog || tile.FogUnitActive))
        //{
        //    lastTileChecked = tile;
        //}

        if (tileOkay || !lerpTargetLock)
        {
            lastTileChecked = tile;
        }

        //lastTileChecked = tile;

        return tileOkay;
    }

    //Checking if a button is acceptable
    public bool ButtonAllowed(ButtonType button)
    {
        bool buttonOkay = false;

        if (ButtonsNormallyAllowed(lastTileChecked).Contains(button))
        {
            switch (stage)
            {
                case TutorialStage.BuildHarvesters:
                case TutorialStage.BuildHarvestersExtended:
                    buttonOkay = button == ButtonType.Harvester;
                    break;
                case TutorialStage.BuildExtender:
                    buttonOkay = button == ButtonType.Extender;
                    break;
                case TutorialStage.BuildGenerator:
                case TutorialStage.BuildMoreGenerators:
                    buttonOkay = button == ButtonType.Generator;
                    //Debug.Log($"Stage BuildGenerator / BuildMoreGenerators. ButtonType: {button}; ButtonOkay: {buttonOkay}");
                    break;
                case TutorialStage.CollectMinerals:
                case TutorialStage.CollectSonar:
                    buttonOkay = button == ButtonType.Extender
                           || button == ButtonType.Harvester
                           || button == ButtonType.Generator
                           || button == ButtonType.Destroy;
                    break;
                case TutorialStage.BuildExtenderInFog:
                    buttonOkay = button == ButtonType.Extender;
                    break;
                case TutorialStage.BuildMortar:
                    buttonOkay = button == ButtonType.AirCannon;
                    break;
                case TutorialStage.BuildPulseDefence:
                    buttonOkay = (button == ButtonType.FogRepeller /*&& subStage >= 5*/)
                           /*|| (button == ButtonType.Extender && lastTileChecked != pulseDefenceLandmark.Location)*/;
                    break;
                case TutorialStage.DefenceActivation:
                case TutorialStage.BuildDefencesInRange:
                    buttonOkay = button != ButtonType.Upgrades;
                    break;
                case TutorialStage.Finished:
                    buttonOkay = true;
                    break;
                default:
                    buttonOkay = button == currentlyLerping || button == ButtonType.Destroy;
                    break;
            }
        }

        return buttonOkay;
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

            result.Add(ButtonType.AirCannon);
            result.Add(ButtonType.Extender);
            result.Add(ButtonType.FogRepeller);
        }

        result.Add(ButtonType.Destroy);
        result.Add(ButtonType.Upgrades);
        return result;
    }

    //Tutorial Utility Methods - Stage Progression - General-----------------------------------------------------------------------------------------

    //Tells MouseController to report clicks to TutorialController
    private void ActivateMouse()
    {
        MouseController.Instance.ReportTutorialClick = true;
    }

    //Dismisses the mouse and increments the substage
    private void DismissMouse()
    {
        MouseController.Instance.ReportTutorialClick = false;
        tileClicked = false;
        currentlyLerping = ButtonType.None;
        IncrementSubStage();
    }

    //Override of SendDialogue that calls IncrementSubStage once dialogue is sent
    protected override void SendDialogue(string dialogueKey, float invokeDelay)
    {
        base.SendDialogue(dialogueKey, invokeDelay);
        IncrementSubStage();
    }

    //Dismisses the dialogue and increments the substage
    private void DismissDialogue()
    {
        ResetDialogueRead();
        IncrementSubStage();
    }

    //Increments the substage by 1
    private void IncrementSubStage()
    {
        subStage++;
    }

    //Dismisses dialogue and the mouse and advances/retreats to the specified sub-stage appropriately appropriately
    private void GoToSubStage(int nextSubStage)
    {
        MouseController.Instance.ReportTutorialClick = false;
        tileClicked = false;
        currentlyLerping = ButtonType.None;
        ResetDialogueRead();
        subStage = nextSubStage;
    }

    //Runs a "yay, you did the thing" screen
    //Borrowed and adapted from ObjectiveController
    IEnumerator CompleteTutorialObjective(string message)
    {
        GameObject objComp = Instantiate(ObjectiveController.Instance.ObjectiveCompletePrefab, GameObject.Find("Canvas").transform);//Apparently TC's prefab is broken, but OC's isn't
        GameObject objCompImage = objComp.GetComponentInChildren<Image>().gameObject;
        TextMeshProUGUI unlocksText = objCompImage.GetComponentInChildren<TextMeshProUGUI>();

        unlocksText.text = message;
        objCompImage.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-Win", GetComponent<Transform>().position);

        yield return new WaitForSecondsRealtime(3f);

        objCompImage.GetComponent<RectTransform>().DOAnchorPosX(1250, 0.3f).SetEase(Ease.InQuad).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.3f);

        Destroy(objComp);

        IncrementSubStage();
    }

    //Resets the substage to 1
    private void ResetSubStage()
    {
        subStage = 1;
    }

    //Tutorial Utility Methods - Stage Progression - Specific----------------------------------------------------------------------------------------

    //Called to advance the collect minerals stage to sub-stage 6
    public void CompleteMineralCollection()
    {
        GoToSubStage(6);
    }

    //Called to advance the defence activation stage to sub-stage 4
    public void ActivateDefences()
    {
        Debug.Log("Running TutorialController.ActivateDefences(); activate button pressed");
        GoToSubStage(4);
    }

    //Tutorial Utility Methods - (Targeted) Building-------------------------------------------------------------------------------------------------

    //In case the player cannot immediately build in the fog
    private void PositionFogExtenderLandmark()
    {
        //Get default NW and S landmark tiles
        TileData nwTile = fogExtenderNWLandmark.Location;
        TileData sTile = fogExtenderSLandmark.Location;

        //Whole-loop variables
        TileData tile = null;
        Landmark landmark = null;

        //Check default NW and S landmark tiles
        if (nwTile.PowerSource != null && nwTile.Building == null && !nwTile.buildingChecks.obstacle && !nwTile.buildingChecks.collectable && (sTile.PowerSource == null || sTile.Building != null || sTile.buildingChecks.obstacle || sTile.buildingChecks.collectable))
        {
            landmark = fogExtenderNWLandmark;
            tile = landmark.Location;
        }
        else if (sTile.PowerSource != null && sTile.Building == null && !sTile.buildingChecks.obstacle && !sTile.buildingChecks.collectable && (nwTile.PowerSource == null || nwTile.Building != null || nwTile.buildingChecks.obstacle || nwTile.buildingChecks.collectable))
        {
            landmark = fogExtenderSLandmark;
            tile = landmark.Location;
        }
        else if (nwTile.PowerSource != null && sTile.PowerSource != null && nwTile.Building == null && sTile.Building == null && !nwTile.buildingChecks.obstacle && !sTile.buildingChecks.obstacle && !nwTile.buildingChecks.collectable && !sTile.buildingChecks.collectable)
        {
            if (Random.Range(0, 2) == 0)    //Random.Range(int, int):int --> the first parameter is included, the second is excluded. A <= result < B. Therefore to get [0, 1], need to specify as [0, 2)
            {
                landmark = fogExtenderNWLandmark;
            }
            else
            {
                landmark = fogExtenderSLandmark;
            }

            tile = landmark.Location;
        }
        else
        {
            //If invalid, checks for closest valid tiles
            List<TileData> poweredFogTiles = new List<TileData>();
            float tileToSelectedLandmark = 9999;
            Landmark newLandmark = null;
            float newTileToLandmark = 9999;

            foreach (TileData t in WorldController.Instance.ActiveTiles)
            {
                if (t.FogUnitActive && t.Building == null && !t.buildingChecks.obstacle && !t.buildingChecks.collectable && (t.Z > felNorthWestInitialXMin || (t.X < felSouthXMax && t.Z < felSouthZMax)))
                {
                    poweredFogTiles.Add(t);
                }
            }

            //If found one
            if (poweredFogTiles.Count == 1)
            {
                tile = poweredFogTiles[0];
                landmark = Vector2.Distance(tile.Position, nwTile.Position) < Vector2.Distance(tile.Position, sTile.Position) ? fogExtenderNWLandmark : fogExtenderSLandmark;
            }
            //If found more than one
            else if (poweredFogTiles.Count > 1)
            {
                foreach (TileData t in poweredFogTiles)
                {
                    //Reset per-iteration variables
                    newLandmark = null;
                    newTileToLandmark = 9999;

                    if (t.X > felNorthWestInitialXMin)
                    {
                        newLandmark = fogExtenderNWLandmark;
                        newTileToLandmark = Vector3.Distance(t.Position, nwTile.Position);
                    }
                    else if (t.X < felSouthXMax && t.X < felSouthZMax)
                    {
                        newLandmark = fogExtenderSLandmark;
                        newTileToLandmark = Vector3.Distance(t.Position, sTile.Position);
                    }

                    if (newTileToLandmark < tileToSelectedLandmark)
                    {
                        landmark = newLandmark;
                        tile = t;
                    }
                }
            }

            if (landmark == null)
            {
                //Reset whole-loop variables
                tile = null;
                tileToSelectedLandmark = 9999;

                //If found none, checks for tile that will get the player the closest to default NW and S landmark tiles
                foreach (TileData t in WorldController.Instance.ActiveTiles)
                {
                    if (t.Building == null && !t.buildingChecks.obstacle && !t.buildingChecks.collectable && (t.Z > felNorthWestBackupXMin || (t.X < felSouthXMax && t.Z < felSouthZMax)))
                    {
                        //Reset per-iteration variables
                        newLandmark = null;
                        newTileToLandmark = 9999;

                        if (t.X > felNorthWestInitialXMin)
                        {
                            newLandmark = fogExtenderNWLandmark;
                            newTileToLandmark = Vector3.Distance(t.Position, nwTile.Position);
                        }
                        else if (t.X < felSouthXMax && t.X < felSouthZMax)
                        {
                            newLandmark = fogExtenderSLandmark;
                            newTileToLandmark = Vector3.Distance(t.Position, sTile.Position);
                        }

                        if (newTileToLandmark < tileToSelectedLandmark)
                        {
                            landmark = newLandmark;
                            tile = t;
                        }
                    }
                }
            }
        }

        //Precautionary check
        if (landmark != null)
        {
            fogExtenderSelectedLandmark = landmark;
            fogExtenderSelectedLandmark.Location = tile;
            fogExtenderSelectedLandmark.transform.position = tile.Position;

            //Match defences to fog extender landmark
            if (fogExtenderSelectedLandmark == fogExtenderNWLandmark)
            {
                mortarSelectedLandmark = mortarNWLandmark;
                pulseDefenceSelectedLandmark = pulseDefenceNWLandmark;
            }
            else
            {
                mortarSelectedLandmark = mortarSLandmark;
                pulseDefenceSelectedLandmark = pulseDefenceSLandmark;
            }

            //Set up lerp targets remaining properly
            lerpTargetsRemaining.Remove(fogExtenderNWLandmark);
            lerpTargetsRemaining.Remove(mortarNWLandmark);
            lerpTargetsRemaining.Remove(pulseDefenceNWLandmark);
            lerpTargetsRemaining.Remove(fogExtenderSLandmark);
            lerpTargetsRemaining.Remove(mortarSLandmark);
            lerpTargetsRemaining.Remove(pulseDefenceSLandmark);

            lerpTargetsRemaining.Add(fogExtenderSelectedLandmark);
            lerpTargetsRemaining.Add(mortarSelectedLandmark);
            lerpTargetsRemaining.Add(pulseDefenceSelectedLandmark);
        }

        //Final check with warning message
        if (fogExtenderSelectedLandmark == null)
        {
            Debug.Log("WARNING: TutorialController.fogExtenderSelectedLandmark is null; errors are gonna happen. You need to tighten up the code for picking that landmark and the defences' landmarks.");
        }
    }

    //In case the original landmark is not being supplied with power
    private void RepositionPulseDefenceLandmark()
    {
        TileData tile = null;
        float tileToHub = 0;
        float tileToOriginalLandmark = 9999;

        foreach (TileData t in WorldController.Instance.ActiveTiles)
        {
            if (t.Building == null && !t.FogUnitActive && Vector3.Distance(t.Position, pulseDefenceSelectedLandmark.Location.Position) < tileToOriginalLandmark)
            {
                tile = t;
                tileToHub = Vector3.Distance(t.Position, hub.Location.Position);
                tileToOriginalLandmark = Vector3.Distance(t.Position, pulseDefenceSelectedLandmark.Location.Position);
            }
        }

        if (tile != null)
        {
            pulseDefenceSelectedLandmark.Location = tile;
            pulseDefenceSelectedLandmark.transform.position = tile.Position;
        }
    }

    //Activate the building target at the locatable's location
    private void ActivateTarget(Locatable l)
    {
        if (buildMenuCanvasGroup.alpha == 0)
        {
            lerpTargetLock = true;
            GetLocationOf(l);

            if (targetTile.Building != null && stage != TutorialStage.BuildHarvesters && stage != TutorialStage.BuildExtenderInFog)
            {
                l = GetBackupTarget(l);
            }

            buildingTarget.Location = targetTile;
            buildingTarget.transform.position = l.transform.position;
            targetRenderer.enabled = true;

            tileTargetLerpProgress = 0f;
            tileTargetLerpForward = true;

            arrowToTarget.On = true;

            ActivateMouse();
        }        
    }

    //Get location of a locatable object
    private void GetLocationOf(Locatable l)
    {
        if (l != null)
        {
            targetTile = l.Location;
        }
        else
        {
            Debug.Log("Locatable l in TutorialController.GetLocationOf(Locatable l) is null");
        }

        if (targetTile == null)
        {
            Debug.Log("TutorialController.CurrentTile is null");
        }
    }

    //Compensates for if the player speedruns and builds on the tile the target is trying to be activated on
    Locatable GetBackupTarget(Locatable l)
    {
        List<TileData> alternatives = new List<TileData>();
        List<TileData> invalidTiles = new List<TileData>();

        foreach (Locatable t in lerpTargetsRemaining)
        {
            invalidTiles.Add(t.Location);
        }

        foreach (TileData t in targetTile.AllAdjacentTiles)
        {
            if (!invalidTiles.Contains(t) && t.Building == null && t.Resource == null && t.PowerSource != null && !t.FogUnitActive && !t.buildingChecks.obstacle)
            {
                alternatives.Add(t);
            }
        }

        if (alternatives.Count == 0)
        {
            foreach (TileData t in WorldController.Instance.ActiveTiles)
            {
                if (!invalidTiles.Contains(t) && t.Building == null && t.Resource == null && t.PowerSource != null && !t.FogUnitActive && !t.buildingChecks.obstacle)
                {
                    if (alternatives.Count == 0)
                    {
                        alternatives.Add(t);
                    }
                    else
                    {
                        float dist = Vector3.Distance(targetTile.Position, t.Position);
                        float bestDist = Vector3.Distance(targetTile.Position, alternatives[0].Position);

                        if (dist < bestDist)
                        {
                            alternatives.Clear();
                            alternatives.Add(t);
                        }
                        else if (dist == bestDist)
                        {
                            alternatives.Add(t);
                        }
                    }
                }
            }
        }

        if (alternatives.Count > 0)
        {
            targetTile = alternatives[UnityEngine.Random.Range(0, alternatives.Count)];
            l.Location = targetTile;
            l.transform.position = targetTile.Position;
        }

        return l;
    }

    //Lerp the target decal
    private void LerpDecal()
    {
        float lerped;

        if (stage != TutorialStage.ActivateSonar)
        {
            lerped = Mathf.Lerp(decalMinLerp, decalMaxLerp, tileTargetLerpProgress);
        }
        else
        {
            lerped = Mathf.Lerp(decalMinLerp * largeLerpMultiplier, decalMaxLerp * largeLerpMultiplier, tileTargetLerpProgress);
        }

        buildingTarget.transform.localScale = new Vector3(lerped, 1, lerped);

        UpdateTileTargetLerpValues();
    }

    //Update lerp progress
    private void UpdateTileTargetLerpValues()
    {
        if (tileTargetLerpForward)
        {
            tileTargetLerpProgress += Time.deltaTime * lerpMultiplier;
        }
        else
        {
            tileTargetLerpProgress -= Time.deltaTime * lerpMultiplier;
        }

        if (tileTargetLerpProgress > 1)
        {
            tileTargetLerpProgress = 1;
            tileTargetLerpForward = false;
        }
        else if (tileTargetLerpProgress < 0)
        {
            tileTargetLerpProgress = 0;
            tileTargetLerpForward = true;
        }
    }

    //Check if the building currently being built at a specific location has been built
    private bool BuiltCurrentlyBuilding()
    {
        return targetTile != null && targetTile.Building != null && targetTile.Building.BuildingType == currentlyBuilding;
    }

    //Deactivates the building target
    private void DeactivateTarget()
    {
        targetRenderer.enabled = false;
        arrowToTarget.On = false;
    }

    //Tutorial Utility Methods - UI Colour-Lerp Target----------------------------------------------------------------------------------------------

    //Activates the UI lerp target
    private void ActivateUIColourLerpTarget(Image button, Color minLerpColour, Color maxLerpColour)
    {
        lerpUIColourTarget = true;
        uiColourLerpTarget = button;
        uiColourLerpTarget.gameObject.SetActive(true);

        uiTargetLerpForward = true;
        uiTargetLerpProgress = 0;

        this.minLerpColour = minLerpColour;
        this.maxLerpColour = maxLerpColour;
        uiColourLerpTarget.color = minLerpColour;
    }

    //Lerps the UI lerp target
    private void LerpUIColourTarget()
    {
        uiColourLerpTarget.color = Color.Lerp(minLerpColour, maxLerpColour, uiTargetLerpProgress);
        UpdateUITargetLerpValues();
    }

    //Called by other functionality to update the colour of the lerp target on mouse enter and pause lerping
    public void PauseColourLerp(Image calledBy)
    {
        if (lerpUIColourTarget && uiColourLerpTarget == calledBy)
        {
            uiColourLerpTarget.color = maxLerpColour;
            lerpPaused = true;
        }
    }

    //Called by other functionality to update the colour of the lerp target on mouse exit and unpause lerping
    public void UnpauseColourLerp(Image calledBy)
    {
        if (lerpUIColourTarget && uiColourLerpTarget == calledBy)
        {
            lerpPaused = false;
            uiTargetLerpProgress = 1;
            uiTargetLerpForward = false;
            uiColourLerpTarget.gameObject.SetActive(true);
        }
    }

    //Deactivates the UI lerp target
    private void DeactivateUIColourLerpTarget()
    {
        lerpUIColourTarget = false;
        lerpPaused = false;
        uiTargetLerpForward = true;
        uiTargetLerpProgress = 0;

        minLerpColour = Color.clear;
        maxLerpColour = Color.clear;
        uiLerpTarget.color = Color.clear;

        uiColourLerpTarget.gameObject.SetActive(false);
        uiColourLerpTarget = null;
    }

    //Tutorial Utility Methods - UI Scaling-Lerp Target----------------------------------------------------------------------------------------------

    //Activates the UI lerp target, also assigning mouse enter and mouse exit colours
    private void ActivateUIScalingLerpTarget(Image newUILerpTarget, float minLerp, Color mouseEnterColour, Color mouseExitColour)
    {
        maxLerpColour = mouseEnterColour;
        minLerpColour = mouseExitColour;
        ActivateUIScalingLerpTarget(newUILerpTarget, minLerp, mouseExitColour);
    }

    //Activates the UI lerp target
    private void ActivateUIScalingLerpTarget(Image newUILerpTarget, float minLerp, Color colour)
    {
        lerpUIScalingTarget = true;
        uiLerpTarget.transform.position = newUILerpTarget.transform.position;
        uiTargetLerpForward = true;
        uiTargetLerpProgress = 0;
        uiMinLerp = minLerp;
        currentUILerpFocus = newUILerpTarget;
        UpdateUIScalingLerpTargetColour(colour);
    }

    //Updates the colour of the ui lerp target
    private void UpdateUIScalingLerpTargetColour(Color colour)
    {
        uiLerpTarget.color = colour;

        if (multipleLerpRingsForBattery && stage == TutorialStage.MouseOverPowerDiagram)
        {
            foreach (Image i in uiLerpTarget.GetComponentsInChildren<Image>())
            {
                i.color = colour;
            }
        }
    }

    //Lerps the UI lerp target
    private void LerpUIScalingTarget()
    {
        float lerp = Mathf.Lerp(uiMinLerp, uiMinLerp * 1.5f, uiTargetLerpProgress);
        uiLerpTarget.transform.localScale = new Vector3(lerp, lerp, uiLerpTarget.transform.localScale.z);

        if (uiLerpTarget.transform.position != currentUILerpFocus.transform.position)
        {
            uiLerpTarget.transform.position = currentUILerpFocus.transform.position;
        }

        if (stage == TutorialStage.MouseOverPowerDiagram)
        {
            SynchroniseToBatteryColour();
        }

        UpdateUITargetLerpValues();
    }

    //Called to update the battery colour according to the current amount of power stored
    private void SynchroniseToBatteryColour()
    {
        float power = UIController.instance.CurrentPowerValDisplayed;

        if (power == 0)
        {
            if (uiLerpTarget.color != batteryEmptyColour)
            {
                UpdateUIScalingLerpTargetColour(batteryEmptyColour);
            }
        }
        else if (power <= 25)
        {
            if (uiLerpTarget.color != batteryLowColour)
            {
                UpdateUIScalingLerpTargetColour(batteryLowColour);
            }
        }
        else if (power <= 50)
        {
            if (uiLerpTarget.color != batteryHalfColour)
            {
                UpdateUIScalingLerpTargetColour(batteryHalfColour);
            }
        }
        else if (power <= 75)
        {
            if (uiLerpTarget.color != batteryHighColour)
            {
                UpdateUIScalingLerpTargetColour(batteryHighColour);
            }
        }
        else if (power > 75)
        {
            if (uiLerpTarget.color != batteryFullColour)
            {
                UpdateUIScalingLerpTargetColour(batteryFullColour);
            }
        }
    }

    //Deactivates the UI lerp target
    private void DeactivateUIScalingLerpTarget()
    {
        lerpUIScalingTarget = false;
        uiLerpTarget.transform.localScale = new Vector3(uiMinLerp, uiMinLerp, uiLerpTarget.transform.localScale.z);
        UpdateUIScalingLerpTargetColour(Color.clear);

        uiMinLerp = 1;
        //uiMaxLerp = 1;
    }

    //Tutorial Utility Methods - UI Lerp Value Updating----------------------------------------------------------------------------------------------

    //Update UI lerp progress
    private void UpdateUITargetLerpValues()
    {
        if (uiTargetLerpForward)
        {
            uiTargetLerpProgress += Time.deltaTime * lerpMultiplier;
        }
        else
        {
            uiTargetLerpProgress -= Time.deltaTime * lerpMultiplier;
        }

        if (uiTargetLerpProgress > 1)
        {
            uiTargetLerpProgress = 1;
            uiTargetLerpForward = false;
        }
        else if (uiTargetLerpProgress < 0)
        {
            uiTargetLerpProgress = 0;
            uiTargetLerpForward = true;
        }
    }
}