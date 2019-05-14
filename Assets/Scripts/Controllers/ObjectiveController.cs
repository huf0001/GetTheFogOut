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
    StorePower
}

public class ObjectiveController : DialogueBoxController
{
    // Fields -------------------------------------------------------------------------------------

    // Serialized Fields
    [SerializeField] bool objectivesOn = true;
    [SerializeField] ObjectiveStage currStage = ObjectiveStage.None;
    [SerializeField] int subStage = 0;
    [SerializeField] GameObject objectiveWindow;
    [SerializeField] GameObject objectiveCompletePrefab;
    [SerializeField] GameObject hub;
    [SerializeField] GameObject ShipComponent;
    [SerializeField] int mineralTarget = 500;
    [SerializeField] int powerTarget = 500;
    [SerializeField] int generatorLimit = 1;
    [SerializeField] AudioClip audioCompleteObjective;
    [SerializeField] AudioClip audioStage1;
    [SerializeField] AudioClip audioTransition1To2;
    [SerializeField] AudioClip audioStage2;
    [SerializeField] AudioClip audioTransition2To3;
    [SerializeField] AudioClip audioStage3;

    // Non-Serialized Fields
    bool stageComplete = false;
    bool objWindowVisibilty = false;
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
        if (objectivesOn)
        {
            CheckObjectiveStage();
        }
    }

    void CheckObjectiveStage()
    {
        switch (currStage)
        {
            // case ObjectiveStage.None:

            //     break;
            case ObjectiveStage.HarvestMinerals:
                HarvestMineralStage();
                break;
            case ObjectiveStage.RecoverPart:
                RecoverPartStage();
                break;
            case ObjectiveStage.StorePower:
                StorePowerStage();
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
                // Run AI text for stage
                SendDialogue("start harvest stage", 1);
                // Set fog AI to 'Docile'
                // Unlock 5 generators
                IncrementSubstage();
                break;
            case 1:
                // Update objective window with 0-500 mineral gauge, and button for fix hull when gauge filled
                UIController.instance.UpdateObjectiveText((int)currStage);
                if (ResourceController.Instance.StoredMineral >= 500)
                {
                    IncrementSubstage();
                }
                break;
            case 2:
                // Run AI completion text
                SendDialogue("end harvest stage", 1);
                ResetSubstage();
                IncrementStage();
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
                // Run AI text for stage
                SendDialogue("start part stage", 1);
                // Set fog AI to 'Moderate Aggression'
                IncrementSubstage();
                break;
            case 1:
                // Update objectives window to 'Recover ship thrusters'
                UIController.instance.UpdateObjectiveText((int)currStage);
                // End stage if the part is collected
                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {
                    ShipComponent.SetActive(false);
                    IncrementSubstage();
                }
                break;
            case 2:
                // Run AI completion text
                SendDialogue("end part stage", 1);
                ResetSubstage();
                IncrementStage();
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
                // Run AI text for stage
                SendDialogue("start power stage", 1);
                // Set fog AI to 'Overly Aggressive'
                IncrementSubstage();
                break;
            case 1:
                // Update objective window to 100-5000 power gauge, and button for escape when gauge is filled
                UIController.instance.UpdateObjectiveText((int)currStage);
                if (ResourceController.Instance.StoredPower >= 500)
                {
                    IncrementSubstage();
                }
                break;
            case 2:
                // Run AI completetion text
                SendDialogue("end power stage", 1);
                ResetSubstage();
                IncrementStage();
                break;
            default:
                break;
        }
    }

    // Utility Functions ------------------------------------------------------------------------------------------

    public void IncrementStage()
    {
        if (currStage != 0)
        {
            StartCoroutine(CompleteObjective());
        }
        stageComplete = false;
        currStage++;
        generatorLimit += 4;
    }

    void IncrementSubstage()
    {
        subStage++;
    }

    void ResetSubstage()
    {
        subStage = 0;
    }

    public void ToggleObjWindow()
    {
        if (!objWindowVisibilty)
        {
            objectiveWindow.GetComponent<RectTransform>().DOAnchorPosX(-5, 0.3f).SetEase(Ease.OutCubic);
            //objectiveWindow.GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(true);
            objWindowVisibilty = true;
        }
        else
        {
            objectiveWindow.GetComponent<RectTransform>().DOAnchorPosX(227, 0.3f).SetEase(Ease.InCubic);
            //objectiveWindow.GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(false);
            objWindowVisibilty = false;
        }
    }

    // run as coroutine
    IEnumerator CompleteObjective()
    {
        GameObject objComp = Instantiate(objectiveCompletePrefab, GameObject.Find("Canvas").transform);
        GameObject objCompImage = objComp.GetComponentInChildren<Image>().gameObject;
        TextMeshProUGUI unlocksText = objCompImage.GetComponentInChildren<TextMeshProUGUI>();
        unlocksText.text = $"You can build an extra 4 generators now!";
        objCompImage.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).SetEase(Ease.OutQuad);
        audioSource.PlayOneShot(audioCompleteObjective);
        yield return new WaitForSeconds(5f);
        objCompImage.GetComponent<RectTransform>().DOAnchorPosX(1250, 0.3f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(0.3f);
        Destroy(objComp);
        if (!objWindowVisibilty)
        {
            ToggleObjWindow();
        }
    }
}
