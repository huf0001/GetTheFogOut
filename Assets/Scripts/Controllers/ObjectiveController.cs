using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] AudioClip audioStage1;
    [SerializeField] AudioClip audioTransition1To2;
    [SerializeField] AudioClip audioStage2;
    [SerializeField] AudioClip audioTransition2To3;
    [SerializeField] AudioClip audioStage3;

    [SerializeField] int fogGrowthEasy;
    [SerializeField] int fogGrowthMedium;
    [SerializeField] int fogGrowthHard;

    // Non-Serialized Fields
    bool stageComplete = false;
    private AudioSource audioSource;

    // Public Properties -------------------------------------------------------------------------------------

    public static ObjectiveController Instance { get; protected set; }
    public int PowerTarget { get => powerTarget; }
    public int MineralTarget { get => mineralTarget; }
    public int CurrStage { get => (int)currStage; }
    public int GeneratorLimit { get => generatorLimit; }

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
    }

    // Update Functions -------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        if (objectivesOn) // && TutorialController.Instance.TutorialStage == TutorialStage.Finished)
        {
            CheckObjectiveStage();
        }
    }

    void CheckObjectiveStage()
    {
        UIController.instance.UpdateObjectiveText(currStage);
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
                    SkipObjectivesAhead(3);
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
                // Run AI completion text
                SendDialogue("end harvest stage", 1);
                IncrementSubStage();
                break;
            case 4:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ResetSubStage();
                    IncrementStage();
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
                ShipComponent.SetActive(true);
                // Play music Var 2 soundtrack
                MusicController.Instance.StartStage2();
                // Set fog AI to 'Moderate Aggression'
                Fog.Instance.FogGrowth = fogGrowthMedium;
                // Run AI text for stage
                SendDialogue("start part stage", 1);
                IncrementSubStage();
                break;
            case 1:
                // Update objectives window to 'Recover ship thrusters'
                // End stage if the part is collected
                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {
                    ShipComponent.SetActive(false);
                    SkipObjectivesAhead(3);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }
                break;
            case 2:
                // Update objectives window to 'Recover ship thrusters'
                // End stage if the part is collected
                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {
                    ShipComponent.SetActive(false);
                    IncrementSubStage();
                }

                break;
            case 3:
                // Run AI completion text
                SendDialogue("end part stage", 1);
                IncrementSubStage();
                break;
            case 4:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ResetSubStage();
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
                // Update hub model with attached thrusters
                hub.transform.GetChild(1).gameObject.SetActive(false);
                hub.transform.GetChild(2).gameObject.SetActive(true);
                // Play music Var 3 soundtrack
                MusicController.Instance.StartStage3();
                // Set fog AI to 'Overly Aggressive'
                Fog.Instance.FogGrowth = fogGrowthHard;
                // Run AI text for stage
                SendDialogue("start power stage", 1);
                IncrementSubStage();
                break;
            case 1:
                // Update objective window to 100-5000 power gauge, and button for escape when gauge is filled
                if (ResourceController.Instance.StoredPower >= 500)
                {
                    SkipObjectivesAhead(3);
                }
                else if (dialogueRead)
                {
                    DismissDialogue();
                }
                break;
            case 2:
                // Update objective window to 100-5000 power gauge, and button for escape when gauge is filled
                if (ResourceController.Instance.StoredPower >= 500)
                {
                    IncrementSubStage();
                }
                break;
            case 3:
                // Run AI completetion text
                SendDialogue("end power stage", 1);
                IncrementSubStage();
                break;
            case 4:
                if (dialogueRead)
                {
                    DismissDialogue();
                    ResetSubStage();
                    IncrementStage();
                    // Note: if more stages are added to the objective controller, when the last one is fulfilled, you can't just 
                    // reset the substage, or it'll loop back to the start of the stage rather than finishing and that will
                    // create issues with the dialogue if there isn't a propper dialogueRead check like this one.
                }
                break;
            default:
                break;
        }
    }

    // Utility Functions ------------------------------------------------------------------------------------------

    protected override void SendDialogue(string dialogueKey, float invokeDelay)
    {
        dialogueRead = false;
        base.SendDialogue(dialogueKey, invokeDelay);
    }

    private void DismissDialogue()
    {
        ResetDialogueRead();
        IncrementSubStage();
    }

    private void SkipObjectivesAhead(int nextSubStage)
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

        stageComplete = false;
        currStage++;
    }

    void IncrementSubStage()
    {
        subStage++;
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
        objCompImage.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).SetEase(Ease.OutQuad);
        audioSource.PlayOneShot(audioCompleteObjective);
        yield return new WaitForSeconds(5f);
        objCompImage.GetComponent<RectTransform>().DOAnchorPosX(1250, 0.3f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(0.3f);
        Destroy(objComp);
        if (!objWindowVisible)
        {
            ToggleObjWindow();
        }
    }
}
