using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mainText;
    [SerializeField] GameObject healthBar;
    [SerializeField] Image healthBarFill;
    [SerializeField] Button destroyButton;
    [SerializeField] Gradient healthGradient;
    [HideInInspector] public Building building;
    private RectTransform parent;

    public bool Visible { get; private set; }

    private void Update()
    {
        if (healthBar.activeSelf)
        {
            healthBarFill.fillAmount = building.Health / building.MaxHealth;
            healthBarFill.color = healthGradient.Evaluate(healthBarFill.fillAmount);
        }
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
            destroyButton.gameObject.SetActive(false);
        }
        else
        {
            destroyButton.gameObject.SetActive(true);
        }

        mainText.text = $"<b>{b.BuildingType}</b>\n" +
            $"HP\n";
        if (b.Powered)
        {
            mainText.text += "<color=#009900>POWERED</color>\n";
        }
        else
        {
            mainText.text += "<color=\"red\">NO POWER</color>\n";
        }

        healthBar.SetActive(true);
        parent.position = new Vector3(b.transform.position.x, 0, b.transform.position.z) + new Vector3(0.5f, 1, -1.5f);//Camera.main.WorldToScreenPoint(b.transform.position) + new Vector3(Screen.width / 13, 0);
        parent.LookAt(Camera.main.transform);
        gameObject.SetActive(true);
        Visible = true;
    }

    public void ShowInfo(ShipComponent shipComponent)
    {
        if (parent == null)
        {
            parent = GetComponentInParent<RectTransform>();
        }
        if (shipComponent.Location.FogUnit != null)
        {
            mainText.text = "<b>It looks like your missing thruster!\n" +
                "Try clearing away the fog to collect it</b>";
        }
        else
        {
            mainText.text = "<b>It's your missing thruster!\n" +
                "Collect it so we can move on</b>";
        }

        destroyButton.gameObject.SetActive(false);
        healthBar.SetActive(false);
        parent.position = new Vector3(shipComponent.transform.position.x, 0, shipComponent.transform.position.z) + new Vector3(0.5f, 1, -1.5f);//Camera.main.WorldToScreenPoint(b.transform.position) + new Vector3(Screen.width / 13, 0);
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
