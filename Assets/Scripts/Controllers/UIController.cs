using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public static UIController Instance { get; protected set; }

    [SerializeField] Text powerText;
    [SerializeField] Text organicText;
    [SerializeField] Text mineralText;
    [SerializeField] Text fuelText;

    Hub hub = null;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more UI managers.");
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (hub == null)
        {
            hub = FindObjectOfType<Hub>();
        }

        UpdateResourceText();
    }

    void UpdateResourceText()
    {
        if (hub != null)
        {
            powerText.text = "Power: " + hub.StoredPower + "/" 
                + hub.MaxPower + " Change: " + hub.PowerChange;
            organicText.text = "Organic: " + hub.StoredOrganic;
            mineralText.text = "Minerals: " + hub.StoredMineral
                + " Change: " + hub.MineralChange;
            fuelText.text = "Fuel: " + hub.StoredFuel;
        }
    }
}
