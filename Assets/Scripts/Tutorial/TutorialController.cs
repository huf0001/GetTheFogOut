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
    BuildExtenderInFog,
    BuildMortar,
    BuildPulseDefence,
    DefenceActivation,
    DontBuildInFog,
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

    [Header("Tutorial Game State (Testing)")]
    [SerializeField] private TutorialStage tutorialStage = TutorialStage.ExplainSituation;
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
    [SerializeField] private ResourceNode extendedHarvesterResource;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark extenderInFogLandmark;
    [SerializeField] private Landmark mortarLandmark;
    [SerializeField] private Landmark pulseDefenceLandmark;
    [SerializeField] private Locatable buildingTarget;

    [Header("UI Elements")]
    [SerializeField] private CameraKey wKey;
    [SerializeField] private CameraKey aKey;
    [SerializeField] private CameraKey sKey;
    [SerializeField] private CameraKey dKey;
    [SerializeField] private Image powerDiagram;

    [Header("Cameras")]
    [SerializeField] private CinemachineVirtualCamera mineralDepositCamera;
    [SerializeField] private CinemachineVirtualCamera thrusterCamera;

    [Header("Colours")]
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

    private MusicFMOD musicFMOD;

    private int extendersGoal;
    private bool defencesOn = false;

    private TutorialStage savedTutorialStage;
    private int savedSubStage;

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
    public TutorialStage TutorialStage { get => tutorialStage; }
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
            tutorialStage = TutorialStage.Finished;
            ObjectiveController.Instance.IncrementStage();
            defencesOn = true;
            wKey.transform.parent.gameObject.SetActive(false);
            //pulseDefencePrefab.GetComponentInChildren<Animator>().enabled = true;
        }
        else
        {
            targetRenderer = buildingTarget.GetComponent<MeshRenderer>();
            //pulseDefencePrefab.GetComponentInChildren<Animator>().enabled = false;
        }
    }

    //General Recurring Methods----------------------------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        CheckTutorialStage();

        if (tutorialStage != TutorialStage.Finished)
        {
            if (tutorialStage == TutorialStage.DefenceActivation && subStage > 5)
            {
                UIController.instance.UpdateObjectiveText(TutorialStage.None);
            }
            else
            {
                UIController.instance.UpdateObjectiveText(tutorialStage);
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
        switch (tutorialStage)
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
            case TutorialStage.Finished:
                //End tutorial, game is fully responsive to player's input.
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Erroneous stage called.");
                break;
        }
    }

    //Stage-Specific Recurring Methods - Player Completes Tutorial-----------------------------------------------------------------------------------

    //Nexy explains the situation to the player
    private void ExplainSituation()
    {
        switch (subStage)
        {
            case 1:
                UIController.instance.UpdateObjectiveText(tutorialStage);
                SendDialogue("explain situation", 2);
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    tutorialStage = TutorialStage.ExplainMinerals;
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
                tutorialStage = TutorialStage.CameraControls;
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
                UIController.instance.UpdateObjectiveText(tutorialStage);
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
                GetCameraMovementInput();

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
                GetCameraMovementInput();

                if (wKey.Finished && aKey.Finished && sKey.Finished && dKey.Finished)
                {
                    IncrementSubStage();
                }

                break;
            case 4:
                tutorialStage = TutorialStage.BuildHarvesters;
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
                tutorialStage = TutorialStage.BuildExtender;
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
                UIController.instance.UpdateObjectiveText(tutorialStage);

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
                tutorialStage = TutorialStage.BuildHarvestersExtended;
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
                UIController.instance.UpdateObjectiveText(tutorialStage);
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
                tutorialStage = TutorialStage.WaitingForPowerDrop;
                currentlyBuilding = BuildingType.Generator;
                ResetSubStage();
                UIController.instance.UpdateObjectiveText(tutorialStage);
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
                    tutorialStage = TutorialStage.MouseOverPowerDiagram;
                    UIController.instance.UpdateObjectiveText(tutorialStage);
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
                tutorialStage = TutorialStage.BuildGenerator;
                UIController.instance.UpdateObjectiveText(tutorialStage);
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
                tutorialStage = TutorialStage.BuildMoreGenerators;
                UIController.instance.UpdateObjectiveText(tutorialStage);

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
                tutorialStage = TutorialStage.CollectMinerals;
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
                UIController.instance.UpdateObjectiveText(tutorialStage);
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
                tutorialStage = TutorialStage.BuildExtenderInFog;
                currentlyBuilding = BuildingType.Extender;
                ResetSubStage();
                break;
            default:
                break;
        }
    }

    //Player builds an extender in the fog to see what happens
    private void BuildExtenderInFog()
    {
        switch (subStage)
        {
            //TODO: pan to thruster, enable it
            case 1:
                // Update Hub model to fixed ship without thrusters / Particle effects
                hub.transform.GetChild(0).gameObject.SetActive(false);
                hub.transform.GetChild(1).gameObject.SetActive(true);

                //Enable thruster to be clicked and collected for attaching
                thruster.SetActive(true);
 
                // Play music Var 2 soundtrack
                WorldController.Instance.musicFMOD.StageTwoMusic();

                //Camera pans to the thruster
                thrusterCamera.gameObject.SetActive(true);
                
                SendDialogue("extend to thruster", 1);
                ActivateMouse();
                break;
            case 2:
                if (dialogueRead)
                {
                    DismissDialogue();
                    thrusterCamera.gameObject.SetActive(false);
                }

                break;
            case 3:
                UIController.instance.UpdateObjectiveText(tutorialStage);
                SendDialogue("build extender in fog", 1);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 4:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    GoToSubStage(6);
                }

                break;
            case 5:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 6:
                currentlyLerping = ButtonType.Extender;
                IncrementSubStage();
                break;
            case 7:
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
                            GoToSubStage(9);
                            break;
                        }
                    }
                }

                break;
            case 8:
                foreach (Relay e in ResourceController.Instance.Extenders)
                {
                    if (e.TakingDamage)
                    {
                        IncrementSubStage();
                        break;
                    }
                }

                break;
            case 9:
                tutorialStage = TutorialStage.BuildMortar;
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
                UIController.instance.UpdateObjectiveText(tutorialStage);
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
                tutorialStage = TutorialStage.BuildPulseDefence;
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
                UIController.instance.UpdateObjectiveText(tutorialStage);
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
                    tutorialStage = TutorialStage.DefenceActivation;
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

                UIController.instance.UpdateObjectiveText(tutorialStage);
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
                    tutorialStage = TutorialStage.Finished;
                    ResetSubStage();
                    ObjectiveController.Instance.IncrementStage();
                    //MusicController.Instance.StartStage1();
                }

                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

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
                    tutorialStage = savedTutorialStage;
                    subStage = savedSubStage;
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

    //Checks a specific camera input's input
    private void GetButtonInput(string input, CameraKey negativeKey, CameraKey positiveKey)
    {
        if (Input.GetAxis(input) > 0.001 && !positiveKey.LerpOutCalled)
        {
            positiveKey.LerpOut();
        }
        else if (Input.GetAxis(input) < -0.001 && !negativeKey.LerpOutCalled)
        {
            negativeKey.LerpOut();
        }
    }

    //Tutorial Utility Methods - Checking if X is allowed--------------------------------------------------------------------------------------------

    //Checking if a tile is acceptable
    public bool TileAllowed(TileData tile)
    {
        lastTileChecked = tile;

        switch (tutorialStage)
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
            case TutorialStage.CollectMinerals:
            case TutorialStage.BuildMortar:
            case TutorialStage.BuildPulseDefence:
            case TutorialStage.DefenceActivation:
                bool tileOkay = tile.FogUnit == null || tile.Building != null;

                if (!tileOkay && !aiText.Activated)
                {
                    savedTutorialStage = tutorialStage;
                    savedSubStage = subStage;

                    tutorialStage = TutorialStage.DontBuildInFog;
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
            switch (tutorialStage)
            {
                case TutorialStage.CollectMinerals:
                    return button == ButtonType.Extender || button == ButtonType.Harvester;
                case TutorialStage.DefenceActivation:
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

    //Tutorial Utility Methods - Stage Progression---------------------------------------------------------------------------------------------------

    //Tells MouseController to report clicks to TutorialController
    private void ActivateMouse()
    {
        MouseController.Instance.ReportTutorialClick = true;
    }

    //Override of SendDialogue that calls IncrementSubStage once dialogue is sent
    protected override void SendDialogue(string dialogueKey, float invokeDelay)
    {
        base.SendDialogue(dialogueKey, invokeDelay);
        IncrementSubStage();
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

