﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Switch;
using UnityEngine.UI;

public class UpgradeScreen : MonoBehaviour
{
    [Header("Button Colours")]
    [SerializeField] private Color upgradedButton;
    [SerializeField] private Color upgradedLine;
    [SerializeField] private Color unupgradedLine;
    [SerializeField, Tooltip("Alpha value for other tree")] private float treeUnavailable;

    private Image buttonImg, lineImg, otherLineImg;
    private CanvasGroup canvas;
    private Button nextButton;

    public void PerformUpgrade(Upgrade upgrade)
    {
        if (ResourceController.Instance.StoredMineral >= upgrade.cost)
        {
            ResourceController.Instance.StoredMineral -= upgrade.cost;
            switch (upgrade.buildingType)
            {
                case BuildingType.Harvester:
                    WorldController.Instance.hvstUpgradeLevel = upgrade;
                    foreach (Harvester harvester in ResourceController.Instance.Harvesters)
                    {
                        harvester.Upgrade(upgrade);
                    }
                    break;
                case BuildingType.AirCannon:
                    WorldController.Instance.mortarUpgradeLevel = upgrade;
                    foreach (ArcDefence mortar in ResourceController.Instance.Mortars)
                    {
                        mortar.Upgrade(upgrade);
                    }
                    break;
                case BuildingType.FogRepeller:
                    WorldController.Instance.pulseDefUpgradeLevel = upgrade;
                    foreach (RepelFan pulseDefence in ResourceController.Instance.PulseDefences)
                    {
                        pulseDefence.Upgrade(upgrade);
                    }
                    break;
            }

            switch (upgrade.upgradeNum)
            {
                case 1:
                    buttonImg.color = upgradedButton;
                    canvas.alpha = treeUnavailable;
                    canvas.interactable = false;
                    otherLineImg.color = unupgradedLine;
                    nextButton.interactable = true;
                    nextButton.GetComponentsInChildren<Image>()[2].color = new Color32(4, 80, 117, 255);
                    lineImg.color = upgradedLine;
                    break;
                case 2:
                    buttonImg.color = upgradedButton;
                    break;
            }
        }
    }

    public void ChangeCurButtonColour(Image image)
    {
        buttonImg = image;
    }

    public void ChangeOtherTreeColour(CanvasGroup canvasGroup)
    {
        canvas = canvasGroup;
    }

    public void ChangeLineColour(Image image)
    {
        lineImg = image;
    }

    public void ChangeOtherTreeLineColour(Image image)
    {
        otherLineImg = image;
    }

    /// <summary>
    /// Make next button interactable
    /// </summary>
    /// <param name="button"></param>
    public void UnlockNextUpgrade(Button button)
    {
        nextButton = button;
    }
}
