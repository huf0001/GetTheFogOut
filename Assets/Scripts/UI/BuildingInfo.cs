using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;
using DG.Tweening;

public class BuildingInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private GameObject healthBar;
    [SerializeField, FormerlySerializedAs("healthBarFill")] private Image healthBarMask;
    [SerializeField] private Image healthBarFill;
    [SerializeField, GradientUsage(true)] private Gradient healthGradient;
    [SerializeField, ColorUsage(true, true)] private Color redHDR;
    [SerializeField] private TextMeshProUGUI refundText;
    [SerializeField] private Button destroyButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image bg;

    [HideInInspector] public Building building;

    private ShipComponent shipComp;
    private int mineralHealth, refundCost;
    private float mineralTime, mineralVal, mineral;
    private Transform Range;
    private Camera cam;

    public bool Visible { get; private set; }

    private void Update()
    {
        if (healthBar.activeSelf)
        {
            healthBarMask.fillAmount = building.Health / building.MaxHealth;
            if (healthBarMask.fillAmount > 0.5)
            {
                DOTween.Kill(healthBarFill);
                healthBarFill.color = healthGradient.Evaluate(healthBarMask.fillAmount);
            }
            else if (!DOTween.IsTweening(healthBarFill)) healthBarFill.DOColor(redHDR, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        if (Visible)
        {
            if (building != null)
            {
                mineralTime += Time.deltaTime;
                transform.position = cam.WorldToScreenPoint(building.transform.position) + new Vector3(Screen.width / 13, 0);
            }
            //else
            //{
            //    transform.position = cam.WorldToScreenPoint(shipComp.transform.position) + new Vector3(Screen.width / 13, 0);
            //}
        }
    }

    private void UpdateText()
    {
        // General text updates
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

        // Update mineral health value
        if (building.BuildingType == BuildingType.Harvester)
        {
            mineralVal = building.Location.Resource.Health + Mathf.Round(ResourceController.Instance.MineralChange/2);
            if (building.Location.Resource.Health != mineral)
            {
                mineralVal = mineral;
                mineral = building.Location.Resource.Health;
                mineralTime = 0;
            }
            if (mineral <= 0)
            {
                mineral = 0;
                mineralVal = 0;
            }
            if (!Visible)
            {
                mineralVal = mineral;
                mainText.text += $"\n<color=#e09100><size=125%><sprite=\"all_icons\" index=2><size=100%>Remaining: {mineral}</color>";
                return;
            }
            mainText.text += $"\n<color=#e09100><size=125%><sprite=\"all_icons\" index=2><size=100%>Remaining: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))}</color>";
        }

        // Update refund cost
        if (building.BuildingType != BuildingType.Hub)
        {
            int returnCost = building.MineralCost;
            if (building.BuildingType != BuildingType.Extender)
            {
                if (building.Health != building.MaxHealth)
                {
                    if (building.Health < 40 && building.Health > 25)
                    {
                        returnCost = Mathf.RoundToInt(returnCost * 0.71429f);
                    }

                    if (building.Health < 20 && building.Health > 5)
                    {
                        returnCost = Mathf.RoundToInt(returnCost * 0.5f);
                    }
                }
            }

            if (returnCost != refundCost)
            {
                refundCost = returnCost;
                refundText.text = $"Refund:\n{refundCost}<size=130%><sprite=\"all_icons\" index=2>";
            }
        }
    }

    public void ShowInfo(Building b)
    {
        if (DOTween.IsTweening(bg)) return;
        if (!cam) cam = Camera.main;
        Visible = true;
        bg.rectTransform.DOScale(1, 0.3f);

        building = b;
        shipComp = null;
        if (building.BuildingType == BuildingType.Hub)
        {
            bg.color = new Color32(0, 92, 118, 237);
            destroyButton.gameObject.SetActive(false);
            upgradeButton.gameObject.SetActive(true);
            refundText.gameObject.SetActive(false);
        }
        else
        {
            destroyButton.gameObject.SetActive(true);
            upgradeButton.gameObject.SetActive(false);
            refundText.gameObject.SetActive(true);
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
        UpdateText();

        Transform range = b.transform.Find("Range");
        if (range)
        {
            Range = range;
            Range.gameObject.SetActive(true);
        }

        healthBar.SetActive(true);
        transform.position = cam.WorldToScreenPoint(b.transform.position) + new Vector3(Screen.width / 13, 0);
        gameObject.SetActive(true);
        InvokeRepeating("UpdateText", 0.1f, 0.1f);
    }

    public void ShowInfo(ShipComponent shipComponent)
    {
        if (!cam) cam = Camera.main;

        building = null;
        shipComp = shipComponent;
        if (shipComponent.Location.FogUnitActive)
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
        transform.position = cam.WorldToScreenPoint(shipComponent.transform.position) + new Vector3(Screen.width / 13, 0);
        gameObject.SetActive(true);
        Visible = true;
    }

    public void HideInfo()
    {
        if (DOTween.IsTweening(bg)) return;
        CancelInvoke("UpdateText");
        if (Range)
        {
            Range.gameObject.SetActive(false);
        }

        bg.rectTransform.DOScale(0.01f, 0.3f).OnComplete(
            delegate
            {
                DOTween.Kill(healthBarFill);
                healthBarFill.color = healthGradient.Evaluate(healthBarMask.fillAmount);
                gameObject.SetActive(false);
                Visible = false;
                building = null;
                shipComp = null;
            });
    }
}
