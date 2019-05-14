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
    private Button button;
    [SerializeField] private BuildingType towerType = BuildingType.None;
    [SerializeField] TextMeshProUGUI buildingDesc;
    [SerializeField] TextMeshProUGUI buildingCost;
    [SerializeField, Tooltip("Building flavour text"), TextArea] string descText;

    //private WorldController WC;
    public KeyCode _key;

    public GameObject Holo_prefab { get => holo_prefab; }
    public GameObject Build_prefab { get => build_prefab; }
    public BuildingType TowerType { get => towerType; }

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            button.onClick.Invoke();
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
        if (button.interactable)
        {
            switch (gameObject.name)
            {
                case "btn_battery":
                    buildingDesc.text = $"<b>Battery</b>\n" +
                        "<line-height=80% size=65%>" + descText;
                    buildingCost.text = $"{build_prefab.GetComponentInChildren<Battery>().MineralCost} <sprite=\"all_icons\" index=2>";
                    break;
                case "btn_generator":
                    buildingDesc.text = $"<b>Power Generator</b> {ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit}\n" +
                        "<line-height=80% size=65%>" + descText;
                    buildingCost.text = $"{build_prefab.GetComponentInChildren<Generator>().MineralCost} <sprite=\"all_icons\" index=2>";
                    break;
                case "btn_harvester":
                    buildingDesc.text = $"<b>Elemental Harvester</b>\n" +
                        "<line-height=80% size=65%>" + descText;
                    buildingCost.text = $"{build_prefab.GetComponentInChildren<Harvester>().MineralCost} <sprite=\"all_icons\" index=2>";
                    break;
                case "btn_arc_defence":
                    buildingDesc.text = $"<b>Arc Defence</b>\n" +
                        "<line-height=80% size=65%>" + descText;
                    buildingCost.text = $"{build_prefab.GetComponentInChildren<ArcDefence>().MineralCost} <sprite=\"all_icons\" index=2>";
                    break;
                case "btn_repel_fan":
                    buildingDesc.text = $"<b>Repel Fan</b>\n" +
                        "<line-height=80% size=65%>" + descText;
                    buildingCost.text = $"{build_prefab.GetComponentInChildren<RepelFan>().MineralCost} <sprite=\"all_icons\" index=2>";
                    break;
                case "btn_relay":
                    buildingDesc.text = $"<b>Power Relay</b>\n" +
                        "<line-height=80% size=65%>" + descText;
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
        else if (gameObject.name == "btn_generator")
        {
            buildingDesc.text = $"<b>Power Generator</b> {ResourceController.Instance.Generators.Count}/{ObjectiveController.Instance.GeneratorLimit}\n" +
                $"<line-height=80% size=65%>You have the max number of generators.";
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildingDesc.text = "";
        buildingCost.text = "";
    }

    public void OnClick()
    {
        buildingDesc.text = "";
        buildingCost.text = "";
    }
}
