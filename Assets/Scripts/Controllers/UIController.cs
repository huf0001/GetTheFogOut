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
    [SerializeField] Image powerImg;

    [SerializeField] Sprite[] powerLevelSprites;
    [SerializeField] TextMeshProUGUI objWindowText;

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
        hudBar = GameObject.Find("HUD");// "HudBar");
        hudBar.GetComponent<RectTransform>().DOAnchorPosY(200f, 1.5f).From(true).SetEase(Ease.OutBounce);

        //warningScript = hudBar.GetComponentInChildren<WarningScript>();
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
        // if (Input.GetButtonDown("Xbox_A"))
        // {
        //     buildingSelector.ToggleVisibility();
        // }
        UpdateResourceText();
    }

    // find sliders and text
    void FindSliders()
    {
        //powerSlider = GameObject.Find("PowerSlider").GetComponent<Slider>();
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

            int powerCheck = int.Parse(powerText.text.Split('/')[0]);

            if (powerCheck > 0 && powerCheck <= 25)
            {
                powerImg.sprite = powerLevelSprites[1];
            }
            else if (powerCheck > 25 && powerCheck <= 50)
            {
                powerImg.sprite = powerLevelSprites[2];
            }
            else if (powerCheck > 50 && powerCheck <= 75)
            {
                powerImg.sprite = powerLevelSprites[3];
            }
            else if (powerCheck > 75)
            {
                powerImg.sprite = powerLevelSprites[4];
            }
            else
            {
                powerImg.sprite = powerLevelSprites[0];
            }

            objWindowText.text = "<b>Repair the Hull</b>\n\n" +
                "<size=75%> Gather enough mineral resources to repair your ship's hull.\n\n" +
                $"Target: {resourceController.StoredMineral} / 500 <sprite=\"all_icons\" index=2>";

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
