﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingSelector : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buildingDesc;
    [SerializeField] bool visible = false;

    public bool Visible { get => visible; set => visible = value; }

    public void ToggleVisibility()
    {
        if (gameObject.activeSelf)
        {
            btnTutorial[] buttons = GetComponentsInChildren<btnTutorial>();

            foreach (btnTutorial b in buttons)
            {
                if (b.Lerping)
                {
                    b.DeactivateLerping();
                }
            }
        }

        gameObject.SetActive(!gameObject.activeSelf);
        buildingDesc.gameObject.SetActive(!buildingDesc.gameObject.activeSelf);
        visible = !visible;
    }
}
