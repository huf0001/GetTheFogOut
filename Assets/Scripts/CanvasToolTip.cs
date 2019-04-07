using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CanvasToolTip : MonoBehaviour
{
	public GameObject ToolTip;
	public GameObject[] Buttons;
	
	private Text ToolTipText;
	private bool IsVisible;
    void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        trigger.triggers.Add(entry);

    	ToolTip.SetActive(false);
        ToolTipText.text = "";
        IsVisible = false;
    }

    public void OnPointerEnter(PointerEventData data)
    {
        // Code to check the value, update string to show.
        //
        // Battery
        // ToolTipText.text = "Battery\nBattery description\nXps";
        //
        // Defence
        // ToolTipText.text = "Defence\nDefence description\nXps";
        //
        // Generator
        // ToolTipText.text = "Generator\nGenerator description\nXps";
        //
        // Harvester
        // ToolTipText.text = "Harvester\nHarvester description\nXps";
        //
        // Relay
        // ToolTipText.text = "Relay\nRelay description\nXps";

    	ToolTip.SetActive(true);
        Debug.Log("OnPointerEnter called.");

    }

    void Update()
    {
        if (IsVisible)
        {
            ToolTip.SetActive(true);
        }
        else
        {
            ToolTip.SetActive(false);
        }
    }
}
