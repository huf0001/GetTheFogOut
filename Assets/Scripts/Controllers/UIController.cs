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

    private Slider powerSlider;
    private int power = 0, powerChange = 0, mineral = 0;
    private float powerVal = 0.0f, mineralVal = 0.0f;
    private float powerTime = 0.0f, mineralTime = 0.0f;
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

        DontDestroyOnLoad(gameObject);

        FindSliders();

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
                colour = "#009900>+";
            }
            else if (powerChange < 0)
            {
                colour = "\"red\">";
            }
            else
            {
                colour = "#006273>±";
            }

            // update text values
            powerText.text = Mathf.Round(Mathf.Lerp(powerVal, power, powerTime)) + "/" + resourceController.MaxPower + "\n<color=" + colour + powerChange + "</color>";

            float powerCheck = float.Parse(powerText.text.Split('/')[0]) / resourceController.MaxPower;

            if (powerCheck > 0 && powerCheck <= .25f)
            {
                powerImg.sprite = powerLevelSprites[1];
            }
            else if (powerCheck > .25f && powerCheck <= .50f)
            {
                powerImg.sprite = powerLevelSprites[2];
            }
            else if (powerCheck > .50f && powerCheck <= .75f)
            {
                powerImg.sprite = powerLevelSprites[3];
            }
            else if (powerCheck > .75f)
            {
                powerImg.sprite = powerLevelSprites[4];
            }
            else
            {
                powerImg.sprite = powerLevelSprites[0];
            }

            // old code that probably should be adapted to tween mineral stock number
            if (resourceController.StoredMineral != mineral)
            {
                mineralVal = mineral;
                mineral = resourceController.StoredMineral;
                mineralTime = 0;
            }


            mineralText.text = Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime)) + " units";
        }
    }

    public void UpdateObjectiveText(int stageNum)
    {
        switch (stageNum)
        {
            case 1:
                hudObjText.text = "Objective: Repair the Hull";
                objWindowText.text = "<b>Repair the Hull</b>\n\n" +
                    "<size=75%>Gather enough mineral resources to repair your ship's hull.\n\n" +
                    $"Target: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))} / {ObjectiveController.Instance.MineralTarget} <size=90%><sprite=\"all_icons\" index=2>";
                break;
            case 2:
                string nf = "Not Found";
                string f = "Found";
                hudObjText.text = "Objective: Recover the Thrusters";
                objWindowText.text = "<b>Recover the Thrusters</b>\n\n" +
                    "<size=75%>Push your way through the fog to find the missing thrusters from your ship.\n\n" +
                    "Thrusters: " + (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected ? f : nf);
                break;
            case 3:
                hudObjText.text = "Objective: Leave the Planet";
                objWindowText.text = "<b>Leave the Planet</b>\n\n" +
                    "<size=75%>The fog is out to get you! Hurry and gather enough power to leave this wretched planet behind!\n\n" +
                    $"Target: {Mathf.Round(Mathf.Lerp(powerVal, power, powerTime))} / {ObjectiveController.Instance.PowerTarget} <size=90%><sprite=\"all_icons\" index=0>";
                break;
        }
    }
}
