using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mainText;
    [SerializeField] Image healthBar;
    [HideInInspector] public Building building;

    public bool Visible { get; private set; }

    private void Update()
    {
        healthBar.fillAmount = building.Health / building.MaxHealth;
        //Debug.Log(building.MaxHealth);
    }

    public void ShowInfo(Building b)
    {
        building = b;
        mainText.text = $"<b>{b.BuildingType}</b>\n" +
            $"HP";
        GetComponentInParent<RectTransform>().position = new Vector3(b.transform.position.x, 0, b.transform.position.z) + new Vector3(0.5f, 1, -1.5f);//Camera.main.WorldToScreenPoint(b.transform.position) + new Vector3(Screen.width / 13, 0);
        transform.LookAt(Camera.main.transform);
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
