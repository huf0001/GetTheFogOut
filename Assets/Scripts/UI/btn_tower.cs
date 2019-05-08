using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class btn_tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject holo_prefab;
    [SerializeField] private GameObject build_prefab;
    private Button _button;
    [SerializeField] private BuildingType towerType = BuildingType.None;
    [SerializeField] TextMeshProUGUI buildingDesc;
    [SerializeField] TextMeshProUGUI buildingCost;
    [SerializeField, Tooltip("Building flavour text"), TextArea] string descText;

    //private WorldController WC;
    public KeyCode _key;

    public GameObject Holo_prefab { get => holo_prefab; }
    public GameObject Build_prefab { get => build_prefab; }
    public BuildingType TowerType { get => towerType; }

    void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            _button.onClick.Invoke();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        switch (gameObject.name)
        {
            case "btn_battery":
                buildingDesc.text = $"<b>Battery</b>\n" + "<line-height=80% size=60%>" + descText;
                buildingCost.text = $"{build_prefab.GetComponentInChildren<Battery>().MineralCost} <sprite=\"all_icons\" index=3>";
                break;
            case "btn_generator":
                buildingDesc.text = $"<b>Power Generator</b>\n" + "<line-height=80% size=60%>" + descText;
                buildingCost.text = $"{build_prefab.GetComponentInChildren<Generator>().MineralCost} <sprite=\"all_icons\" index=3>";
                break;
            case "btn_harvester":
                buildingDesc.text = $"<b>Elemental Harvester</b>\n" + "<line-height=80% size=60%>" + descText;
                buildingCost.text = $"{build_prefab.GetComponentInChildren<Harvester>().MineralCost} <sprite=\"all_icons\" index=3>";
                break;
            case "btn_arc_defence":
                buildingDesc.text = $"<b>Arc Defence</b>\n" + "<line-height=80% size=60%>" + descText;
                buildingCost.text = $"{build_prefab.GetComponentInChildren<ArcDefence>().MineralCost} <sprite=\"all_icons\" index=3>";
                break;
            case "btn_repel_fan":
                buildingDesc.text = $"<b>Repel Fan</b>\n" + "<line-height=80% size=60%>" + descText;
                buildingCost.text = $"{build_prefab.GetComponentInChildren<RepelFan>().MineralCost} <sprite=\"all_icons\" index=3>";
                break;
            case "btn_relay":
                buildingDesc.text = $"<b>Power Relay</b>\n" + "<line-height=80% size=60%>" + descText;
                buildingCost.text = $"{build_prefab.GetComponentInChildren<Relay>().MineralCost} <sprite=\"all_icons\" index=3>";
                break;
            case "btn_remove":
                buildingDesc.text = "<b>Destroy Building</b>\n";
                break;
            default:
                buildingDesc.text = "";
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (gameObject.name)
        {
            case "btn_battery":
                buildingDesc.text = $"<b>Battery</b>\n" +
                    "<line-height=80% size=65%>" + descText + "</cspace>";
                //$"<line-height=100%>Build Cost: {build_prefab.GetComponentInChildren<Battery>().MineralCost} <sprite=\"all_icons\" index=2>\n" +
                //$"Running Cost: {build_prefab.GetComponentInChildren<Battery>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                //"Effect: +10 max storage <sprite=\"all_icons\" index=0>";
                buildingCost.text = $"{build_prefab.GetComponentInChildren<Battery>().MineralCost} <sprite=\"all_icons\" index=2>";
                break;
            case "btn_generator":
                buildingDesc.text = $"<b>Power Generator</b>\n" +
                    "<line-height=80% size=65%>" + descText;
                //$"<line-height=100%>Build Cost: {build_prefab.GetComponentInChildren<Generator>().MineralCost} <sprite=\"all_icons\" index=2>\n" +
                //$"Running Cost: +{build_prefab.GetComponentInChildren<Generator>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                //"Effect: +2 <sprite=\"all_icons\" index=0> /s";
                buildingCost.text = $"{build_prefab.GetComponentInChildren<Generator>().MineralCost} <sprite=\"all_icons\" index=2>";
                break;
            case "btn_harvester":
                buildingDesc.text = $"<b>Elemental Harvester</b>\n" +
                    "<line-height=80% size=65%>" + descText;
                //$"<line-height=100%>Build Cost: {build_prefab.GetComponentInChildren<Harvester>().MineralCost} <sprite=\"all_icons\" index=2>\n" +
                //$"Running Cost: {build_prefab.GetComponentInChildren<Harvester>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                //"Effect: +5 <sprite=\"all_icons\" index=0>/<sprite=\"all_icons\" index=1>/<sprite=\"all_icons\" index=2>/<sprite=\"all_icons\" index=2> /s";
                buildingCost.text = $"{build_prefab.GetComponentInChildren<Harvester>().MineralCost} <sprite=\"all_icons\" index=2>";
                break;
            case "btn_arc_defence":
                buildingDesc.text = $"<b>Arc Defence</b>\n" +
                    "<line-height=80% size=65%>" + descText;
                //$"<line-height=100%>Build Cost: {build_prefab.GetComponentInChildren<ArcDefence>().MineralCost} <sprite=\"all_icons\" index=2>\n" +
                //$"Running Cost: {build_prefab.GetComponentInChildren<ArcDefence>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                //"Effect: Damages fog\n" +
                //"Damage: 50 centre, 25 adjacent";
                buildingCost.text = $"{build_prefab.GetComponentInChildren<ArcDefence>().MineralCost} <sprite=\"all_icons\" index=2>";
                break;
            case "btn_repel_fan":
                buildingDesc.text = $"<b>Repel Fan</b>\n" +
                    "<line-height=80% size=65%>" + descText;
                //$"<line-height=100%>Build Cost: {build_prefab.GetComponentInChildren<RepelFan>().MineralCost} <sprite=\"all_icons\" index=2>\n" +
                //$"Running Cost: {build_prefab.GetComponentInChildren<RepelFan>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                //"Effect: Damages fog\n" +
                //"Damage: 50 centre, 25 adjacent";
                buildingCost.text = $"{build_prefab.GetComponentInChildren<RepelFan>().MineralCost} <sprite=\"all_icons\" index=2>";
                break;
            case "btn_relay":
                buildingDesc.text = $"<b>Power Relay</b>\n" +
                    "<line-height=80% size=65%>" + descText;
                //$"<line-height=100%>Build Cost: {build_prefab.GetComponentInChildren<Relay>().MineralCost} <sprite=\"all_icons\" index=2>\n" +
                //$"Running Cost: {build_prefab.GetComponentInChildren<Relay>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                //"Effect: Extends range from Hub";
                buildingCost.text = $"{build_prefab.GetComponentInChildren<Relay>().MineralCost} <sprite=\"all_icons\" index=2>";
                break;
            case "btn_remove":
                buildingDesc.text = "<b>Destroy Building</b>\n";
                break;
            default:
                buildingDesc.text = "";
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildingDesc.text = "";
        buildingCost.text = "";
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
     //   WC = FindObjectOfType<WorldController>();
    }

}
