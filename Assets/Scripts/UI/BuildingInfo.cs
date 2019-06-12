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
    [SerializeField] private Image bg;
    [HideInInspector] public Building building;
    private RectTransform parent;
    private int mineralHealth;
    private float mineralTime, mineralVal, mineral;

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
            mineralTime += Time.deltaTime;
            parent.LookAt(Camera.main.transform);
        }
    }

    private void UpdateText()
    {
        mainText.text = $"<b>{building.name}</b>\n" +
            $"HP\n";
        if (building.Powered)
        {
            mainText.text += "<color=#009900>POWERED</color>\n";
        }
        else
        {
            mainText.text += "<color=\"red\">NO POWER</color>\n";
        }

        if (building.BuildingType == BuildingType.Harvester)
        {
            if (building.Location.Resource.Health != mineral)
            {
                mineralVal = mineral;
                mineral = building.Location.Resource.Health;
                mineralTime = 0;
            }

            if (!Visible)
            {
                mineralVal = mineral;
                mainText.text += $"\n<color=#e09100><size=125%><sprite=\"all_icons\" index=2><size=100%>Remaining: {mineral}</color>";
                return;
            }
            mainText.text += $"\n<color=#e09100><size=125%><sprite=\"all_icons\" index=2><size=100%>Remaining: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))}</color>";
        }
    }

    public void ShowInfo(Building b)
    {
        building = b;
        if (parent == null)
        {
            parent = GetComponentInParent<RectTransform>();
        }
        if (building.BuildingType == BuildingType.Hub)
        {
            bg.color = new Color32(0, 92, 118, 237);
            destroyButton.gameObject.SetActive(false);
        }
        else
        {
            destroyButton.gameObject.SetActive(true);
        }
        if (building.BuildingType == BuildingType.Battery || building.BuildingType == BuildingType.Generator || building.BuildingType == BuildingType.Extender)
        {
            bg.color = new Color32(0, 166, 81, 237);
        }
        else if (building.BuildingType == BuildingType.Harvester)
        {
            bg.color = new Color32(224, 145, 0, 237);
        }
        else if (building.BuildingType == BuildingType.FogRepeller || building.BuildingType == BuildingType.AirCannon)
        {
            bg.color = new Color32(113, 66, 236, 237);
        }
        InvokeRepeating("UpdateText", 0, 0.1f);

        healthBar.SetActive(true);
        parent.position = new Vector3(b.transform.position.x, 1.5f, b.transform.position.z) + new Vector3(-0.3f, 0, -2.3f);//Camera.main.WorldToScreenPoint(b.transform.position) + new Vector3(Screen.width / 13, 0);
        parent.LookAt(Camera.main.transform);
        gameObject.SetActive(true);
        Visible = true;
    }

    public void ShowInfo(ShipComponent shipComponent)
    {
        building = null;
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

        bg.color = new Color32(0, 92, 118, 237);

        destroyButton.gameObject.SetActive(false);
        healthBar.SetActive(false);
        parent.position = new Vector3(shipComponent.transform.position.x, 0, shipComponent.transform.position.z) + new Vector3(0.5f, 1, -1.5f);//Camera.main.WorldToScreenPoint(b.transform.position) + new Vector3(Screen.width / 13, 0);
        parent.LookAt(Camera.main.transform);
        gameObject.SetActive(true);
        Visible = true;
    }

    public void HideInfo()
    {
        CancelInvoke("UpdateText");
        building = null;
        gameObject.SetActive(false);
        Visible = false;
    }
}
