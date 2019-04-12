using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public static UIController instance = null;

    TextMeshProUGUI powerText;
    TextMeshProUGUI organicText;
    TextMeshProUGUI mineralText;
    TextMeshProUGUI fuelText;

    Slider powerSlider;
    Slider fuelSlider;
    Slider organicSlider;
    Slider mineralSlider;

    int power = 0;
    float powerVal = 0;
    float pTime = 0;

    int organic = 0;
    float organicVal = 0;
    float oTime = 0;

    int mineral = 0;
    float mineralVal = 0;
    float mTime = 0;

    int fuel = 0;
    float fuelVal = 0;
    float fTime = 0;

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

        pTime += Time.deltaTime;
        oTime += Time.deltaTime;
        mTime += Time.deltaTime;
        fTime += Time.deltaTime;
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

    void UpdateResourceText()
    {
        if (hub != null)
        {
            if (hub.StoredPower != power)
            {
                power = hub.StoredPower;
                powerVal = powerSlider.value;
                pTime = 0;
            }
            if (hub.StoredOrganic != organic)
            {
                organic = hub.StoredOrganic;
                organicVal = organicSlider.value;
                oTime = 0;
            }
            if (hub.StoredMineral != mineral)
            {
                mineral = hub.StoredMineral;
                mineralVal = mineralSlider.value;
                mTime = 0;
            }
            if (hub.StoredFuel != fuel)
            {
                fuel = hub.StoredFuel;
                fuelVal = fuelSlider.value;
                fTime = 0;
            }

            powerSlider.maxValue = hub.MaxPower;
            powerSlider.value = Mathf.Lerp(powerVal, power, pTime);
            fuelSlider.value = Mathf.Lerp(fuelVal, fuel, fTime);
            organicSlider.value = Mathf.Lerp(organicVal, organic, oTime);
            mineralSlider.value = Mathf.Lerp(mineralVal, mineral, mTime);

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

            powerText.text = Mathf.Round(Mathf.Lerp(powerVal, power, pTime)) + "/" + hub.MaxPower + "    <color=" + colour + hub.PowerChange + "</color>";
            //organicText.text = "Organic: " + hub.StoredOrganic;
            //mineralText.text = "Minerals: " + hub.StoredMineral + " Change: " + hub.MineralChange;
            //fuelText.text = "Fuel: " + hub.StoredFuel;
        }
    }
}
