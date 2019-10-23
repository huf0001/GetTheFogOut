using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UpgradeController : MonoBehaviour
{
    [SerializeField] private GameObject harvester;
    [SerializeField] private GameObject pulse;
    [SerializeField] private GameObject cannon;
    private Harvester hvst;
    private RepelFan rpf;
    private ArcDefence air;
    public int upgradeCost;
    public int upgradePath;
    public int upgradeLevel;
    public string selectedBuilding;

    public void GetBuilding(string selected)
    {
        selectedBuilding = selected;
    }

    public void GetUpgradePath(int path)
    {
        upgradePath = path;
    }

    public void ProcessUpgrade()
    {
        // need to check COST too, by getting from UI or manually button
        if (BuildingType.Harvester.ToString().Equals(selectedBuilding))
        {
            Harvester hvst = harvester.GetComponentInChildren<Harvester>();
            //hvst.Upgrade(upgradePath);
        }
        else if (BuildingType.FogRepeller.ToString().Equals(selectedBuilding))
        {
            RepelFan rpf = pulse.GetComponentInChildren<RepelFan>();
        }
        else if (BuildingType.AirCannon.ToString().Equals(selectedBuilding))
        {
            ArcDefence air = cannon.GetComponentInChildren<ArcDefence>();
        }
    }
}

