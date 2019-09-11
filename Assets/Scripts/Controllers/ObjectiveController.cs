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
    SurvivalStage,
    Finished
}

public class ObjectiveController : DialogueBoxController
{
    // Fields -------------------------------------------------------------------------------------

    // Serialized Fields
    [SerializeField] bool objectivesOn = true;
    [SerializeField] ObjectiveStage currStage = ObjectiveStage.None;
    [SerializeField] int subStage = 0;
    [SerializeField] GameObject objectiveCompletePrefab;
    [SerializeField] GameObject hub;
    [SerializeField] Hub hubScript;
    [SerializeField] public GameObject thruster;
    [SerializeField] int mineralTarget = 500;
    [SerializeField] int powerTarget = 500;
    [SerializeField] int generatorLimit = 3;

    [SerializeField] private CameraController cameraController;
    [SerializeField] private CinemachineVirtualCamera thrusterCamera;

    // Non-Serialized Fields
    // bool stageComplete = false;

    private bool powerOverloaded = false;
    private bool alertedAboutOverload = false;
    private bool powerOverloadedLastUpdate = false;
    private float lastOverload = -1f;
    private float lastOverloadDialogue = -1f;
    private float tick = 0;
    private int countdown = 60;

    private MusicFMOD musicFMOD;

    // Public Properties -------------------------------------------------------------------------------------

    // Basic Public Properties
    public static ObjectiveController Instance { get; protected set; }
    public int Countdown { get => countdown; set => countdown = value; }
    public int CurrStage { get => (int)currStage; }
    public int GeneratorLimit { get => generatorLimit; set => generatorLimit = value; }
    public int MineralTarget { get => mineralTarget; }
    public bool PowerOverloaded { get => powerOverloaded; set => powerOverloaded = value; }
    public int PowerTarget { get => powerTarget; }

    // Start functions -------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more objective managers.");
        }

        Instance = this;

        if (GameObject.Find("MusicFMOD") != null)
        {
            musicFMOD = GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        lastOverload = Time.fixedTime;
        lastOverloadDialogue = Time.fixedTime;
        WorldController.Instance.Inputs.InputMap.OpenCloseObjectiveWindow.performed += ctx => ToggleObjWindow();
    }

    // Update Functions -------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        Profiler.BeginSample("objective");
        if (objectivesOn) // && TutorialController.Instance.TutorialStage == TutorialStage.Finished)
        {
            
            // This is very performance heavy, not sure if required so won't delete for now. 
            /* if (GameObject.Find("MusicFMOD") != null)
            {
                musicFMOD = GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>();
            } */
            
            
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
                musicFMOD.StageOneMusic();
                HarvestMineralStage();
                break;
            case ObjectiveStage.RecoverPart:
                musicFMOD.StageTwoMusic();
                RecoverPartStage();
                break;
            case ObjectiveStage.SurvivalStage:
                musicFMOD.StageThreeMusic();
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
        //Debug.Log("Checking Power Overloaded");
        if (powerOverloaded != powerOverloadedLastUpdate)
        {
            powerOverloadedLastUpdate = !powerOverloadedLastUpdate;

            if (powerOverloaded)
            {
                lastOverload = Time.fixedTime;
                alertedAboutOverload = false;
            }
        }

        if (powerOverloaded && !alertedAboutOverload && !aiText.Activated && (Time.fixedTime - lastOverload) >= 5f)
        {
            lastOverloadDialogue = Time.fixedTime;
            SendDialogue("power overloaded", 0f);
            alertedAboutOverload = true;
        }
        else if (powerOverloaded && alertedAboutOverload && aiText.Activated && aiText.CurrentDialogueSet != "power overloaded" && (Time.fixedTime - lastOverload) <= 2f)
        {
            alertedAboutOverload = false;
        }
        else if (aiText.Activated && aiText.CurrentDialogueSet == "power overloaded" && (!powerOverloaded || (Time.fixedTime - lastOverloadDialogue) >= 10f))
        {
            aiText.SubmitDeactivation();
        }
    }

    // Stage Functions ----------------------------------------------------------------------------------------

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
                    ChangeToSubStage(3);
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
                    ChangeToSubStage(1);
                }

                break;
            default:
                break;
        }
    }

    void RecoverPartStage()
    {
        switch (subStage)
        {
            case 0:
                // Set fog AI to 'Moderate Aggression'
                Fog.Instance.Intensity += 1;
                // Play music Var 2 soundtrack
                musicFMOD.StageTwoMusic();

                if (TutorialController.Instance.SkipTutorial)
                {
                    cameraController.MovementEnabled = false;
                    hubScript.Animator.enabled = false;  //add this, so the repaired hub is shown/active ? not sure if we need animator to set to true back.
                    hubScript.BrokenShip.SetActive(false);
                    hubScript.AttachedWing.SetActive(false);
                    hubScript.RepairedShip.SetActive(true);
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
                    generatorLimit += 4;    //Would normally be incremented in IncrementStage()
                    ChangeToSubStage(2);
                }

                break;
            case 1:
                if (dialogueRead)
                {
                    Time.timeScale = 1f;
                    thrusterCamera.gameObject.SetActive(false);
                    cameraController.MovementEnabled = true;
                    DismissDialogue();
                    ChangeToSubStage(3);
                }

                break;
            case 2:
                // Update objectives window to 'Recover ship thrusters'
                // End stage if the part is collected
                
                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {
                    thruster.SetActive(false);
                    ChangeToSubStage(4);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 3:
                // Update objectives window to 'Recover ship thrusters'
                // End stage if the part is collected
                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {
                    thruster.SetActive(false);
                    IncrementSubStage();
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
                hubScript.BrokenShip.SetActive(false);
                hubScript.RepairedShip.SetActive(false);
                hubScript.AttachedWing.SetActive(true);

                // Play music Var 3 soundtrack
                musicFMOD.StageThreeMusic();

                //Go to next stage
                IncrementStage();

                break;
            default:
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
                Fog.Instance.Intensity += 1;

                // Run AI completion text
                SendDialogue("end part stage", 1);
                UIController.instance.ShowCountdownSlider();
                IncrementSubStage();
                break;
            case 1:
                //Survival countdown
                Tick();
                //Debug.Log($"Countdown: {countdown}");

                if (countdown <= 0)
                {
                    tick = 0;
                    ChangeToSubStage(5);
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
                    ChangeToSubStage(5);
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

    // Utility Functions ------------------------------------------------------------------------------------------

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

    public void ChangeToSubStage(int nextSubStage)
    {
        ResetDialogueRead();
        subStage = nextSubStage;
    }

    public void IncrementStage()
    {
        if (currStage != 0)
        {
            generatorLimit += 4;
            StartCoroutine(CompleteObjective());
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

    // run as coroutine
    IEnumerator CompleteObjective()
    {
        GameObject objComp = Instantiate(objectiveCompletePrefab, GameObject.Find("Canvas").transform);
        GameObject objCompImage = objComp.GetComponentInChildren<Image>().gameObject;
        TextMeshProUGUI unlocksText = objCompImage.GetComponentInChildren<TextMeshProUGUI>();
        unlocksText.text = $"You can build a maximum of {generatorLimit} generators now!";
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
