using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneObject : Entity
{
    ToolTip buildingTooltip;
    
    protected void FindToolTip()
    {
        buildingTooltip = GameObject.Find("Canvas").GetComponentInChildren<ToolTip>(true);
    }

    public void OnMouseEnter()
    {
        if (!WorldController.Instance.InBuildMode && !EventSystem.current.IsPointerOverGameObject())
        {
            buildingTooltip.gameObject.SetActive(true);
            buildingTooltip.UpdateText(this);
        }
    }

    public void OnMouseExit()
    {
        buildingTooltip.gameObject.SetActive(false);
    }
}