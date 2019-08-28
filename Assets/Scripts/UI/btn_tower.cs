using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class btn_tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject holo_prefab;
    [SerializeField] private GameObject build_prefab;
    [SerializeField] private BuildingType towerType = BuildingType.None;
    [SerializeField] private RectTransform buttonBG;
    [SerializeField] private Image buildingDescBG;
    [SerializeField] TextMeshProUGUI buildingDesc;
    [SerializeField] TextMeshProUGUI buildingCost;
    [SerializeField] string buildingName;
    [SerializeField, Tooltip("Building flavour text"), TextArea] string descText;
    [Header("Key Info")]
    [SerializeField] private CanvasGroup keyInfo;
    [SerializeField] private TextMeshProUGUI minCost;
    [SerializeField] private TextMeshProUGUI powCost;
    [SerializeField] private TextMeshProUGUI uniqueStat;

    private Button button;
    private Color buttonColour;
    private int minCostVal;
    private float powCostVal;
    private float uniqueStatVal;

    public GameObject Holo_prefab { get => holo_prefab; }
    public GameObject Build_prefab { get => build_prefab; }
    public BuildingType TowerType { get => towerType; }
    public float PowCostVal
    {
        get => powCostVal;
        set
        {
            powCostVal = value;
            powCost.text = $"<size=250%><sprite=\"all_icons\" index=0>\n<size=120%>{(powCostVal > 0 ? "+" : "")}{powCostVal} %/s";
        }
    }
    public float UniqueStatVal
    {
        get => uniqueStatVal;
        set
        {
            uniqueStatVal = value;
        }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonColour = buttonBG.GetComponent<Image>().color;
        minCostVal = build_prefab.GetComponentInChildren<Building>().MineralCost;
        powCostVal = build_prefab.GetComponentInChildren<Building>().Upkeep;
        minCost.text = $"<size=250%><sprite=\"all_icons\" index=2>\n<size=120%>{minCostVal}";
        powCost.text = $"<size=250%><sprite=\"all_icons\" index=0>\n<size=120%>{(powCostVal > 0 ? "+" : "")}{powCostVal} %/s";
    }

    public void ChangeDescColour()
    {
        buildingDescBG.color = buttonColour;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
        {
            if (gameObject.name != "btn_generator")
            {
                buildingDesc.text = $"<b>{buildingName}</b>\n" +
                    "<line-height=80% size=65%>" + descText;
                buildingCost.text = $"{minCostVal} <sprite=\"all_icons\" index=2>";
            }
            else
            {
                buildingDesc.text = $"<b>{buildingName}</b> {ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit}  +{powCostVal} %/s<sprite=\"all_icons\" index=0>\n" +
                    "<line-height=80% size=65%>" + descText;
                buildingCost.text = $"{minCostVal} <sprite=\"all_icons\" index=2>";
            }
        }
        else if (gameObject.name == "btn_generator" && ResourceController.Instance.Generators.Count == ObjectiveController.Instance.GeneratorLimit)
        {
            buildingDesc.text = $"<b>Power Generator</b> {ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit}\n" +
                $"<line-height=80% size=65%>You have the max number of generators.";
        }
        buttonBG.DOSizeDelta(new Vector2(170, 170), 0.2f).OnComplete(
            delegate
            {
                keyInfo.alpha = 1;
            });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DOTween.Kill(buttonBG);
        buildingDesc.text = "";
        buildingCost.text = "";
        keyInfo.alpha = 0;
        buttonBG.DOSizeDelta(new Vector2(75, 75), 0.2f);
    }
}
