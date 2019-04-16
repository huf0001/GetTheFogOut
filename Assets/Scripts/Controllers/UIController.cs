using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public static UIController instance = null;

    TextMeshProUGUI powerText, organicText, mineralText, fuelText;
    public GameObject endGame, pauseGame;
    public TextMeshProUGUI endGameText;

    private Slider powerSlider, fuelSlider, organicSlider, mineralSlider;
    private int power = 0, organic = 0, mineral = 0, fuel = 0;
    private float powerVal = 0.0f, organicVal = 0.0f, mineralVal = 0.0f, fuelVal = 0.0f;
    private float powerTime = 0.0f, organicTime = 0.0f, minerTime = 0.0f, fuelTime = 0.0f;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (hub == null)
        {
            hub = WorldController.Instance.Hub;
        }

        powerTime += Time.deltaTime;
        organicTime += Time.deltaTime;
        minerTime += Time.deltaTime;
        fuelTime += Time.deltaTime;
        UpdateResourceText();
    }

    void FindSliders()
    {
        powerSlider = GameObject.Find("PowerSlider").GetComponent<Slider>();
        fuelSlider = GameObject.Find("FuelSlider").GetComponent<Slider>();
        organicSlider = GameObject.Find("OrganicSlider").GetComponent<Slider>();
        mineralSlider = GameObject.Find("MineralSlider").GetComponent<Slider>();
        powerText = powerSlider.GetComponentInChildren<TextMeshProUGUI>();
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
            if (hub.StoredPower != power)
            {
                power = hub.StoredPower;
                powerVal = powerSlider.value;
                powerTime = 0;
            }
            if (hub.StoredOrganic != organic)
            {
                organic = hub.StoredOrganic;
                organicVal = organicSlider.value;
                organicTime = 0;
            }
            if (hub.StoredMineral != mineral)
            {
                mineral = hub.StoredMineral;
                mineralVal = mineralSlider.value;
                minerTime = 0;
            }
            if (hub.StoredFuel != fuel)
            {
                fuel = hub.StoredFuel;
                fuelVal = fuelSlider.value;
                fuelTime = 0;
            }

            powerSlider.maxValue = hub.MaxPower;
            powerSlider.value = Mathf.Lerp(powerVal, power, powerTime);
            fuelSlider.value = Mathf.Lerp(fuelVal, fuel, fuelTime);
            organicSlider.value = Mathf.Lerp(organicVal, organic, organicTime);
            mineralSlider.value = Mathf.Lerp(mineralVal, mineral, minerTime);

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
                colour = "black\">";
            }

            powerText.text = Mathf.Round(Mathf.Lerp(powerVal, power, powerTime)) + "/" + hub.MaxPower + "    <color=" + colour + hub.PowerChange + "</color>";
            //organicText.text = "Organic: " + hub.StoredOrganic;
            //mineralText.text = "Minerals: " + hub.StoredMineral + " Change: " + hub.MineralChange;
            //fuelText.text = "Fuel: " + hub.StoredFuel;
        }
    }
}
