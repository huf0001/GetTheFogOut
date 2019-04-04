using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public static UIController instance = null;

    [SerializeField] TextMeshProUGUI powerText;
    [SerializeField] TextMeshProUGUI organicText;
    [SerializeField] TextMeshProUGUI mineralText;
    [SerializeField] TextMeshProUGUI fuelText;
    [SerializeField] Slider powerSlider;
    [SerializeField] Slider fuelSlider;
    [SerializeField] Slider organicSlider;
    [SerializeField] Slider mineralSlider;

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

    void UpdateResourceText()
    {
        if (hub != null)
        {
            powerText.text = hub.StoredPower + "/" + hub.MaxPower; //+ hub.PowerChange;
            //organicText.text = "Organic: " + hub.StoredOrganic;
            //mineralText.text = "Minerals: " + hub.StoredMineral + " Change: " + hub.MineralChange;
            //fuelText.text = "Fuel: " + hub.StoredFuel;
        }

        if (hub != null)
        {
            powerSlider.maxValue = hub.MaxPower;
            powerSlider.value = hub.StoredPower;
            fuelSlider.value = hub.StoredFuel;
            organicSlider.value = hub.StoredOrganic;
            mineralSlider.value = hub.StoredMineral;
        }
    }
}
