using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneObject : Entity
{
    GameObject buildingTooltip;

    // Start is called before the first frame update
    void Awake()
    {
        buildingTooltip = GameObject.Find("ToolTip");
    }

    public void OnMouseEnter()
    {
        buildingTooltip.SetActive(true);
        buildingTooltip.GetComponent<ToolTip>().UpdateText(this);
    }

    public void OnMouseExit()
    {
        buildingTooltip.SetActive(false);
    }
}