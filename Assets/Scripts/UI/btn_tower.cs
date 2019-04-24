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
    [SerializeField] private Button _button;
    [SerializeField] private BuildingType towerType = BuildingType.None;
    [SerializeField] TextMeshProUGUI buildingDesc;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hub hub = WorldController.Instance.Hub;

        switch (gameObject.name)
        {
            case "btn_battery":
                buildingDesc.text = "<b>Battery</b>\n" +
                    $"Build Cost: {hub.BuildingsCosts[BuildingType.Battery]["mineral"]} <sprite=\"all_icons\" index=3>\n" +
                    $"Running Cost: {build_prefab.GetComponentInChildren<Battery>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                    "Effect: +10 max storage <sprite=\"all_icons\" index=0>";
                break;
            case "btn_generator":
                buildingDesc.text = "<b>Power Generator</b>\n" +
                    $"Build Cost: {hub.BuildingsCosts[BuildingType.Generator]["mineral"]} <sprite=\"all_icons\" index=3>\n" +
                    $"Running Cost: +{build_prefab.GetComponentInChildren<Generator>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                    "Effect: +2 <sprite=\"all_icons\" index=0> /s";
                break;
            case "btn_harvester":
                buildingDesc.text = "<b>Elemental Harvester</b>\n" +
                    $"Build Cost: {hub.BuildingsCosts[BuildingType.Harvester]["mineral"]} <sprite=\"all_icons\" index=3>\n" +
                    $"Running Cost: {build_prefab.GetComponentInChildren<Harvester>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                    "Effect: +5 <sprite=\"all_icons\" index=0>/<sprite=\"all_icons\" index=1>/<sprite=\"all_icons\" index=2>/<sprite=\"all_icons\" index=3> /s";
                break;
            case "btn_defence":
                buildingDesc.text = "<b>Cluster Fan</b>\n" +
                    $"Build Cost: {hub.BuildingsCosts[BuildingType.Defence]["mineral"]} <sprite=\"all_icons\" index=3>\n" +
                    $"Running Cost: {build_prefab.GetComponentInChildren<Defence>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                    "Effect: Damages fog\n" +
                    "Damage: 50 centre, 25 adjacent";
                break;
            case "btn_relay":
                buildingDesc.text = "<b>Power Relay</b>\n" +
                    $"Build Cost: {hub.BuildingsCosts[BuildingType.Relay]["mineral"]} <sprite=\"all_icons\" index=3>\n" +
                    $"Running Cost: {build_prefab.GetComponentInChildren<Relay>().Upkeep} <sprite=\"all_icons\" index=0> /s\n" +
                    "Effect: Extends range from Hub";
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
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
     //   WC = FindObjectOfType<WorldController>();
    }

}
