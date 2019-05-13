using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mainText;
    [SerializeField] Image healthBar;
    [SerializeField] Button destroyButton;
    [HideInInspector] public Building building;
    private RectTransform parent;

    public bool Visible { get; private set; }

    private void Update()
    {
        healthBar.fillAmount = building.Health / building.MaxHealth;
        if (Visible)
        {
            parent.LookAt(Camera.main.transform);
        }
    }

    public void ShowInfo(Building b)
    {
        if (parent == null)
        {
            parent = GetComponentInParent<RectTransform>();
        }
        building = b;
        if (building.BuildingType == BuildingType.Hub)
        {
            destroyButton.interactable = false;
        }
        else
        {
            destroyButton.interactable = true;
        }
        mainText.text = $"<b>{b.BuildingType}</b>\n" +
            $"HP";
        parent.position = new Vector3(b.transform.position.x, 0, b.transform.position.z) + new Vector3(0.5f, 1, -1.5f);//Camera.main.WorldToScreenPoint(b.transform.position) + new Vector3(Screen.width / 13, 0);
        parent.LookAt(Camera.main.transform);
        gameObject.SetActive(true);
        Visible = true;
    }

    public void HideInfo()
    {
        building = null;
        gameObject.SetActive(false);
        Visible = false;
    }
}
