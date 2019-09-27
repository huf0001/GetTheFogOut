using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Btn_Ability : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Ability abilityObject;
    [SerializeField] private TextMeshProUGUI descTextBox;
    [SerializeField] private TextMeshProUGUI costTextBox;
    [SerializeField] private string abilityName;
    [SerializeField, TextArea] private string abilityDescription;

    private const string NOT_ENOUGH = "#ff0000";
    private const string ENOUGH = "#ffffff";
    private int abilityCost;

    private void Start()
    {
        abilityCost = abilityObject.powerCost;
    }

    public void UpdateTextBox()
    {
        descTextBox.text = $"<b>{abilityName}</b>\n" +
            $"{(AbilityController.Instance.AbilityCollected[abilityObject.AbilityType] ? abilityDescription : "Missing Module. It must have fallen somewhere else.")}";
        costTextBox.text = $"<color={(abilityCost < ResourceController.Instance.StoredPower ? ENOUGH : NOT_ENOUGH)}>{abilityCost}<sprite=\"all_icons\" index=0>";
    }

    public void CheckPowerLevel()
    {
        if (abilityCost > ResourceController.Instance.StoredPower)
        {
            descTextBox.text = $"<b>{abilityName}</b>\n" +
                "Not enough power!";

            CancelInvoke(nameof(UpdateTextBox));
            Invoke(nameof(UpdateTextBox), 3);
        }
        else
        {
            AbilityController.Instance.OnButtonClicked(abilityObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-UI_Move", GetComponent<Transform>().position);
    }
}
