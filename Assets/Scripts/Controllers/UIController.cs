using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIController : MonoBehaviour
{

    public static UIController instance = null;

    TextMeshProUGUI powerText, organicText, mineralText, fuelText;
    public GameObject endGame, pauseGame;
    GameObject hudBar;
    public BuildingSelector buildingSelector;
    public BuildingInfo buildingInfo;
    public TextMeshProUGUI endGameText;

    private GameObject cursor;
    private Slider powerSlider;
    private int power = 0, powerChange = 0, mineral = 0;
    private float powerVal = 0.0f, mineralVal = 0.0f;
    private float powerTime = 0.0f, mineralTime = 0.0f;
    private bool isCursorOn = false;
    [SerializeField] Image powerImg;

    [SerializeField] Sprite[] powerLevelSprites;
    [SerializeField] TextMeshProUGUI objWindowText;
    [SerializeField] TextMeshProUGUI hudObjText;

    ResourceController resourceController = null;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        FindSliders();

        cursor = GameObject.Find("Cursor");
        cursor.SetActive(false);
        //Tweens in the UI for a smooth bounce in from outside the canvas
        //hudBar = GameObject.Find("HUD");// "HudBar");
        //hudBar.GetComponent<RectTransform>().DOAnchorPosY(200f, 1.5f).From(true).SetEase(Ease.OutBounce);
    }

    // Update is called once per frame
    void Update()
    {
        if (resourceController == null)
        {
            resourceController = ResourceController.Instance;
        }

        powerTime += Time.deltaTime;
        mineralTime += Time.deltaTime;
        UpdateResourceText();

        if (Input.GetKeyDown("c"))
        {
            isCursorOn = !isCursorOn;
            cursor.SetActive(isCursorOn);
        }
    }

    // find sliders and text
    void FindSliders()
    {
        powerText = GameObject.Find("PowerLevel").GetComponent<TextMeshProUGUI>();
        mineralText = GameObject.Find("MineralLevel").GetComponent<TextMeshProUGUI>();
    }

    // End Game Method
    public void EndGameDisplay(string text)
    {
        endGameText.text = text;
        endGame.SetActive(true);
    }

    public void ToggleCursor(bool isOn)
    {
        cursor.SetActive(isOn);
    }

    void UpdateResourceText()
    {
        if (resourceController != null)
        {
            // if the stored power is different, change values used for lerping
            if (resourceController.StoredPower != power)
            {
                powerVal = power;
                power = resourceController.StoredPower;
                powerTime = 0;
            }

            powerChange = resourceController.PowerChange;

            // change colour of power change text depending on +, - or ±
            string colour;
            if (powerChange > 0)
            {
                WorldController.Instance.changePowerTIle(Color.green);
                colour = "#009900>+";
            }
            else if (powerChange < 0)
            {
                WorldController.Instance.changePowerTIle(Color.red);
                colour = "\"red\">";
            }
            else
            {
                WorldController.Instance.changePowerTIle(Color.yellow);
                colour = "#006273>±";
            }

            // update text values
            powerText.text = Mathf.Round(Mathf.Lerp(powerVal, power, powerTime)) + "/" + resourceController.MaxPower + "\n<color=" + colour + powerChange + "</color>";

            float powerCheck = float.Parse(powerText.text.Split('/')[0]) / resourceController.MaxPower;

            if (powerCheck > 0 && powerCheck <= .25f && powerImg.sprite != powerLevelSprites[1])
            {
                powerImg.sprite = powerLevelSprites[1];
            }
            else if (powerCheck > .25f && powerCheck <= .50f && powerImg.sprite != powerLevelSprites[2])
            {
                powerImg.sprite = powerLevelSprites[2];
            }
            else if (powerCheck > .50f && powerCheck <= .75f && powerImg.sprite != powerLevelSprites[3])
            {
                powerImg.sprite = powerLevelSprites[3];
            }
            else if (powerCheck > .75f && powerImg.sprite != powerLevelSprites[4])
            {
                powerImg.sprite = powerLevelSprites[4];
            }
            else if (powerCheck == 0 && powerImg.sprite != powerLevelSprites[0])
            {
                powerImg.sprite = powerLevelSprites[0];
            }
            
            if (resourceController.StoredMineral != mineral)
            {
                mineralVal = mineral;
                mineral = resourceController.StoredMineral;
                mineralTime = 0;
            }

            int mineralChange = resourceController.MineralChange;

            if (mineralChange > 0)
            {
                colour = "#009900>+";
            }
            else if (mineralChange < 0)
            {
                colour = "\"red\">";
            }
            else
            {
                colour = "#006273>±";
            }

            mineralText.text = Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime)) + " units\n<color=" + colour + mineralChange + "</color>";
        }
    }

    public void UpdateObjectiveText(ObjectiveStage stageNum)
    {
        switch (stageNum)
        {
            case ObjectiveStage.None:
                hudObjText.text = "Objective: Complete the Tutorial";
                objWindowText.text = "<b>Complete the Tutorial</b>\n\n" +
                    "<size=75%>Proceed through the tutorial and learn to play the game!\n\n";
                break;
            case ObjectiveStage.HarvestMinerals:
                hudObjText.text = "Objective: Repair the Hull";
                objWindowText.text = "<b>Repair the Hull</b>\n\n" +
                    "<size=75%>Gather enough mineral resources to repair your ship's hull.\n\n" +
                    $"Target: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))} / {ObjectiveController.Instance.MineralTarget} <size=90%><sprite=\"all_icons\" index=2>";
                break;
            case ObjectiveStage.RecoverPart:
                string nf = "Not Found";
                string f = "Found";
                hudObjText.text = "Objective: Recover the Thruster";
                objWindowText.text = "<b>Recover the Thruster</b>\n\n" +
                    "<size=75%>Push your way through the fog to find the missing thruster from your ship.\n\n" +
                    "Thrusters: " + (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected ? f : nf);
                break;
            case ObjectiveStage.StorePower:
                hudObjText.text = "Objective: Leave the Planet";
                objWindowText.text = "<b>Leave the Planet</b>\n\n" +
                    "<size=75%>The fog is out to get you! Hurry and gather enough power to leave this wretched planet behind!\n\n" +
                    $"Target: {Mathf.Round(Mathf.Lerp(powerVal, power, powerTime))} / {ObjectiveController.Instance.PowerTarget} <size=90%><sprite=\"all_icons\" index=0>";
                break;
        }
    }

    public void UpdateObjectiveText(TutorialStage stage)
    {
        switch (stage)
        {
            case TutorialStage.None:
            case TutorialStage.CrashLanding:
            case TutorialStage.ShipPartsCrashing:
            case TutorialStage.ZoomBackToShip:
            case TutorialStage.ExplainSituation:
                hudObjText.text = "Objective: Complete the Tutorial";
                objWindowText.text = "<b>Complete the Tutorial</b>\n\n" +
                    "<size=75%>Complete the tutorial and learn to play the game!\n\n";
                break;
            case TutorialStage.MoveCamera:
                hudObjText.text = "Objective: Move Nexy";
                objWindowText.text = "<b>Move Nexy</b>\n\n" +
                    "<size=75%>Learn how to move your Nexus Drone.\n\n";
                break;
            case TutorialStage.RotateCamera:
                hudObjText.text = "Objective: Rotate Nexy";
                objWindowText.text = "<b>Rotate Nexy</b>\n\n" +
                    "<size=75%>Learn how to rotate your Nexus Drone.\n\n";
                break;
            case TutorialStage.BuildGenerator:
                hudObjText.text = "Objective: Build Generator";
                objWindowText.text = "<b>Build Generator</b>\n\n" +
                    "<size=75%>Build a Power Generator to increase your available power generation.\n\n";
                break;
            case TutorialStage.BuildRelay:
                hudObjText.text = "Objective: Build Relay";
                objWindowText.text = "<b>Build Relay</b>\n\n" +
                    "<size=75%>Build a Power Relay to extend your range.\n\n";
                break;
            case TutorialStage.BuildBattery:
                hudObjText.text = "Objective: Build Battery";
                objWindowText.text = "<b>Build Battery</b>\n\n" +
                    "<size=75%>Build a Battery to increase your stored power.\n\n";
                break;
            case TutorialStage.IncreasePowerGeneration:
                hudObjText.text = "Objective: More Power Generation";
                objWindowText.text = "<b>More Power Generation</b>\n\n" +
                    $"<size=75%>Build more Power Generators and increase your power output to +{TutorialController.Instance.PowerGainGoal}.\n\n" +
                    $"Target: +{powerChange} / +{TutorialController.Instance.PowerGainGoal} <size=90%><sprite=\"all_icons\" index=0>";
                break;
            case TutorialStage.BuildHarvesters:
                hudObjText.text = "Objective: Build Harvesters";
                objWindowText.text = "<b>Build Harvesters</b>\n\n" +
                    $"<size=75%>Build {TutorialController.Instance.BuiltHarvestersGoal} Mineral Harvesters to replenish your building materials.\n\n" +
                    $"Target: {ResourceController.Instance.Harvesters.Count} / {TutorialController.Instance.BuiltHarvestersGoal} <size=90%><sprite=\"all_icons\" index=0>";
                break;
            case TutorialStage.BuildArcDefence:
                hudObjText.text = "Objective: Build Arc Defence";
                objWindowText.text = "<b>Build Arc Defence</b>\n\n" +
                    "<size=75%>Build an Arc Defence to protect yourself from the Fog!\n\n";
                break;
            case TutorialStage.BuildRepelFan:
                hudObjText.text = "Objective: Build Repel Fan";
                objWindowText.text = "<b>Build Repel Fan</b>\n\n" +
                    "<size=75%>Build a Repel Fan to protect yourself from the Fog!\n\n";
                break;
        }
    }
}
