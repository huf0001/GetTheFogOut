﻿using System.Collections;
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
            powerSlider.maxValue = Mathf.Lerp(powerSlider.maxValue, hub.MaxPower, 1f * Time.deltaTime);
            powerSlider.value = Mathf.Lerp(powerSlider.value, hub.StoredPower, 1f * Time.deltaTime);
            fuelSlider.value = Mathf.Lerp(fuelSlider.value, hub.StoredFuel, 1f * Time.deltaTime);
            organicSlider.value = Mathf.Lerp(organicSlider.value, hub.StoredOrganic, 1f * Time.deltaTime);
            mineralSlider.value = Mathf.Lerp(mineralSlider.value, hub.StoredMineral, 1f * Time.deltaTime);

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

            powerText.text = Mathf.Round(Mathf.Lerp(powerSlider.value, hub.StoredPower, 1f * Time.deltaTime)) + "/" + hub.MaxPower + "    <color=" + colour + hub.PowerChange + "</color>";
            //organicText.text = "Organic: " + hub.StoredOrganic;
            //mineralText.text = "Minerals: " + hub.StoredMineral + " Change: " + hub.MineralChange;
            //fuelText.text = "Fuel: " + hub.StoredFuel;
        }
    }
}
