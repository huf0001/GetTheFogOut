using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum TutorialStage
{
    None,
    ExplainSituation,
    ExplainMinerals,
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
    Destroy
}

public class TutorialController : DialogueBoxController
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Tutorial UI Elements")]
    [SerializeField] private CameraInput wKey;
    [SerializeField] private CameraInput aKey;
    [SerializeField] private CameraInput sKey;
    [SerializeField] private CameraInput dKey;
    [SerializeField] private CameraInput zoomIn;
    [SerializeField] private CameraInput zoomOut;
    [SerializeField] private Image powerDiagram;
    [SerializeField] private GameObject abilityUnlockCanvas;

    [Header("Skip Tutorial")]
    [SerializeField] private bool skipTutorial = true;

    [Header("Tutorial Game State (Testing)")]
    [SerializeField] private TutorialStage stage = TutorialStage.ExplainSituation;
    [SerializeField] private int subStage = 1;
    [SerializeField] private BuildingType currentlyBuilding = BuildingType.None;
    [SerializeField] private ButtonType currentlyLerping;
    [SerializeField] private TileData currentTile = null;
    [SerializeField] private TileData lastTileChecked;

    [Header("Objects on Game Board")]
    [SerializeField] private Hub hub;
    [SerializeField] public GameObject thruster;
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark extenderLandmark;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark sonarLandmark;
    [SerializeField] private Locatable buildingTarget;

    [Header("Cameras")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CinemachineVirtualCamera mineralDepositCamera;
    [SerializeField] private CinemachineVirtualCamera sonarCamera;
    [SerializeField] private CinemachineVirtualCamera artilleryCamera;
    [SerializeField] private CinemachineVirtualCamera thrusterCamera;

    [Header("UI Lerp Colours")]
    [SerializeField] private Color uiNormalColour;
    [SerializeField] private Color uiHighlightColour;

    [Header("Goals")]
    [SerializeField] private int builtHarvestersGoal;
    [SerializeField] private int builtHarvestersExtendedGoal;
    [SerializeField] private int builtGeneratorsGoal;
    [SerializeField] private int collectedMineralsGoal;

    //[Header("Prefabs")]
    //[SerializeField] private GameObject pulseDefencePrefab;

    //Non-Serialized Fields
    private MeshRenderer targetRenderer = null;
    private float decalMin = 1.5f;
    private float decalMax = 3f;
    private float lerpMultiplier = 1f;
    private float lerpProgress = 0f;
    private bool lerpForward = true;

    private int extendersGoal;
    private bool defencesOn = false;
    private int mortarsGoal;
    private int pulseDefencesGoal;

    private TutorialStage savedTutorialStage;
    private int savedSubStage;

    private TileData sonarLandmarkTile;

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
    public Color UIHighlightColour { get => uiHighlightColour; }
    public Color UINormalColour { get => uiNormalColour; }

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
    }

    //Method called by WorldController to set up the tutorial's stuff; also organises the setup of the fog
    public void StartTutorial()
    {
        //Setup music
        WorldController.Instance.musicFMOD.StageOneMusic();

        //Setup fog
        Fog.Instance.enabled = true;
        Fog.Instance.SpawnStartingFog();

        if (skipTutorial)
        {
            Fog.Instance.InvokeWakeUpFog(5);
            Fog.Instance.InvokeBeginUpdatingDamage(5);
            stage = TutorialStage.Finished;
            ObjectiveController.Instance.IncrementStage();
            defencesOn = true;
            wKey.transform.parent.gameObject.SetActive(false);
            //pulseDefencePrefab.GetComponentInChildren<Animator>().enabled = true;
        }
        else
        {
            sonarLandmarkTile = WorldController.Instance.GetTileAt(sonarLandmark.transform.position);
            targetRenderer = buildingTarget.GetComponent<MeshRenderer>();
            //pulseDefencePrefab.GetComponentInChildren<Animator>().enabled = false;
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
            case TutorialStage.ExplainMinerals:
                ExplainMinerals();
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
                SendDialogue("explain situation", 2);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    stage = TutorialStage.ExplainMinerals;
                    ResetSubStage();
                }

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Inaccurate sub stage");
                break;
        }
    }

    //Nexy pans to a mineral deposit and explains it to the player
    private void ExplainMinerals()
    {
        switch (subStage)
        {
            case 1:
                SendDialogue("explain minerals", 1);
                mineralDepositCamera.gameObject.SetActive(true);
                break;
            case 2:
                if (dialogueRead)
                {
                    mineralDepositCamera.gameObject.SetActive(false);
                    DismissDialogue();
                }

                break;
            case 3:
                stage = TutorialStage.CameraControls;
                ResetSubStage();
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
                wKey.transform.parent.gameObject.SetActive(false);

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
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 4:
                currentlyLerping = ButtonType.Harvester;
                SendDialogue("build harvester icon", 1);
                break;
            case 5:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (BuiltCurrentlyBuilding())
                {
                    DeactivateTarget();
                    GoToSubStage(7);
                }

                break;
            case 6:
                if (BuiltCurrentlyBuilding())
                {
                    DeactivateTarget();
                    IncrementSubStage();
                }

                break;
            case 7:
                SendDialogue("build more harvesters", 1);
                ActivateMouse();
                break;
            case 8:
                if (dialogueRead)
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
                currentlyLerping = ButtonType.Harvester;
                IncrementSubStage();
                break;
            case 11:
                if (ResourceController.Instance.Harvesters.Count == builtHarvestersGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 12:
                stage = TutorialStage.BuildExtender;
                currentlyBuilding = BuildingType.Extender;
                ResetSubStage();
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
                currentlyLerping = ButtonType.Extender;
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
                stage = TutorialStage.BuildHarvestersExtended;
                currentlyBuilding = BuildingType.Harvester;
                ResetSubStage();
                DeactivateTarget();

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
                    GoToSubStage(4);
                }

                break;
            case 4:
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
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 7:
                currentlyLerping = ButtonType.Generator;
                IncrementSubStage();
                break;
            case 8:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (BuiltCurrentlyBuilding())
                {
                    DeactivateTarget();
                    GoToSubStage(10);
                }

                break;
            case 9:
                if (BuiltCurrentlyBuilding())
                {
                    DeactivateTarget();
                    IncrementSubStage();
                }

                break;
            case 10:
                SendDialogue("build more generators", 1);
                ActivateMouse();
                stage = TutorialStage.BuildMoreGenerators;
                UIController.instance.UpdateObjectiveText(stage);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 11:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    GoToSubStage(13);
                }

                break;
            case 12:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 13:
                currentlyLerping = ButtonType.Generator;
                IncrementSubStage();
                break;
            case 14:
                if (ResourceController.Instance.Generators.Count == builtGeneratorsGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 15:
                stage = TutorialStage.CollectMinerals;
                currentlyBuilding = BuildingType.None;
                currentlyLerping = ButtonType.None;
                ResetSubStage();
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
                stage = TutorialStage.CollectSonar;
                currentlyBuilding = BuildingType.None;
                ResetSubStage();
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
                UIController.instance.UpdateObjectiveText(stage);
                sonarCamera.gameObject.SetActive(true);
                SendDialogue("collect sonar", 1);
                break;
            case 2:
                if (AbilityController.Instance.AbilityCollected[AbilityEnum.Sonar])
                {
                    sonarCamera.gameObject.SetActive(false);
                    GoToSubStage(4);
                }
                else if (dialogueRead)
                {
                    sonarCamera.gameObject.SetActive(false);
                    DismissDialogue();
                }

                break;
            case 3:
                if (AbilityController.Instance.AbilityCollected[AbilityEnum.Sonar])
                {
                    sonarCamera.gameObject.SetActive(false);
                    IncrementSubStage();
                }

                break;
            case 4:
                stage = TutorialStage.ActivateSonar;
                SendDialogue("select sonar", 1);
                break;
            case 5:
                if (AbilityController.Instance.IsAbilitySelected)
                {
                    abilityUnlockCanvas.SetActive(false);
                    GoToSubStage(7);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                    abilityUnlockCanvas.SetActive(false);
                }

                break;
            case 6:
                if (AbilityController.Instance.IsAbilitySelected)
                {
                    IncrementSubStage();
                }

                break;
            case 7:
                SendDialogue("activate sonar", 1);
                break;
            case 8:
                if (AbilityController.Instance.AbilityTriggered[AbilityEnum.Sonar])
                {
                    GoToSubStage(10);
                }
                else if (dialogueRead)
                {
                    IncrementSubStage();
                }

                break;
            case 9:
                if (AbilityController.Instance.AbilityTriggered[AbilityEnum.Sonar])
                {
                    IncrementSubStage();

                }

                break;
            case 10:
                // Update Hub model to fixed ship without thrusters / Particle effects
                hub.transform.GetChild(0).gameObject.SetActive(false);
                hub.transform.GetChild(1).gameObject.SetActive(true);

                //Enable thruster to be clicked and collected for attaching
                thruster.SetActive(true);

                artilleryCamera.gameObject.SetActive(true);
                stage = TutorialStage.SonarActivated;
                SendDialogue("explain abilities", 1);
                break;
            case 11:
                if (dialogueRead)
                {
                    DismissDialogue();

                }

                break;
            case 12:
                SendDialogue("explain thruster", 1);
                artilleryCamera.gameObject.SetActive(false);
                thrusterCamera.gameObject.SetActive(true);
                break;
            case 13:
                if (dialogueRead)
                {
                    DismissDialogue();
                    thrusterCamera.gameObject.SetActive(false);
                    stage = TutorialStage.BuildExtenderInFog;
                    currentlyBuilding = BuildingType.Extender;
                    ResetSubStage();
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
                }
                else if (tileClicked)
                {
                    DismissMouse();
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
                currentlyLerping = ButtonType.Extender;
                IncrementSubStage();
                break;
            case 5:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else
                {
                    foreach (Relay e in ResourceController.Instance.Extenders)
                    {
                        if (e.TakingDamage)
                        {
                            GoToSubStage(7);
                            break;
                        }
                    }
                }

                break;
            case 6:
                foreach (Relay e in ResourceController.Instance.Extenders)
                {
                    if (e.TakingDamage)
                    {
                        IncrementSubStage();
                        break;
                    }
                }

                break;
            case 7:
                stage = TutorialStage.BuildMortar;
                currentlyBuilding = BuildingType.AirCannon;
                ResetSubStage();
                DeactivateTarget();
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
                currentlyLerping = ButtonType.AirCannon;
                IncrementSubStage();
                break;
            case 5:
                if (Hub.Instance.GetMortars().Count == 1)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                stage = TutorialStage.BuildPulseDefence;
                currentlyBuilding = BuildingType.FogRepeller;
                ResetSubStage();
                DeactivateTarget();
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
                currentlyLerping = ButtonType.FogRepeller;
                IncrementSubStage();
                break;
            case 5:
                if (Hub.Instance.GetPulseDefences().Count == 1)
                {
                    stage = TutorialStage.DefenceActivation;
                    currentlyBuilding = BuildingType.None;
                    ResetSubStage();
                    DeactivateTarget();
                }

                break;

            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    //Player turns the defences on, waking the fog
    //TODO: pulse defence animation needs to be off until defences are switched on
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
                //foreach (RepelFan pd in ResourceController.Instance.PulseDefences)
                //{
                //    pd.GetComponentInChildren<Animator>().enabled = true;
                //}

                //pulseDefencePrefab.GetComponentInChildren<Animator>().enabled = true;
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
                SendDialogue("finished", 1);
                break;
            case 7:
                if (dialogueRead)
                {
                    stage = TutorialStage.Finished;
                    ResetSubStage();
                    ObjectiveController.Instance.IncrementStage();
                    //MusicController.Instance.StartStage1();
                    FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-Win", GetComponent<Transform>().position);
                    WorldController.Instance.musicFMOD.StageTwoMusic();
                }

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

        if (stage <= TutorialStage.CollectSonar && tile == sonarLandmarkTile)
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
            case TutorialStage.BuildExtenderInFog:
                return tile.Resource == null;
            case TutorialStage.CollectSonar:
                return false;
            case TutorialStage.CollectMinerals:
            case TutorialStage.BuildMortar:
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
                    return button == ButtonType.Extender || button == ButtonType.Harvester;
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

        lerpProgress = 0f;
        lerpForward = true;

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
        float lerped = Mathf.Lerp(decalMin, decalMax, lerpProgress);
        buildingTarget.transform.localScale = new Vector3(lerped, 1, lerped);

        UpdateLerpValues();
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
        return currentTile != null && currentTile.Building != null && currentTile.Building.BuildingType == currentlyBuilding;
    }

    //Deactivates the building target
    private void DeactivateTarget()
    {
        targetRenderer.enabled = false;
    }
}

