using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class btn_tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject obj_prefab;
    public GameObject Obj_prefab { get => obj_prefab; }

    [SerializeField]
    private Button _button;
    private WorldController WC;
    public KeyCode _key;

    [SerializeField] TextMeshProUGUI buildingDesc;

    void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            _button.onClick.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (gameObject.name)
        {
            case "btn_battery":
                buildingDesc.text = "<b>Battery</b>\n" +
                    "Build Cost: 30 <sprite=\"all_icons\" index=0>\n" +
                    "Running Cost: 0\n" +
                    "Effect: +10 max storage <sprite=\"all_icons\" index=0>";
                break;
            case "btn_generator":
                buildingDesc.text = "<b>Power Generator</b>\n" +
                    "Build Cost: 30 <sprite=\"all_icons\" index=0>\n" +
                    "Running Cost: 0\n" +
                    "Effect: +2 <sprite=\"all_icons\" index=0> /s";
                break;
            case "btn_harvester":
                buildingDesc.text = "<b>Elemental Harvester</b>\n" +
                    "Build Cost: 50 <sprite=\"all_icons\" index=0>\n" +
                    "Running Cost: 5 <sprite=\"all_icons\" index=0> /s\n" +
                    "Effect: +5 <sprite=\"all_icons\" index=0>/<sprite=\"all_icons\" index=1>/<sprite=\"all_icons\" index=2>/<sprite=\"all_icons\" index=3> /s";
                break;
            case "btn_defence":
                buildingDesc.text = "<b>Cluster Fan</b>\n" +
                    "Build Cost: 50 <sprite=\"all_icons\" index=0>\n" +
                    "Running Cost: 5 <sprite=\"all_icons\" index=0> /s\n" +
                    "Effect: Damages fog\n" +
                    "Damage: 50 centre, 25 adjacent";
                break;
            case "btn_relay":
                buildingDesc.text = "<b>Power Relay</b>\n" +
                    "Build Cost: 10 <sprite=\"all_icons\" index=0>\n" +
                    "Running Cost: 0\n" +
                    "Effect: Extends range from Hub";
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
        WC = FindObjectOfType<WorldController>();
    }

}
