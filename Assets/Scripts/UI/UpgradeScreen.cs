using System.Collections;
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
    [Header("Building Buttons")]
    [SerializeField] private btn_tower harvesterButton;
    [SerializeField] private btn_tower mortarButton;
    [SerializeField] private btn_tower pulseDefenceButton;

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

                    switch (upgrade.pathNum)
                    {
                        case 1:
                            switch (upgrade.upgradeNum)
                            {
                                case 1:
                                    harvesterButton.UniqueStatVal = 2.5f;
                                    break;
                                case 2:
                                    harvesterButton.UniqueStatVal = 3;
                                    break;
                            }
                            break;
                        case 2:
                            switch (upgrade.upgradeNum)
                            {
                                case 1:
                                    harvesterButton.PowCostVal = -0.8f;
                                    break;
                                case 2:
                                    harvesterButton.PowCostVal = -0.5f;
                                    break;
                            }
                            break;
                    }
                    break;
                case BuildingType.AirCannon:
                    WorldController.Instance.mortarUpgradeLevel = upgrade;
                    foreach (ArcDefence mortar in ResourceController.Instance.Mortars)
                    {
                        mortar.Upgrade(upgrade);
                    }

                    switch (upgrade.pathNum)
                    {
                        case 1:
                            
                            break;
                        case 2:
                            switch (upgrade.upgradeNum)
                            {
                                case 1:
                                    mortarButton.PowCostVal = -1.6f;
                                    break;
                                case 2:
                                    mortarButton.PowCostVal = -1;
                                    break;
                            }
                            break;
                    }
                    break;
                case BuildingType.FogRepeller:
                    WorldController.Instance.pulseDefUpgradeLevel = upgrade;
                    foreach (RepelFan pulseDefence in ResourceController.Instance.PulseDefences)
                    {
                        pulseDefence.Upgrade(upgrade);
                    }

                    switch (upgrade.pathNum)
                    {
                        case 1:
                            switch (upgrade.upgradeNum)
                            {
                                case 1:
                                    pulseDefenceButton.UniqueStatVal = 3;
                                    break;
                                case 2:
                                    pulseDefenceButton.UniqueStatVal = 4;
                                    break;
                            }
                            break;
                        case 2:
                            switch (upgrade.upgradeNum)
                            {
                                case 1:
                                    pulseDefenceButton.PowCostVal = -0.8f;
                                    break;
                                case 2:
                                    pulseDefenceButton.PowCostVal = -0.5f;
                                    break;
                            }
                            break;
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
