﻿using System;
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
    [SerializeField] private GameObject buttonHighlight;
    [SerializeField] private Image buildingDescBG;
    [SerializeField] TextMeshProUGUI buildingDesc;
    [SerializeField] TextMeshProUGUI buildingCost;
    [SerializeField] string buildingName;
    [SerializeField, Tooltip("Building flavour text"), TextArea] string descText;
    [Header("Key Info")]
    [SerializeField] private CanvasGroup keyInfo;
    [SerializeField] private TextMeshProUGUI buildName;
    [SerializeField] private TextMeshProUGUI minCost;
    [SerializeField] private TextMeshProUGUI powCost;
    [SerializeField] private TextMeshProUGUI uniqueStat;

    private Button button;
    private Color buttonColour;
    private int minCostVal;
    private float powCostVal;
    private float uniqueStatVal;
    private bool lerping;

    public Button Button { get => button; set => button = value; }
    public GameObject Holo_prefab { get => holo_prefab; }
    public GameObject Build_prefab { get => build_prefab; }
    public BuildingType TowerType { get => towerType; }
    public float PowCostVal
    {
        get => powCostVal;
        set
        {
            powCostVal = value;
            powCost.text = $"<size=250%><sprite=\"all_icons\" index=0>\n<size=120%>{(powCostVal > 0 ? "+" : "") + powCostVal.ToString("F1")}\n%/s";
        }
    }
    public float UniqueStatVal
    {
        get => uniqueStatVal;
        set
        {
            uniqueStatVal = value;

            switch (gameObject.name)
            {
                case "btn_arc_defence":
                case "btn_repel_fan":
                case "btn_relay":
                    uniqueStat.text = $"Range: {uniqueStatVal} tiles";
                    break;
                case "btn_harvester":
                    uniqueStat.text = $"+{uniqueStatVal.ToString("F1")} <size=150%><sprite=\"all_icons\" index=2><size=100%>/s";
                    break;
            }
        }
    }

    private void Start()
    {
        if (gameObject.name == "btn_remove" || gameObject.name == "RemoveIcon") return;
        Button = GetComponent<Button>();

        buttonColour = buttonBG.GetComponent<Image>().color;
        minCostVal = build_prefab.GetComponentInChildren<Building>().MineralCost;
        powCostVal = build_prefab.GetComponentInChildren<Building>().Upkeep;

        switch (gameObject.name)
        {
            case "btn_arc_defence":
                uniqueStatVal = 5;
                uniqueStat.text = $"Range: {uniqueStatVal} tiles";
                break;
            case "btn_repel_fan":
                uniqueStatVal = 2;
                uniqueStat.text = $"Range: {uniqueStatVal} tiles";
                break;
            case "btn_relay":
                uniqueStatVal = 5;
                uniqueStat.text = $"Range: {uniqueStatVal} tiles";
                break;
            case "btn_harvester":
                uniqueStatVal = 2;
                uniqueStat.text = $"+{uniqueStatVal.ToString("F1")} <sprite=\"all_icons\" index=2>/s";
                break;
            case "btn_generator":
                uniqueStat.text = $"{ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit} Built";
                break;
        }

        buildName.text = buildingName;
        minCost.text = $"<size=250%><sprite=\"all_icons\" index=2>\n<size=120%>{minCostVal}";
        powCost.text = $"<size=250%><sprite=\"all_icons\" index=0>\n<size=120%>{(powCostVal > 0 ? "+" : "") + powCostVal.ToString("F1")}\n%/s";
    }

    private void Update()
    {
        if (gameObject.name != "btn_remove" && gameObject.name != "RemoveIcon" && keyInfo.alpha == 1 && buildingCost.color != Color.white && minCostVal < ResourceController.Instance.StoredMineral)
        {
            buildingCost.color = Color.white;
            minCost.color = Color.white;
        }
    }

    public void ChangeDescColour()
    {
        buildingDescBG.color = buttonColour;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-UI_Move", GetComponent<Transform>().position);
        if (gameObject.name != "btn_remove" && gameObject.name != "RemoveIcon")
        {
            if (Button.interactable)
            {
                // Update description text
                if (gameObject.name != "btn_generator")
                {
                    buildingDesc.text = $"<b>{buildingName}</b>   {powCostVal} %/s<sprite=\"all_icons\" index=0>\n" +
                        "<line-height=80% size=65%>" + descText;
                }
                else
                {
                    buildingDesc.text = $"<b>{buildingName}</b> {ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit}  +{powCostVal} %/s<sprite=\"all_icons\" index=0>\n" +
                        "<line-height=80% size=65%>" + descText;
                    uniqueStat.text = $"{ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit} Built";
                }
            }
            else if (gameObject.name == "btn_generator" && ResourceController.Instance.Generators.Count == ObjectiveController.Instance.GeneratorLimit)
            {
                // Make it known player can't build more generators
                buildingDesc.text = $"<b>{buildingName}</b> {ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit}  +{powCostVal} %/s<sprite=\"all_icons\" index=0>\n" +
                    $"<line-height=80% size=65%>You have the max number of generators.";
                uniqueStat.text = $"{ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit} Built";
            }
            else
            {
                // Tell player they can't place that building there
                buildingDesc.text = $"<b>{buildingName}</b>   {powCostVal} %/s<sprite=\"all_icons\" index=0>\n" +
                        "<line-height=80% size=65%>This building cannot be placed here";
            }

            buildingCost.text = $"{minCostVal} <sprite=\"all_icons\" index=2>";

            if (minCostVal > ResourceController.Instance.StoredMineral)
            {
                buildingCost.color = Color.red;
                minCost.color = Color.red;
            }
            else
            {
                buildingCost.color = Color.white;
                minCost.color = Color.white;
            }

            if (buttonHighlight != null && buttonHighlight.activeSelf)
            {
                buttonHighlight.SetActive(false);
                lerping = true;
            }

            buttonBG.DOSizeDelta(new Vector2(170, 170), 0.2f).OnComplete(
                delegate
                {
                    keyInfo.alpha = 1;
                });
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gameObject.name != "btn_remove" && gameObject.name != "RemoveIcon")
        {
            DOTween.Kill(buttonBG);
            buildingDesc.text = "";
            buildingCost.text = "";
            keyInfo.alpha = 0;
            buttonBG.DOSizeDelta(new Vector2(75, 75), 0.2f).OnComplete(
                delegate
                {
                    if (lerping)
                    {
                        buttonHighlight.SetActive(true);
                        lerping = false;
                    }
                });
        }
    }
}
