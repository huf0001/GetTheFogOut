using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipComponentsEnum
{
    Thrusters,
    Engine,
    Hull
}

public class ShipComponent : Entity
{
    [SerializeField] private WorldController controller;
    [SerializeField] private ShipComponentsEnum id;

    public ShipComponentsEnum Id { get => id; set => id = value; }

    // Start is called before the first frame update
    void Start()
    {
        controller = WorldController.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        Collect();
    }
    private void OnMouseEnter()
    {
        ShaderOnMethod();
        UIController.instance.buildingInfo.ShowInfo(this);
    }

    private void OnMouseExit()
    {
        ShaderOffMethod();
        UIController.instance.buildingInfo.HideInfo();
    }

    public void Collect()
    {
        if (Location.FogUnit == null)
        {
            controller.GetShipComponent(id).Collected = true;
            UIController.instance.buildingInfo.HideInfo();
        }
    }

    private void ShaderOnMethod()
    {
        //Stuff
    }

    private void ShaderOffMethod()
    {
        //Stuff
    }
}
