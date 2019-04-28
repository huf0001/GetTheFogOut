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
    public TextMeshProUGUI endGameText;

    private Slider powerSlider;
    private int power = 0, powerChange = 0, mineral = 0;
    private float powerVal = 0.0f, mineralVal = 0.0f;
    private float powerTime = 0.0f, mineralTime = 0.0f;

    ResourceController resourceController = null;

    //WarningScript warningScript;

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
        hudBar = GameObject.Find("HudBar");
        hudBar.GetComponent<RectTransform>().DOAnchorPosY(200f, 1.5f).From(true).SetEase(Ease.OutBounce);

        //warningScript = hudBar.GetComponentInChildren<WarningScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (resourceController == null)
        {
            resourceController = WorldController.Instance.ResourceController;
        }

        powerTime += Time.deltaTime;
        mineralTime += Time.deltaTime;
        if (Input.GetButtonDown("Xbox_LB"))
        {
            buildingSelector.ToggleVisibility();
        }
        UpdateResourceText();
    }

    // find sliders and text
    void FindSliders()
    {
        powerSlider = GameObject.Find("PowerSlider").GetComponent<Slider>();
        powerText = powerSlider.GetComponentInChildren<TextMeshProUGUI>();
        mineralText = GameObject.Find("MineralSlider").GetComponent<Slider>().GetComponentInChildren<TextMeshProUGUI>();
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
                power = resourceController.StoredPower;
                powerVal = powerSlider.value;
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
                //warningScript.AddWarning("Power grid is overloaded!", WarningScript.WarningLevel.Danger);
            }
            else
            {
                colour = "\"black\">±";
                //warningScript.AddWarning("Power grid is at maximum capacity!", WarningScript.WarningLevel.Warning);
            }

            // update slider and text values
            powerSlider.maxValue = resourceController.MaxPower;
            powerSlider.value = Mathf.Lerp(powerVal, power, powerTime);
            powerText.text = Mathf.Round(Mathf.Lerp(powerVal, power, powerTime)) + "/" + resourceController.MaxPower + "    <color=" + colour + powerChange + "</color>";
            mineralText.text = resourceController.StoredMineral + " units";

            // old code that probably should be adapted to tween mineral stock number
            //if (hub.StoredMineral != mineral)
            //{
            //    mineral = hub.StoredMineral;
            //    mineralVal = mineralSlider.value;
            //    mineralTime = 0;
            //}
        }
    }
}
