using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Non-Serialized Fields
    bool stageComplete = false;


    // Public Properties -------------------------------------------------------------------------------------

    public static ObjectiveController Instance { get; protected set; }

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
            case ObjectiveStage.None:
                IncrementStage();
                break;
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
                // Play music Var 2 soundtrack
                // Run AI text for stage
                SendDialogue("start part stage", 1);
                // Set fog AI to 'Moderate Aggression'
                break;
            case 1:
                // Update objectives window to 'Recover ship thrusters'

                // End stage if the part is collected
                if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
                {
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
                // Play music Var 3 soundtrack
                // Run AI text for stage
                SendDialogue("start power stage", 1);
                // Set fog AI to 'Overly Aggressive'
                IncrementSubstage();
                break;
            case 1:
                // Update objective window to 100-5000 power gauge, and button for escape when gauge is filled
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

    void IncrementStage()
    {
        stageComplete = false;
        currStage++;
    }

    void IncrementSubstage()
    {
        subStage++;
    }

    void ResetSubstage()
    {
        subStage = 0;
    }
}
