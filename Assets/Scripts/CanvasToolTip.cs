using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CanvasToolTip : MonoBehaviour
{
	public GameObject ToolTip;
	public Text ToolTipText;

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
                ToolTipText.text = "Battery\nBattery description\nXps";
            }
            if (message == "Defence")
            {
                ToolTipText.text = "Defence\nDefence description\nXps";
            }
            if (message == "Generator")
            {
                ToolTipText.text = "Generator\nGenerator description\nXps";
            }
            if (message == "Harvester")
            {
                ToolTipText.text = "Harvester\nHarvester description\nXps";
            }
            if (message == "Relay")
            {
                ToolTipText.text = "Relay\nRelay description\nXps";
            }
            ToolTip.SetActive(true);
        }
        else
        {
            ToolTipText.text = "Error, unknown message, ZIPPIDEE DOO.";
            return;
        }
    }

    public void ExitToolTip()
    {
        ToolTip.SetActive(false);
        ToolTipText.text = "";
    }
}
