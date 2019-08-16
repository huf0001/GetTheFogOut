﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeScreen : MonoBehaviour
{
    [Header("Button Colours")]
    [SerializeField] private Color upgradedButton;
    [SerializeField] private Color upgradedLine;
    [SerializeField] private Color unupgradedLine;
    [SerializeField, Tooltip("Alpha value for other tree")] private float treeUnavailable;

    public void PerformUpgrade()
    {
        // TODO: add functionality to actually do upgrades
    }

    public void ChangeCurButtonColour(Image image)
    {
        image.color = upgradedButton;
    }

    public void ChangeOtherTreeColour(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = treeUnavailable;
        canvasGroup.interactable = false;
    }

    public void ChangeLineColour(Image image)
    {
        image.color = upgradedLine;
    }

    public void ChangeOtherTreeLineColour(Image image)
    {
        image.color = unupgradedLine;
    }

    public void UnlockNextUpgrade(Button button)
    {
        button.interactable = true;
    }
}
