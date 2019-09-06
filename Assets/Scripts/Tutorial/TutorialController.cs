using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TutorialStage
{
    None,
    ExplainSituation,
    CameraControls,
    ExplainMinerals,
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
    Destroy
}

public class TutorialController : DialogueBoxController
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Skip Tutorial")]
    [SerializeField] private bool skipTutorial = true;

    [Header("Tutorial Game State")]
    [SerializeField] private TutorialStage stage = TutorialStage.ExplainSituation;
    [SerializeField] private int subStage = 1;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;
    [SerializeField] private ButtonType currentlyLerping;
    [SerializeField] private TileData currentTile = null;
    [SerializeField] private TileData lastTileChecked;
    [SerializeField] private GameObject objectiveCompletePrefab;
    
    [Header("Goals")]
    [SerializeField] private int builtHarvestersGoal;
    [SerializeField] private int builtHarvestersExtendedGoal;
    [SerializeField] private int builtGeneratorsGoal;
    [SerializeField] private int collectedMineralsGoal;

    [Header("Cameras")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CinemachineVirtualCamera mineralDepositCamera;
    [SerializeField] private CinemachineVirtualCamera sonarCamera;
    [SerializeField] private CinemachineVirtualCamera artilleryCamera;
    [SerializeField] private CinemachineVirtualCamera thrusterCamera;

    [Header("Game Objects")]
    [SerializeField] private Hub hub;
    [SerializeField] public GameObject thruster;
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark extenderLandmark;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark sonarLandmark;
    [SerializeField] private Landmark fogExtenderLandmark;
    [SerializeField] private Landmark mortarLandmark;
    [SerializeField] private Landmark pulseDefenceLandmark;
    [SerializeField] private Locatable buildingTarget;
    [SerializeField] private GameObject arrowToTargetPrefab;

    [Header("UI Elements")]
    [SerializeField] private CameraInput wKey;
    [SerializeField] private CameraInput aKey;
    [SerializeField] private CameraInput sKey;
    [SerializeField] private CameraInput dKey;
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

    private int originalGeneratorLimit;
    private int extendersGoal;
    private bool defencesOn = false;
    private int mortarsGoal;
    private int pulseDefencesGoal;

    private TutorialStage savedTutorialStage;
    private int savedSubStage;

    private TileData sonarLandmarkTile;
    private DamageIndicator arrowToTarget = null;

    private MusicFMOD musicFMOD;

    private bool lerpUIScalingTarget = false;
    private Image currentUILerpFocus;
    private float uiMinLerp;
    //private float uiMaxLerp;
    
    private bool lerpUIColourTarget = false;
    private Image uiColourLerpTarget = null;
    private Color minLerpColour;
    private Color maxLerpColour;
    
    private float uiTargetLerpProgress = 0f;
    private bool uiTargetLerpForward = true;
    private bool lerpPaused = false;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public static TutorialController Instance { get; protected set; }
    public int BuiltGeneratorsGoal { get => builtGeneratorsGoal; }
    public int BuiltHarvestersExtendedGoal { get => builtHarvestersExtendedGoal; }
    public int CollectedMineralsGoal { get => collectedMineralsGoal; }
    public TileData CurrentTile { get => currentTile; }
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
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more tutorial managers.");
        }

        Instance = this;

        if (GlobalVars.LoadedFromMenu)
        {
            skipTutorial = GlobalVars.SkipTut;
        }

        //Setup music
        if (GameObject.Find("MusicFMOD") != null)
        {
            musicFMOD = GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>();
        }

        tutProgressSlider.maxValue = 12;
    }

    //Method called by WorldController to set up the tutorial's stuff; also organises the setup of the fog
    public void StartTutorial()
    {
        //Setup fog
        Fog.Instance.enabled = true;
        Fog.Instance.SpawnStartingFog();

        if (skipTutorial)
        {
            Fog.Instance.WakeUpFog(5);
            Fog.Instance.BeginUpdatingDamage(5);
            stage = TutorialStage.Finished;
            tutProgressSlider.gameObject.SetActive(false);
            ObjectiveController.Instance.IncrementStage();
            defencesOn = true;
            wKey.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            sonarLandmarkTile = WorldController.Instance.GetTileAt(sonarLandmark.transform.position);
            targetRenderer = buildingTarget.GetComponent<MeshRenderer>();
            originalGeneratorLimit = ObjectiveController.Instance.GeneratorLimit;
            ObjectiveController.Instance.GeneratorLimit = builtGeneratorsGoal;

            if (builtGeneratorsGoal > originalGeneratorLimit)
            {
                Debug.Log("Warning: TutorialController.builtGeneratorsGoal > originalGeneratorLimit. Shouldn't it be <=?");
            }
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
            //case TutorialStage.ExplainMinerals:
            //    ExplainMinerals();
            //    break;
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
                    stage = TutorialStage.CameraControls;
                    ResetSubStage();
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
                UIController.instance.UpdateObjectiveText(stage);
                SendDialogue("move camera", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                wKey.LerpIn();
                aKey.LerpIn();
                sKey.LerpIn();
                dKey.LerpIn();
                break;
            case 2:
                GetCameraInputs();

                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (wKey.Finished && aKey.Finished && sKey.Finished && dKey.Finished)
                {
                    GoToSubStage(4);
                }

                break;
            case 3:
                GetCameraInputs();

                if (wKey.Finished && aKey.Finished && sKey.Finished && dKey.Finished)
                {
                    IncrementSubStage();
                }

                break;
            case 4:
                SendDialogue("zoom camera", 1);
                zoomIn.LerpIn();
                zoomOut.LerpIn();
                break;
            case 5:
                GetCameraInputs();

                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (zoomIn.Finished && zoomOut.Finished)
                {
                    GoToSubStage(7);
                }

                break;
            case 6:
                GetCameraInputs();

                if (zoomIn.Finished && zoomOut.Finished)
                {
                    IncrementSubStage();
                }

                break;
            case 7:
                stage = TutorialStage.BuildHarvesters;
                currentlyBuilding = BuildingType.Harvester;
                ResetSubStage();
                tutProgressSlider.value++;
                wKey.transform.parent.gameObject.SetActive(false);

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Inaccurate sub stage");
                break;
        }
    }

    //Nexy pans to a mineral deposit and explains it to the player
    //private void ExplainMinerals()
    //{
    //    switch (subStage)
    //    {
    //        case 1:
    //            cameraController.MovementEnabled = false;
    //            SendDialogue("explain minerals", 1);
    //            mineralDepositCamera.gameObject.SetActive(true);
    //            break;
    //        case 2:
    //            if (dialogueRead)
    //            {
    //                mineralDepositCamera.gameObject.SetActive(false);
    //                DismissDialogue();
    //            }

    //            break;
    //        case 3:
    //            stage = TutorialStage.BuildHarvesters;
    //            currentlyBuilding = BuildingType.Harvester;
    //            cameraController.MovementEnabled = true;
    //            ResetSubStage();
    //            tutProgressSlider.value++;
    //            break;
    //    }
    //}

    //Player learns about and builds a harvester
    private void BuildHarvesters()
    {
        switch (subStage)
        {
            case 1:
                SendDialogue("build harvester target", 1);

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

                break;
            case 3:
                if (dialogueRead)
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
                else if (BuiltCurrentlyBuilding())
                {
                    GoToSubStage(8);
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(3);
                    ActivateTarget(harvesterResource);
                }

                break;
            case 7:
                if (BuiltCurrentlyBuilding())
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
                stage = TutorialStage.BuildExtender;
                currentlyBuilding = BuildingType.Extender;
                ResetSubStage();
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
                SendDialogue("build extender target", 1);
                UIController.instance.UpdateObjectiveText(stage);

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

                break;
            case 3:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 4:
                DeactivateTarget();
                ActivateUIColourLerpTarget(extenderHighlight, minPowerBuildingColour, maxPowerBuildingColour);
                currentlyLerping = ButtonType.Extender;
                IncrementSubStage();
                break;
            case 5:
                if (BuiltCurrentlyBuilding())
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    GoToSubStage(3);
                    DeactivateUIColourLerpTarget();
                    ActivateTarget(extenderLandmark);
                }

                break;
            case 6:
                //Turn off UI element prompting player to build a relay on the prompted tile
                stage = TutorialStage.BuildHarvestersExtended;
                currentlyBuilding = BuildingType.Harvester;
                ResetSubStage();
                DeactivateTarget();
                DeactivateUIColourLerpTarget();
                Destroy(extenderHighlight);
                extenderHighlight = null;
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
                currentlyLerping = ButtonType.Harvester;
                IncrementSubStage();
                break;
            case 5:
                if (ResourceController.Instance.Harvesters.Count == builtHarvestersExtendedGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                stage = TutorialStage.WaitingForPowerDrop;
                currentlyBuilding = BuildingType.Generator;
                ResetSubStage();
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
                if (ResourceController.Instance.StoredPower < 75)
                {
                    stage = TutorialStage.MouseOverPowerDiagram;
                    UIController.instance.UpdateObjectiveText(stage);
                    SendDialogue("explain power", 1);
                    ActivateUIScalingLerpTarget(batteryIcon, batteryIconMinLerp, Color.clear);

                    if (!objWindowVisible)
                    {
                        ToggleObjWindow();
                    }
                }

                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (powerDiagram.fillAmount == 1)
                {
                    GoToSubStage(4);
                }

                break;
            case 3:
                if (powerDiagram.fillAmount == 1)
                {
                    IncrementSubStage();
                }

                break;
            case 4:
                DeactivateUIScalingLerpTarget();
                stage = TutorialStage.BuildGenerator;
                UIController.instance.UpdateObjectiveText(stage);
                SendDialogue("build generator target", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 5:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ActivateTarget(generatorLandmark);
                }

                break;
            case 6:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    GoToSubStage(8);
                }

                break;
            case 7:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 8:
                DeactivateTarget();
                ActivateUIColourLerpTarget(generatorHighlight, minPowerBuildingColour, maxPowerBuildingColour);
                currentlyLerping = ButtonType.Generator;
                IncrementSubStage();
                break;
            case 9:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (BuiltCurrentlyBuilding())
                {
                    GoToSubStage(11);
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(6);
                    ActivateTarget(generatorLandmark);
                }

                break;
            case 10:
                if (BuiltCurrentlyBuilding())
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(6);
                    ActivateTarget(generatorLandmark);
                }

                break;
            case 11:
                DeactivateUIColourLerpTarget();
                Destroy(generatorHighlight);
                generatorHighlight = null;
                SendDialogue("build more generators", 1);
                ActivateMouse();
                stage = TutorialStage.BuildMoreGenerators;
                UIController.instance.UpdateObjectiveText(stage);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 12:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    GoToSubStage(14);
                }

                break;
            case 13:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 14:
                currentlyLerping = ButtonType.Generator;
                IncrementSubStage();
                break;
            case 15:
                if (ResourceController.Instance.Generators.Count == builtGeneratorsGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 16:
                ObjectiveController.Instance.GeneratorLimit = originalGeneratorLimit;
                stage = TutorialStage.CollectMinerals;
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                ResetSubStage();
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
                // Update objective window with 0-500 mineral gauge, and button for fix hull when gauge filled
                if (ResourceController.Instance.StoredMineral >= collectedMineralsGoal)
                {
                    GoToSubStage(4);
                }
                else if (dialogueRead)
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
                StartCoroutine(CompleteTutorialObjective("You repaired damage to your ship!"));
                IncrementSubStage();
                break;
            case 7:
                break;
            case 8:
                stage = TutorialStage.CollectSonar;
                currentlyBuilding = BuildingType.None;
                ResetSubStage();
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
                UIController.instance.UpdateObjectiveText(stage);
                sonarCamera.gameObject.SetActive(true);
                SendDialogue("collect sonar", 1);
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
                    abilityUnlockCanvas.SetActive(false);
                    DismissDialogue();
                }
                else if (abilitySelectorRadialMenu.Radius > 0)
                {
                    abilityUnlockCanvas.SetActive(false);
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
                cameraController.MovementEnabled = false;

                // Update Hub model to fixed ship without thrusters / Particle effects
                hub.transform.GetChild(0).gameObject.SetActive(false);
                hub.transform.GetChild(1).gameObject.SetActive(true);

                //Enable thruster to be clicked and collected for attaching
                thruster.SetActive(true);

                artilleryCamera.gameObject.SetActive(true);
                stage = TutorialStage.SonarActivated;
                SendDialogue("explain abilities", 1);
                break;
            //case 14:
            //    if (dialogueRead)
            //    {
            //        DismissDialogue();
            //    }

            //    break;
            //case 15:
            //    SendDialogue("explain thruster", 1);
            //    artilleryCamera.gameObject.SetActive(false);
            //    thrusterCamera.gameObject.SetActive(true);
            //    break;
            case 14:
                if (dialogueRead)
                {
                    DismissDialogue();
                    //thrusterCamera.gameObject.SetActive(false);
                    artilleryCamera.gameObject.SetActive(false);
                    cameraController.MovementEnabled = true;
                    stage = TutorialStage.BuildExtenderInFog;
                    currentlyBuilding = BuildingType.Extender;
                    ResetSubStage();
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
                UIController.instance.UpdateObjectiveText(stage);
                SendDialogue("build extender in fog", 1);
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
                    ActivateTarget(fogExtenderLandmark);
                }

                break;
            case 3:
                if (tileClicked)
                {
                    DeactivateTarget();
                    DismissMouse();
                    currentlyLerping = ButtonType.Extender;
                }

                break;
            case 4:
                if (BuiltCurrentlyBuilding())
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    GoToSubStage(3);
                    ActivateTarget(fogExtenderLandmark);
                }

                break;

            case 5:
                foreach (Relay e in ResourceController.Instance.Extenders)
                {
                    if (e.TakingDamage)
                    {
                        IncrementSubStage();
                        break;
                    }
                }

                break;
            case 6:
                stage = TutorialStage.BuildMortar;
                currentlyBuilding = BuildingType.AirCannon;
                currentlyLerping = ButtonType.None;
                ResetSubStage();
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
                UIController.instance.UpdateObjectiveText(stage);
                SendDialogue("build mortar", 1);
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
                    ActivateTarget(mortarLandmark);
                }

                break;
            case 3:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 4:
                DeactivateTarget();
                ActivateUIColourLerpTarget(mortarHighlight, minDefencesColour, maxDefencesColour);
                currentlyLerping = ButtonType.AirCannon;
                IncrementSubStage();
                break;
            case 5:
                if (Hub.Instance.GetMortars().Count == 1)
                {
                    IncrementSubStage();
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(3);
                    ActivateTarget(mortarLandmark);
                }

                break;
            case 6:
                stage = TutorialStage.BuildPulseDefence;
                currentlyBuilding = BuildingType.FogRepeller;
                ResetSubStage();
                DeactivateTarget();
                DeactivateUIColourLerpTarget();
                Destroy(mortarHighlight);
                mortarHighlight = null;
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
                UIController.instance.UpdateObjectiveText(stage);
                SendDialogue("build pulse defence", 1);
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
                    ActivateTarget(pulseDefenceLandmark);
                }

                break;
            case 3:
                if (tileClicked && lastTileChecked == pulseDefenceLandmark.Location)
                {
                    DismissMouse();
                }
                else
                {
                    tileClicked = false;
                }

                break;
            case 4:
                DeactivateTarget();
                ActivateUIColourLerpTarget(pulseDefenceHighlight, minDefencesColour, maxDefencesColour);
                currentlyLerping = ButtonType.FogRepeller;
                IncrementSubStage();
                break;
            case 5:
                if (Hub.Instance.GetPulseDefences().Count == 1)
                {
                    stage = TutorialStage.DefenceActivation;
                    currentlyBuilding = BuildingType.None;
                    ResetSubStage();
                    DeactivateUIColourLerpTarget();
                    Destroy(pulseDefenceHighlight);
                    pulseDefenceHighlight = null;
                    tutProgressSlider.value++;
                }
                else if (buildMenuCanvasGroup.alpha == 0)
                {
                    DeactivateUIColourLerpTarget();
                    GoToSubStage(3);
                    ActivateTarget(pulseDefenceLandmark);
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
                    UIController.instance.ShowActivateButton();
                }

                UIController.instance.UpdateObjectiveText(stage);
                SendDialogue("activate defences", 1);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 3:
                //Waiting for the big button to be pressed
                break;
            case 4:
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
                        Fog.Instance.WakeUpFog();
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
                SendDialogue("finished", 1);
                stage = TutorialStage.Finished;
                tutProgressSlider.gameObject.SetActive(false);
                ResetSubStage();
                ObjectiveController.Instance.IncrementStage();
                //FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-Win", GetComponent<Transform>().position);
                musicFMOD.StageTwoMusic();
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

    //Tutorial Utility Methods - Camera--------------------------------------------------------------------------------------------------------------

    //Checks WASD inputs for camera movement part of the tutorial
    private void GetCameraInputs()
    {
        if (subStage < 4)
        {
            GetCameraInput("Vertical", sKey, wKey);
            GetCameraInput("Horizontal", aKey, dKey);
        }
        else
        {
            GetCameraInput("Zoom", zoomOut, zoomIn);
        }
    }

    //Checks a specific camera input's input
    private void GetCameraInput(string input, CameraInput negativeInput, CameraInput positiveInput)
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

    //Tutorial Utility Methods - Checking if X is allowed--------------------------------------------------------------------------------------------

    //Checking if a tile is acceptable
    public bool TileAllowed(TileData tile)
    {
        lastTileChecked = tile;

        if (!cameraController.FinishedOpeningCameraPan || (stage <= TutorialStage.CollectSonar && tile == sonarLandmarkTile))
        {
            return false;
        }

        switch (stage)
        {
            case TutorialStage.BuildHarvesters:
                if (subStage > 6)
                {
                    return tile.Resource != null;
                }
                else
                {
                    return tile == currentTile;
                }
            case TutorialStage.BuildHarvestersExtended:
                return tile.Resource != null;
            case TutorialStage.BuildMoreGenerators:
                return tile.Resource == null;
            case TutorialStage.CollectSonar:
                return tile.Resource == null && !tile.FogUnitActive;
            case TutorialStage.BuildExtenderInFog:
                return tile == currentTile || (tile.Resource == null && !tile.FogUnitActive);
            case TutorialStage.CollectMinerals:
            case TutorialStage.BuildPulseDefence:
            case TutorialStage.DefenceActivation:
            case TutorialStage.BuildDefencesInRange:
                bool tileOkay = !tile.FogUnitActive || tile.Building != null;

                if (!tileOkay && !aiText.Activated)
                {
                    savedTutorialStage = stage;
                    savedSubStage = subStage;

                    stage = TutorialStage.DontBuildInFog;
                    subStage = 1;
                }

                if (tileOkay && stage == TutorialStage.BuildPulseDefence)
                {
                    return tile.Resource == null;
                }

                return tileOkay;
            case TutorialStage.Finished:
                return true;
            default:
                return tile == currentTile;
        }
    }

    //Checking if a button is acceptable
    public bool ButtonAllowed(ButtonType button)
    {
        if (ButtonsNormallyAllowed(lastTileChecked).Contains(button))
        {
            switch (stage)
            {
                case TutorialStage.CollectMinerals:
                    return button == ButtonType.Extender 
                           || button == ButtonType.Harvester 
                           || button == ButtonType.Generator 
                           || button == ButtonType.Destroy;
                case TutorialStage.CollectSonar:
                    return button == ButtonType.Extender
                           || button == ButtonType.Destroy;
                case TutorialStage.BuildPulseDefence:
                    return (button == ButtonType.FogRepeller && lastTileChecked == pulseDefenceLandmark.Location) 
                           || (button == ButtonType.Extender && lastTileChecked != pulseDefenceLandmark.Location) 
                           || button == ButtonType.Destroy;
                case TutorialStage.DefenceActivation:
                case TutorialStage.BuildDefencesInRange:
                case TutorialStage.Finished:
                    return true;
                default:
                    return button == currentlyLerping || button == ButtonType.Destroy;
            }
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

            result.Add(ButtonType.AirCannon);
            result.Add(ButtonType.Extender);
            result.Add(ButtonType.FogRepeller);
        }

        result.Add(ButtonType.Destroy);
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
        GameObject objComp = Instantiate(objectiveCompletePrefab, GameObject.Find("Canvas").transform);
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
        GoToSubStage(4);
    }

    //Tutorial Utility Methods - (Targeted) Building-------------------------------------------------------------------------------------------------

    //Activate the building target at the locatable's location
    private void ActivateTarget(Locatable l)
    {
        GetLocationOf(l);

        buildingTarget.Location = currentTile;
        buildingTarget.transform.position = l.transform.position;
        targetRenderer.enabled = true;

        tileTargetLerpProgress = 0f;
        tileTargetLerpForward = true;

        if (arrowToTarget == null)
        {
            arrowToTarget = Instantiate(arrowToTargetPrefab, GameObject.Find("Warnings").transform).GetComponent<DamageIndicator>();
            arrowToTarget.Colour = Color.cyan;
            arrowToTarget.Locatable = buildingTarget;
        }
        else
        {
            arrowToTarget.On = true;
        }


        ActivateMouse();
    }

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

    //Lerp the target decal
    private void LerpDecal()
    {
        float lerped = Mathf.Lerp(decalMinLerp, decalMaxLerp, tileTargetLerpProgress);
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
        return currentTile != null && currentTile.Building != null && currentTile.Building.BuildingType == currentlyBuilding;
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

