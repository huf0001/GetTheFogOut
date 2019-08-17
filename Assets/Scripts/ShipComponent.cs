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

public class ShipComponent : Entity
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
        if (location.FogUnitActive)
        {
            missingWingMaterial.SetFloat("_Toggle", 1f);
        }
        else
        {
            missingWingMaterial.SetFloat("_Toggle", 0f);
        }
    }

    private void OnMouseEnter()
    {
        if (!UIController.instance.buildingInfo.Visible && !WorldController.Instance.IsPointerOverGameObject())
        {
            UIController.instance.buildingInfo.ShowInfo(this);
        }
    }

    private void OnMouseExit()
    {
        if (!UIController.instance.buildingInfo.building)
        {
            UIController.instance.buildingInfo.HideInfo();
        }
    }

    public void Collect()
    {
        if (!Location.FogUnitActive)
        {
            WorldController.Instance.GetShipComponent(id).Collected = true;
            UIController.instance.buildingInfo.HideInfo();
        }
    }
}
