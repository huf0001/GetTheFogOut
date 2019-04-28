using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingSelector : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buildingDesc;

    public void ToggleVisibility()
    {
        if (gameObject.activeSelf)
        {
            btnTutorial[] buttons = GetComponentsInChildren<btnTutorial>();

            foreach (btnTutorial b in buttons)
            {
                if (b.Lerping)
                {
                    b.DeactivateLerping();
                }
            }
        }

        gameObject.SetActive(!gameObject.activeSelf);
        buildingDesc.gameObject.SetActive(!buildingDesc.gameObject.activeSelf);
    }
}
