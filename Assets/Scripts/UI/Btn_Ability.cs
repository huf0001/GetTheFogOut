using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Btn_Ability : MonoBehaviour
{
    [SerializeField] private Ability abilityObject;
    [SerializeField] private TextMeshProUGUI descTextBox;
    [SerializeField] private TextMeshProUGUI costTextBox;
    [SerializeField] private string abilityName;
    [SerializeField, TextArea] private string abilityDescription;

    private int abilityCost;

    private void Start()
    {
        abilityCost = abilityObject.powerCost;
    }

    public void UpdateTextBox()
    {
        descTextBox.text = $"<b>{abilityName}</b>\n" +
            $"{abilityDescription}";
        costTextBox.text = $"{abilityCost}<sprite=\"all_icons\" index=0>";
    }
}
