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
    public TextMeshProUGUI endGameText;

    private Slider powerSlider;
    private int power = 0, mineral = 0;
    private float powerVal = 0.0f, mineralVal = 0.0f;
    private float powerTime = 0.0f, mineralTime = 0.0f;

    Hub hub = null;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (hub == null)
        {
            hub = WorldController.Instance.Hub;
        }

        powerTime += Time.deltaTime;
        mineralTime += Time.deltaTime;
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
        if (hub != null)
        {
            // if the stored power is different, change values used for lerping
            if (hub.StoredPower != power)
            {
                power = hub.StoredPower;
                powerVal = powerSlider.value;
                powerTime = 0;
            }

            // change colour of power change text depending on +, - or ±
            string colour;
            if (hub.PowerChange > 0)
            {
                colour = "#009900>+";
            }
            else if (hub.PowerChange < 0)
            {
                colour = "red\">";
            }
            else
            {
                colour = "black\">±";
            }

            // update slider and text values
            powerSlider.maxValue = hub.MaxPower;
            powerSlider.value = Mathf.Lerp(powerVal, power, powerTime);
            powerText.text = Mathf.Round(Mathf.Lerp(powerVal, power, powerTime)) + "/" + hub.MaxPower + "    <color=" + colour + hub.PowerChange + "</color>";
            mineralText.text = hub.StoredMineral + " units";

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
