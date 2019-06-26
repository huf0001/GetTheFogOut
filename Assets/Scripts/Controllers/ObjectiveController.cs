using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Cinemachine;

public enum ObjectiveStage
{
    None,
    HarvestMinerals,
    RecoverPart,
    StorePower,
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
    [SerializeField] GameObject ShipComponent;
    [SerializeField] int mineralTarget = 500;
    [SerializeField] int powerTarget = 500;
    [SerializeField] int generatorLimit = 3;
    [SerializeField] AudioClip audioCompleteObjective;

    [SerializeField] int fogGrowthEasy;
    [SerializeField] int fogGrowthMedium;
    [SerializeField] int fogGrowthHard;

    [SerializeField] GameObject shipSmoke;

    [SerializeField] private CinemachineVirtualCamera thrusterCamera;

    // Non-Serialized Fields
    bool stageComplete = false;
    private AudioSource audioSource;

    private bool powerOverloaded = false;
    private bool alertedAboutOverload = false;
    private bool powerOverloadedLastUpdate = false;
    private float lastOverload = -1f;
    private float lastOverloadDialogue = -1f;

    // Public Properties -------------------------------------------------------------------------------------

    public static ObjectiveController Instance { get; protected set; }
    public int PowerTarget { get => powerTarget; }
    public int MineralTarget { get => mineralTarget; }
    public int CurrStage { get => (int)currStage; }
    public int GeneratorLimit { get => generatorLimit; }
    public bool PowerOverloaded { get => powerOverloaded; set => powerOverloaded = value; }

    // Start functions -------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more objective managers.");
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lastOverload = Time.fixedTime;
        lastOverloadDialogue = Time.fixedTime;
        CheckDifficulty();
    }

    // Update Functions -------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        if (objectivesOn) // && TutorialController.Instance.TutorialStage == TutorialStage.Finished)
        {
            CheckObjectiveStage();
        }

        if (currStage > ObjectiveStage.None)
        {
            UIController.instance.UpdateObjectiveText(currStage);
            CheckPowerOverloaded();
        }
    }

    void CheckDifficulty()
    {
        switch (Fog.Instance.selectDifficulty)
        {
            case Difficulty.easy:
                fogGrowthEasy = fogGrowthEasy / 2;
                fogGrowthMedium = fogGrowthMedium / 2;
                fogGrowthHard = fogGrowthHard / 2;
                break;
            case Difficulty.normal:
                break;
            case Difficulty.hard:
                fogGrowthEasy = fogGrowthEasy * 2;
                fogGrowthMedium = fogGrowthMedium * 2;
                fogGrowthHard = fogGrowthHard * 2;
                break;
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
                RecoverPartStage();
                break;
            case ObjectiveStage.StorePower:
                StorePowerStage();
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
                // Play music Var 1 soundtrack
                // Set fog AI to 'Docile'
                Fog.Instance.FogGrowth = fogGrowthEasy;
                // Run AI text for stage
                SendDialogue("start harvest stage", 1);
                // Unlock 5 generators
                IncrementSubStage();
                break;
            case 1:
                // Update objective window with 0-500 mineral gauge, and button for fix hull when gauge filled
                if (ResourceController.Instance.StoredMineral >= 500)
                {
                    ChangeToSubStage(3);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 2:
                if (ResourceController.Instance.StoredMineral >= 500)
                {
                    IncrementSubStage();
                }

                break;
            case 3:
                if (UIController.instance.buttonClosed)
                {
                    UIController.instance.ShowRepairButton();
                    IncrementSubStage();
                }

                break;
            case 4:
                if (ResourceController.Instance.StoredMineral < 500)
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
                // Update Hub model to fixed ship without thrusters / Particle effects
                hub.transform.GetChild(0).gameObject.SetActive(false);
                hub.transform.GetChild(1).gameObject.SetActive(true);
                // Play music Var 2 soundtrack
                MusicController.Instance.StartStage2();
                // Set fog AI to 'Moderate Aggression'
                Fog.Instance.FogGrowth = fogGrowthMedium;
                // Run AI completion text
                SendDialogue("end harvest stage", 1);
                //Camera pans to the thruster
                ShipComponent.SetActive(true);
                thrusterCamera.gameObject.SetActive(true);
                Time.timeScale = 0.25f;
                IncrementSubStage();
                break;
            case 1:
                if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 2:
                // Run AI text for stage
                SendDialogue("start part stage", 1);
                IncrementSubStage();
                break;
            case 3:
                if (dialogueRead)
                {
                    Time.timeScale = 1f;
                    thrusterCamera.gameObject.SetActive(false);
                    DismissDialogue();
                }

                break;
            case 4:
                // Update objectives window to 'Recover ship thrusters'
                // End stage if the part is collected
                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {
                    ShipComponent.SetActive(false);
                    IncrementSubStage();
                }

                break;
            case 5:
                if (UIController.instance.buttonClosed)
                {
                    UIController.instance.ShowAttachButton();
                    IncrementSubStage();
                }

                break;
            case 6:
                break;
            case 7:
                // Update hub model with attached thrusters
                hub.transform.GetChild(1).gameObject.SetActive(false);
                shipSmoke.SetActive(false);
                hub.transform.GetChild(2).gameObject.SetActive(true);

                // Play music Var 3 soundtrack
                MusicController.Instance.StartStage3();

                // Set fog AI to 'Overly Aggressive'
                Fog.Instance.FogGrowth = fogGrowthHard;
                Fog.Instance.ToggleAnger();

                //If already completed store power stage
                if (ResourceController.Instance.StoredPower >= 500)
                {
                    //Advance stage and substage to the point where the launch button appears
                    currStage = ObjectiveStage.StorePower;
                    subStage = 5;
                }
                //Otherwise
                else
                {
                    //Go to next stage
                    IncrementStage();
                }

                break;
            default:
                break;
        }
    }

    void StorePowerStage()
    {
        switch (subStage)
        {
            case 0:
                // Run AI completion text
                SendDialogue("end part stage", 1);
                IncrementSubStage();
                break;
            case 1:
                // Update objective window to 100-5000 power gauge, and button for escape when gauge is filled
                if (ResourceController.Instance.StoredPower >= 500)
                {
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
                // Update objective window to 100-5000 power gauge, and button for escape when gauge is filled
                if (ResourceController.Instance.StoredPower >= 500)
                {
                    ChangeToSubStage(5);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }

                break;
            case 4:
                // Update objective window to 100-5000 power gauge, and button for escape when gauge is filled
                if (ResourceController.Instance.StoredPower >= 500)
                {
                    IncrementSubStage();
                }

                break;
            case 5:
                // Run AI completetion text
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
                if (ResourceController.Instance.StoredPower < 500)
                {
                    UIController.instance.CloseButton();
                    SendDialogue("maintain power", 1);
                    ChangeToSubStage(3);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                    IncrementSubStage();
                }

                break;
            case 8:
                if (ResourceController.Instance.StoredPower < 500)
                {
                    UIController.instance.CloseButton();
                    SendDialogue("maintain power", 1);
                    ChangeToSubStage(3);
                }

                break;
            default:
                // Note: if more stages are added to the objective controller, when the last one is fulfilled, you can't just 
                // reset the substage, or it'll loop back to the start of the stage rather than finishing and that will
                // create issues with the dialogue if there isn't a propper dialogueRead check like this one.
                break;
        }
    }

    // Utility Functions ------------------------------------------------------------------------------------------

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
        stageComplete = false;
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
        audioSource.PlayOneShot(audioCompleteObjective);
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
