using System;
using System.Collections;
using System.Collections.Generic;
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
    BuildGenerator,
    BuildMoreGenerators,
    CollectMinerals,
    ClearFog,
    Finished
}

public enum ButtonType
{
    None,
    AirCannon,
    //Battery,
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

    [Header("Objects on Game Board")]
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark extenderLandmark;
    [SerializeField] private ResourceNode extendedHarvesterResource;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark airCannonLandmark;
    [SerializeField] private Landmark fogRepellerLandmark;
    [SerializeField] private Locatable buildingTarget;

    [Header("Movement Keys")]
    [SerializeField] private CameraKey wKey;
    [SerializeField] private CameraKey aKey;
    [SerializeField] private CameraKey sKey;
    [SerializeField] private CameraKey dKey;

    [Header("Camera Panning")]
    [SerializeField] private CinemachineVirtualCamera mineralDepositCamera;

    [Header("Colours")]
    [SerializeField] private Color uiNormalColour;
    [SerializeField] private Color uiHighlightColour;

    [Header("Goals")]
    [SerializeField] private int builtHarvestersGoal;
    [SerializeField] private int builtHarvestersExtendedGoal;
    [SerializeField] private int builtGeneratorsGoal;

    //Non-Serialized Fields


    private TileData currentTile = null;
    private TileData lastTileChecked;

    private ButtonType currentlyLerping;

    private MeshRenderer targetRenderer = null;
    private float decalMin = 1.5f;
    private float decalMax = 3f;
    private float lerpMultiplier = 1f;
    private float lerpProgress = 0f;
    private bool lerpForward = true;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public static TutorialController Instance { get; protected set; }
    public int BuiltGeneratorsGoal { get => builtGeneratorsGoal; }
    public int BuiltHarvestersExtendedGoal { get => builtHarvestersExtendedGoal; }
    public TileData CurrentTile { get => currentTile; }
    public BuildingType CurrentlyBuilding { get => currentlyBuilding; }
    public ButtonType CurrentlyLerping { get => currentlyLerping; }
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
        Fog.Instance.enabled = true;
        Fog.Instance.SpawnStartingFog();

        if (skipTutorial)
        {
            Fog.Instance.InvokeWakeUpFog(5);
            tutorialStage = TutorialStage.Finished;
            ObjectiveController.Instance.IncrementStage();
            //MusicController.Instance.SkipTutorial();
            wKey.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            targetRenderer = buildingTarget.GetComponent<MeshRenderer>();
        }
    }

    //General Recurring Methods----------------------------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        CheckTutorialStage();

        if (tutorialStage != TutorialStage.Finished)
        {
            //TODO: determine which part of the final tutorial stage would best serve the conditions here.
            //if (tutorialStage == TutorialStage.BuildFogRepeller && subStage > 5)
            //{
            //    UIController.instance.UpdateObjectiveText(TutorialStage.None);
            //}
            //else
            //{
                UIController.instance.UpdateObjectiveText(tutorialStage);
            //}

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
            case TutorialStage.BuildGenerator:
            case TutorialStage.BuildMoreGenerators:
                BuildGenerator();
                break;
            case TutorialStage.CollectMinerals:
                //CollectMinerals();
                break;
            case TutorialStage.ClearFog:
                //ClearFog();
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
                    SkipTutorialAhead(4);
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
                ActivateTarget(harvesterResource);

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
                    SkipTutorialAhead(7);
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
                    SkipTutorialAhead(10);
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
                ActivateTarget(extenderLandmark);

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
                if (ResourceController.Instance.Harvesters.Count == builtHarvestersExtendedGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                tutorialStage = TutorialStage.BuildGenerator;
                currentlyBuilding = BuildingType.Generator;
                ResetSubStage();
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
                    UIController.instance.UpdateObjectiveText(tutorialStage);
                    SendDialogue("build generator target", 1);
                    ActivateTarget(generatorLandmark);

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
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (BuiltCurrentlyBuilding())
                {
                    DeactivateTarget();
                    SkipTutorialAhead(7);
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
                SendDialogue("build more generators", 1);
                ActivateMouse();
                tutorialStage = TutorialStage.BuildMoreGenerators;
                UIController.instance.UpdateObjectiveText(tutorialStage);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                break;
            case 8:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (tileClicked)
                {
                    SkipTutorialAhead(10);
                }

                break;
            case 9:
                if (tileClicked)
                {
                    DismissMouse();
                }

                break;
            case 10:
                currentlyLerping = ButtonType.Generator;
                IncrementSubStage();
                break;
            case 11:
                if (ResourceController.Instance.Generators.Count == builtGeneratorsGoal)
                {
                    IncrementSubStage();
                }

                break;
            case 12:
                tutorialStage = TutorialStage.CollectMinerals;
                currentlyBuilding = BuildingType.Extender;
                currentlyLerping = ButtonType.None;
                ResetSubStage();
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("Inaccurate sub stage");
                break;
        }
    }

    //Checks that player has collected the required minerals.
    //TODO: CollectMinerals()

    //Player clears fog away from further minerals with defences
    //TODO: ClearFog()
    //Note: incorporate BuildMortar() and BuildPulseDefence() content into ClearFog()

    //Player learns about and builds an arc defence
    private void BuildMortar()
    {
        switch (subStage)
        {
            case 1:
                Fog.Instance.WakeUpFog();
                SendDialogue("build air cannon", 1);
                //Invoke("ActivateTarget", 1);
                ActivateTarget(airCannonLandmark);

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
                //tutorialStage = TutorialStage.BuildFogRepeller;
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

    //Player learns about and builds a repel fan
    private void BuildPulseDefence()
    {
        switch (subStage)
        {
            case 1:
                SendDialogue("build fog repeller", 1);
                //Invoke("ActivateTarget", 1);
                ActivateTarget(fogRepellerLandmark);

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
                currentlyLerping = ButtonType.FogRepeller;
                IncrementSubStage();
                break;
            case 5:
                if (Hub.Instance.GetPulseDefences().Count == 1)
                {
                    IncrementSubStage();
                }

                break;
            case 6:
                currentlyBuilding = BuildingType.None;
                SendDialogue("finished", 1);
                DeactivateTarget();
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
        return tutorialStage == TutorialStage.Finished 
            || tile == currentTile 
            || (tile.Resource != null && ((tutorialStage == TutorialStage.BuildHarvesters && subStage > 6) || tutorialStage == TutorialStage.BuildHarvestersExtended))
            || (tile.Resource == null && tutorialStage == TutorialStage.BuildMoreGenerators);
    }

    //Checking if a button is acceptable
    public bool ButtonAllowed(ButtonType button)
    {
        return (tutorialStage == TutorialStage.Finished || button == currentlyLerping || button == ButtonType.Destroy) && ButtonsNormallyAllowed(lastTileChecked).Contains(button);
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

    //Dismisses dialogue and the mouse and skips the tutorial ahead appropriately
    private void SkipTutorialAhead(int nextSubStage)
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

    //Tutorial Utility Methods - (Targeted) Building-------------------------------------------------------------------------------------------------

    //(Invokably) activate the building target based on the current stage rather than passing a parameter
    //private void ActivateTarget()
    //{
    //    Locatable l = null;

    //    switch (tutorialStage)
    //    {
    //        //case TutorialStage.BuildGenerator:
    //        //    l = generatorLandmark;
    //        //    break;
    //        //case TutorialStage.BuildExtender:
    //        //    l = extenderLandmark;
    //        //    break;
    //        case TutorialStage.BuildHarvesters:
    //            l = harvesterResource;
    //            break;
    //        default:
    //            Debug.Log($"Why are you activating a target during stage {tutorialStage}?");
    //            break;
    //    }

    //    if (l != null)
    //    {
    //        ActivateTarget(l);
    //    }
    //    else
    //    {
    //        Debug.Log("Locatable l in TutorialController.ActivateTarget() is null");
    //    }

    //}

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

