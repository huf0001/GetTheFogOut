using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerManager : MonoBehaviour
{
    
    [SerializeField] private btn_tower selectedTower;
    [SerializeField] private BuildingType buildingType = BuildingType.None;
    [SerializeField] private GameObject emptyprefab;


    public btn_tower SelectedTower { get => selectedTower; set => selectedTower = value; }
    public BuildingType TowerBuildingType { get => buildingType; set => buildingType = value; }
    private bool InbuildMode = false;

    //TODO: TOGGLE ?
    //DESC: click on the button will get the value of the assigned value(button) ? xD
    public void OnButtonClicked(btn_tower chooseTower)
    {
        this.SelectedTower = chooseTower;
        WorldController.Instance.InBuildMode = true;
        InbuildMode = true;
    }

    //DESC: return prefab object *replace with tower 3d later
    public GameObject GetTower(string version)
    {
        if (selectedTower != null)
        {
            switch(version)
            {
                case "holo":
                    return this.SelectedTower.Holo_prefab;
                case "build":
                    return this.SelectedTower.Build_prefab;
            }
        }

        return emptyprefab;
    }

    public bool IsinBuild()
    {
        if (selectedTower != null)
        {
            return true;
        }
        else
            return false;
    }

    public BuildingType GetBuildingType()
    {
        return SelectedTower.TowerType;
    }

    public void EscToCancel()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            selectedTower = null;
            InbuildMode = false;
        }
    }

}
