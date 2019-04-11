using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingSelector : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buildingDesc;

    public void ToggleVisibility()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        buildingDesc.gameObject.SetActive(!buildingDesc.gameObject.activeSelf);
    }
}
