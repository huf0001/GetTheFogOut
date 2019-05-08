using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingSelector : MonoBehaviour
{
    //[SerializeField] TextMeshProUGUI buildingDesc;
    [SerializeField] bool visible = false;

    public bool Visible { get => visible; set => visible = value; }

    public void ToggleVisibility(TileData tile = null)
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
        else
        {
            CheckTile(tile);
        }

        gameObject.SetActive(!gameObject.activeSelf);
        visible = !visible;
        //buildingDesc.gameObject.SetActive(!buildingDesc.gameObject.activeSelf);
    }

    public void CheckTile(TileData tile)
    {
        Button[] buttons = GetComponentsInChildren<Button>();

        if (tile.Resource != null)
        {
            foreach (Button b in buttons)
            {
                if (b.gameObject.name != "btn_harvester")
                {
                    b.interactable = false;
                }
                else
                {
                    b.interactable = true;
                }
            }
        }
        else
        {
            foreach (Button b in buttons)
            {
                if (b.gameObject.name == "btn_harvester")
                {
                    b.interactable = false;
                }
                else
                {
                    b.interactable = true;
                }
            }
        }
    }
}
