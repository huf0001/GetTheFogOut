using System.Collections;
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

    public void PerformUpgrade(Upgrade upgrade)
    {
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

    /// <summary>
    /// Make next button interactable
    /// </summary>
    /// <param name="button"></param>
    public void UnlockNextUpgrade(Button button)
    {
        button.interactable = true;
        button.GetComponentsInChildren<Image>()[2].color = new Color32(4, 80, 117, 255);
    }
}
