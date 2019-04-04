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
            powerSlider.maxValue = hub.MaxPower;
            powerSlider.value = hub.StoredPower;
            fuelSlider.value = hub.StoredFuel;
            organicSlider.value = hub.StoredOrganic;
            mineralSlider.value = hub.StoredMineral;

            powerText.text = hub.StoredPower + "/" + hub.MaxPower; //+ hub.PowerChange;
            //organicText.text = "Organic: " + hub.StoredOrganic;
            //mineralText.text = "Minerals: " + hub.StoredMineral + " Change: " + hub.MineralChange;
            //fuelText.text = "Fuel: " + hub.StoredFuel;
        }
    }
}
