using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Profiling;

public enum ObjectiveStage
{
    None,
    HarvestMinerals,
    RecoverPart,
    Upgrades,
    SurvivalStage,
    Finished
}

public class ObjectiveController : DialogueBoxController
{
    // Fields ----------------------------------------------------------------------------------------------------------

    private bool skipTutorial = false;

    // Serialized Fields
    [Header("Stages")]
    [SerializeField] ObjectiveStage currStage = ObjectiveStage.None;
    [SerializeField] int subStage = 0;
    [SerializeField] bool objectivesOn = true;

    [Header("Progression of Objectives")]
    [SerializeField] int earlyGameGeneratorLimit;
    [SerializeField] int midGameGeneratorLimit;
    [SerializeField] int lateGameGeneratorLimit;
    [SerializeField] int mineralTarget = 300;
    [SerializeField] GameObject objectiveCompletePrefab;

    [Header("Hub")]
    [SerializeField] Hub hubScript;
    [SerializeField] private GameObject thruster;

    [Header("Cameras")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CinemachineVirtualCamera thrusterCamera;

    [Header("Upgrades Tutorial")]
    [SerializeField] int mineralsForUpgrades = 750;
    [SerializeField] private GameObject upgradesButton;
    [SerializeField] private GameObject upgradesCanvas;

    // Non-Serialized Fields -------------------------------------------------------------------------------------------

    private bool powerOverloaded = false;
    private bool alertedAboutOverload = false;
    private bool powerOverloadedLastUpdate = false;
    private float lastOverload = -1f;
    private float lastOverloadDialogue = -1f;
    private float powerOverloadAlertWait = 10f;
    private float powerOverloadAlertInitialWait = 10f;
    private float powerOverloadAlertCooldownWait = 30f;


    private float tick = 0;
    private int countdown = 60;

    private bool completedUpgrades = false;
    private float upgradesTimer = 0;

    private int generatorLimit;

    private float negativePowerDuration = 0f;
    private bool alertedToNegativePower = false;

    // Public Properties -----------------------------------------------------------------------------------------------

    // Basic Public Properties
    public static ObjectiveController Instance { get; protected set; }
    public int Countdown { get => countdown; set => countdown = value; }
    public int CurrStage { get => (int)currStage; }
    public int GeneratorLimit { get => generatorLimit; set => generatorLimit = value; }
    public int EarlyGameGeneratorLimit { get => earlyGameGeneratorLimit; set => earlyGameGeneratorLimit = value; }
    public int MineralTarget { get => mineralTarget; }
    public GameObject ObjectiveCompletePrefab { get => objectiveCompletePrefab; }
    public bool PowerOverloaded { get => powerOverloaded; set => powerOverloaded = value; }
    //public int PowerTarget { get => powerTarget; }

    // Start functions -------------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more objective managers.");
        }

        if (GlobalVars.LoadedFromMenu)
        {
            skipTutorial = GlobalVars.SkipTut;
            if (skipTutorial)
            {
                currStage = ObjectiveStage.HarvestMinerals;
            }
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        lastOverload = Time.fixedTime;
        lastOverloadDialogue = Time.fixedTime;
        WorldController.Instance.Inputs.InputMap.OpenCloseObjectiveWindow.performed += ctx => ToggleObjWindow();
    }

    // Update Functions ------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        Profiler.BeginSample("objective");
        if (objectivesOn)
        {
            CheckObjectiveStage();
        }
        Profiler.EndSample();

        if (currStage > ObjectiveStage.None)
        {
            UIController.instance.UpdateObjectiveText(currStage);
            CheckPowerOverloaded();
        }
    }

    void CheckObjectiveStage()
    {
        switch (currStage)
        {
            case ObjectiveStage.HarvestMinerals:
                HarvestMineralStage();
                break;
            case ObjectiveStage.RecoverPart:
                GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>().StageTwoMusic();
                RecoverPartStage();
                break;
            case ObjectiveStage.Upgrades:
                UpgradesStage();
                break;
            case ObjectiveStage.SurvivalStage:
                GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>().StageThreeMusic();
                SurvivalStage();
                break;
            case ObjectiveStage.Finished:
                //End of game
                break;
            default:
                break;
        }
    }

    private void CheckPowerOverloaded()
    {
        powerOverloaded = ResourceController.Instance.PowerChange < 0 || ResourceController.Instance.StoredPower <= 0;

        //Power Overloaded-ness changed
        if (powerOverloaded != powerOverloadedLastUpdate)
        {
            powerOverloadedLastUpdate = !powerOverloadedLastUpdate;

            if (powerOverloaded)
            {
                lastOverload = Time.fixedTime;
                alertedAboutOverload = false;
                powerOverloadAlertWait = powerOverloadAlertInitialWait;
            }
        }

        //Overloaded and no dialogue up
        if (powerOverloaded && !alertedAboutOverload && !dialogueBox.Activated && (Time.fixedTime - lastOverload) > powerOverloadAlertWait)
        {
            lastOverloadDialogue = Time.fixedTime;
            SendDialogue("power overloaded", 0f);
            alertedAboutOverload = true;
        }
        //Alert timed out or power restored
        else if (dialogueBox.Activated && dialogueBox.CurrentDialogueSet == "power overloaded" && (!powerOverloaded || (Time.fixedTime - lastOverloadDialogue) > 10f))
        {
            dialogueBox.SubmitDeactivation();
        }
        //Dialogue deactivating while overloaded
        else if (powerOverloaded && dialogueBox.Deactivating)
        {
            lastOverload = Time.fixedTime;
            alertedAboutOverload = false;
                
            //Alert timed out
            if (dialogueBox.LastDialogueSet == "power overloaded")
            {
                powerOverloadAlertWait = powerOverloadAlertCooldownWait;
            }
            //Other dialogue came up
            else
            {
                powerOverloadAlertWait = powerOverloadAlertInitialWait;
            }  
        }
    }

    // Stage Functions -------------------------------------------------------------------------------------------------

    void HarvestMineralStage()
    {
        switch (subStage)
        {
            case 0:
                if (cameraController.FinishedOpeningCameraPan)
                {
                    if (TutorialController.Instance.SkipTutorial)
                    {
                        SendDialogue("start harvest stage", 1);
                        IncrementSubStage();
                    }
                    else
                    {
                        currStage = ObjectiveStage.RecoverPart;
                        RecoverPartStage();
                    }
                }

                break;
            case 1:
                // Update objective window with 0-500 mineral gauge, and button for fix hull when gauge filled
                if (ResourceController.Instance.StoredMineral >= mineralTarget)
                {
                    GoToSubStage(3);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 2:
                if (ResourceController.Instance.StoredMineral >= mineralTarget)
                {
                    IncrementSubStage();
                }

                break;
            case 3:
                if (UIController.instance.buttonClosed)
                {
                    UIController.instance.ShowRepairButton("O");
                    IncrementSubStage();
                }

                break;
            case 4:
                if (ResourceController.Instance.StoredMineral < mineralTarget)
                {
                    UIController.instance.CloseButton();
                    SendDialogue("maintain minerals", 1);
                    GoToSubStage(1);
                }

                break;
            default:
                break;
        }
    }

    void RecoverPartStage()
    {
        upgradesTimer += Time.deltaTime;

        switch (subStage)
        {
            case 0:
                // Set fog AI to 'Moderate Aggression'
                Fog.Instance.Intensity = 2;
                generatorLimit = midGameGeneratorLimit;
                // Play music Var 2 soundtrack
                GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>().StageTwoMusic();

                if (TutorialController.Instance.SkipTutorial)
                {
                    cameraController.MovementEnabled = false;
                    hubScript.Animator.enabled = false;  //add this, so the repaired hub is shown/active ? not sure if we need animator to set to true back.
                    hubScript.SetCurrentModel("repaired");
                    // Run AI completion text
                    SendDialogue("start part stage", 1);
                    //Camera pans to the thruster
                    thruster.SetActive(true);
                    thrusterCamera.gameObject.SetActive(true);
                    Time.timeScale = 0.25f;
                    IncrementSubStage();
                }
                else
                {
                    GoToSubStage(2);
                }

                break;
            case 1:
                //Read dialogue about the thruster (only if skipped tutorial), then revert back to playing
                if (dialogueRead)
                {
                    Time.timeScale = 1f;
                    thrusterCamera.gameObject.SetActive(false);
                    cameraController.MovementEnabled = true;
                    DismissDialogue();
                    GoToSubStage(3);
                }

                break;
            case 2:
                // Update objectives window to 'Recover ship thrusters'
                // End stage if the part is collected

                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {
                    //thrusterChildWithMaterial.GetComponent<MeshRenderer>().material.DOFloat(1, "_Lerp", 1f);

                    thruster.GetComponentInChildren<MeshRenderer>().material.DOFloat(1, "_Lerp", 1f);
                    thruster.transform.DOMoveY(transform.position.y - 0.5f, 1);
                    GoToSubStage(4);
                }
                else if (dialogueBox.DialogueTimer >= 10 && dialogueBox.Activated)
                {
                    dialogueBox.RegisterDialogueRead();
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (!TutorialController.Instance.SkipTutorial && !completedUpgrades && ResourceController.Instance.StoredMineral > mineralsForUpgrades && upgradesTimer > 30)
                {
                    ResetSubStage();
                    currStage = ObjectiveStage.Upgrades;
                }


                break;
            case 3:
                // Update objectives window to 'Recover ship thrusters'
                // End stage if the part is collected
                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {

                    //thrusterChildWithMaterial.GetComponent<MeshRenderer>().material.DOFloat(1, "_Lerp", 1f);

                    thruster.GetComponentInChildren<MeshRenderer>().material.DOFloat(1, "_Lerp", 1f);
                    thruster.transform.DOMoveY(transform.position.y - 0.5f, 1);
                    IncrementSubStage();
                }
                else if (!TutorialController.Instance.SkipTutorial && !completedUpgrades && ResourceController.Instance.StoredMineral > mineralsForUpgrades && upgradesTimer > 30)
                {
                    ResetSubStage();
                    currStage = ObjectiveStage.Upgrades;
                }

                break;
            case 4:
                if (UIController.instance.buttonClosed)
                {
                    UIController.instance.ShowAttachButton();
                    IncrementSubStage();
                }

                break;
            case 5:
                break;
            case 6:
                // Update hub model with attached thrusters
                thruster.SetActive(false);
                hubScript.SetCurrentModel("attached");

                // Play music Var 3 soundtrack
                GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>().StageThreeMusic();

                //Go to next stage
                GoToStage(ObjectiveStage.SurvivalStage);

                break;
            default:
                break;
        }
    }

    //Player learns about and uses upgrades
    private void UpgradesStage()
    {
        switch (subStage)
        {
            case 0:
                SendDialogue("upgrades click ship", 1);
                UIController.instance.UpdateObjectiveText(currStage);

                if (!objWindowVisible)
                {
                    ToggleObjWindow();
                }

                IncrementSubStage();
                break;
            case 1:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (upgradesButton.activeSelf)
                {
                    GoToSubStage(3);
                }

                break;
            case 2:
                if (upgradesButton.activeSelf)
                {
                    IncrementSubStage();
                }

                break;
            case 3:
                SendDialogue("upgrades click icon", 0);
                IncrementSubStage();
                break;
            case 4:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (upgradesCanvas.activeSelf)
                {
                    Time.timeScale = 0.01f;
                    GoToSubStage(6);
                }

                break;
            case 5:
                if (upgradesCanvas.activeSelf)
                {
                    GoToSubStage(6);
                }

                break;
            case 6:
                SendDialogue("upgrades use upgrade", 0);
                IncrementSubStage();
                break;
            case 7:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (WorldController.Instance.UpgradeUsed)
                {
                    GoToSubStage(9);
                }

                break;
            case 8:
                if (WorldController.Instance.UpgradeUsed)
                {
                    IncrementSubStage();
                }

                break;
            case 9:
                SendDialogue("upgrades finished", 0);
                IncrementSubStage();
                break;
            case 10:
                if (dialogueRead)
                {
                    DismissDialogue();
                }
                else if (!upgradesCanvas.activeSelf)
                {
                    GoToSubStage(12);
                }

                break;
            case 11:
                if (!upgradesCanvas.activeSelf)
                {
                    IncrementSubStage();
                }

                break;

            case 12:
                completedUpgrades = true;
                Time.timeScale = 1;
                currStage = ObjectiveStage.RecoverPart;
                subStage = 2;
                break;
            default:
                SendDialogue("error", 1);
                Debug.Log("inaccurate sub stage");
                break;
        }
    }

    void SurvivalStage()
    {
        Hub.Instance.extinguishingFire(); // smoke for the hub
        switch (subStage)
        {
            case 0:
                // Set fog AI to 'Overly Aggressive'
                Fog.Instance.Intensity = 3;
                generatorLimit = lateGameGeneratorLimit;

                // Run AI completion text
                SendDialogue("end part stage", 1);
                UIController.instance.ShowCountdownSlider();
                IncrementSubStage();
                break;
            case 1:
                //Survival countdown
                Tick();

                if (countdown <= 0)
                {
                    tick = 0;
                    GoToSubStage(5);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 2:
                SendDialogue("start power stage", 1);
                IncrementSubStage();
                break;
            case 3:
                //Survival countdown
                Tick();

                Debug.Log($"Countdown: {countdown}");

                if (countdown <= 0)
                {
                    tick = 0;
                    GoToSubStage(5);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 4:
                //Survival countdown
                Tick();
                Debug.Log($"Countdown: {countdown}");

                if (countdown <= 0)
                {
                    tick = 0;
                    IncrementSubStage();
                }

                break;
            case 5:
                UIController.instance.HideCountdownSlider();
                // Run AI completion text
                SendDialogue("end power stage", 1);
                IncrementSubStage();
                break;
            case 6:
                if (UIController.instance.buttonClosed)
                {
                    UIController.instance.ShowLaunchButton();
                    IncrementSubStage();
                }

                break;
            case 7:
                break;
            case 8:
                UIController.instance.WinGame();
                break;
            default:
                // Note: if more stages are added to the objective controller, when the last one is fulfilled, you can't just 
                // reset the substage, or it'll loop back to the start of the stage rather than finishing and that will
                // create issues with the dialogue if there isn't a propper dialogueRead check like this one.
                break;
        }
    }

    // Utility Functions -----------------------------------------------------------------------------------------------

    private void Tick()
    {
        UIController.instance.UpdateCountdownSlider();
        tick += Time.deltaTime;

        if (tick >= 1)
        {
            tick -= 1;
            countdown -= 1;
        }
    }

    protected override void SendDialogue(string dialogueKey, float invokeDelay)
    {
        dialogueRead = false;
        base.SendDialogue(dialogueKey, invokeDelay);
    }

    public void FogDestroyed()
    {
        SendDialogue("fog destroyed", 1);
    }

    public void AllTilesPowered()
    {
        SendDialogue("all tiles powered", 1);
    }

    private void DismissDialogue()
    {
        ResetDialogueRead();
        IncrementSubStage();
    }

    public void GoToSubStage(int nextSubStage)
    {
        ResetDialogueRead();
        subStage = nextSubStage;
    }

    public void IncrementStage()
    {
        if (currStage != 0)
        {
            //generatorLimit += 4;
            StartCoroutine(CompleteObjective($"You can build a maximum of {generatorLimit + 10} generators now!"));
        }
        else
        {
            if (!objWindowVisible)
            {
                ToggleObjWindow();
            }
        }

        ResetSubStage();
        // stageComplete = false;
        currStage++;
    }

    public void GoToStage(ObjectiveStage stage)
    {
        if (currStage != 0)
        {
            //generatorLimit += 4;
            StartCoroutine(CompleteObjective($"You can build a maximum of {generatorLimit + 10} generators now!"));
        }
        else
        {
            if (!objWindowVisible)
            {
                ToggleObjWindow();
            }
        }

        ResetSubStage();
        // stageComplete = false;
        currStage = stage;
    }

    public void IncrementSubStage()
    {
        subStage++;
    }

    public void DecrementSubStage()
    {
        subStage--;
    }

    void ResetSubStage()
    {
        subStage = 0;
    }

    // Run as coroutine
    IEnumerator CompleteObjective(string message)
    {
        GameObject objComp = Instantiate(objectiveCompletePrefab, GameObject.Find("Canvas").transform);
        GameObject objCompImage = objComp.GetComponentInChildren<Image>().gameObject;
        TextMeshProUGUI unlocksText = objCompImage.GetComponentInChildren<TextMeshProUGUI>();
        unlocksText.text = message;
        objCompImage.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-Win", GetComponent<Transform>().position);
        yield return new WaitForSecondsRealtime(5f);
        objCompImage.GetComponent<RectTransform>().DOAnchorPosX(1250, 0.3f).SetEase(Ease.InQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.3f);
        Destroy(objComp);
        if (!objWindowVisible)
        {
            ToggleObjWindow();
        }
    }
}
