using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ShipComponentsEnum
{
    Thrusters,
    Engine,
    Hull
}

public class ShipComponent : Entity, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Component ID")]
    [SerializeField] private ShipComponentsEnum id;

    private Material missingWingMaterial;
    public ShipComponentsEnum Id { get => id; set => id = value; }

    // Start is called before the first frame update
    void Start()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        missingWingMaterial = rend.material;
    }

    // Update is called once per frame
    void Update()
    {
        if (location.FogUnitActive && location.PowerSource)
        {
            missingWingMaterial.SetFloat("_Toggle", 1f);
        }
        else
        {
            missingWingMaterial.SetFloat("_Toggle", 0f);
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (!UIController.instance.buildingInfo.Visible && !WorldController.Instance.IsPointerOverGameObject())
        {
            UIController.instance.buildingInfo.ShowInfo(this);
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (!UIController.instance.buildingInfo.building)
        {
            UIController.instance.buildingInfo.HideInfo();
        }
    }

    public void Collect()
    {
        if (!Location.FogUnitActive && location.PowerSource)
        {
            WorldController.Instance.GetShipComponent(id).Collected = true;
            UIController.instance.buildingInfo.HideInfo();
        }
    }
}
