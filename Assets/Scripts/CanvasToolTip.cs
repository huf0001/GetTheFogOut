using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CanvasToolTip : MonoBehaviour
{
	public GameObject ToolTip;
	public TextMeshProUGUI ToolTipText;

    void Start()
    {
        // EventTrigger trigger = GetComponent<EventTrigger>();
        // EventTrigger.Entry entry = new EventTrigger.Entry();
        // entry.eventID = EventTriggerType.PointerEnter;
        // entry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        // trigger.triggers.Add(entry);

        ToolTipText.text = "";
    	ToolTip.SetActive(false);
    }

    public void NotifyToolTip(string message)
    {
        if (message != null)
        {
            if (message == "Battery")
            {
                ToolTipText.text = "Battery\nIncrease your Maximum battery storage.\n+10 Storage, Cost: xx";
            }
            if (message == "Defence")
            {
                ToolTipText.text = "Defence\nDefence description\nXps, Cost: xx";
            }
            if (message == "Generator")
            {
                ToolTipText.text = "Generator\nIncrease the rate of generating power.\n+2 per second, Cost: xx";
            }
            if (message == "Harvester")
            {
                ToolTipText.text = "Harvester\nHarvester description\nXps, Cost: 50";
            }
            if (message == "Relay")
            {
                ToolTipText.text = "Relay\nConnect buildings to send back to base.\nXps, Cost: xx";
            }
            ToolTip.SetActive(true);
        }
        else
        {
            ToolTipText.text = "Error, unknown building message.";
        }
    }

    public void ExitToolTip()
    {
        ToolTip.SetActive(false);
        ToolTipText.text = "";
    }
}
