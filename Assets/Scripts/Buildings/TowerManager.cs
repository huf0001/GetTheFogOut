using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerManager : MonoBehaviour
{

    [SerializeField] private btn_tower selectedTower;
    [SerializeField] private BuildingType buildingType = BuildingType.None;
    [SerializeField] private GameObject emptyprefab;
    [SerializeField] private TileData currentTile;
    [SerializeField] private GameObject hologramTower;


    public btn_tower SelectedTower { get => selectedTower; set => selectedTower = value; }
    public BuildingType TowerBuildingType { get => buildingType; set => buildingType = value; }
    public TileData CurrentTile { get => currentTile; set => currentTile = value; }

    //TODO: TOGGLE ?
    //DESC: click on the button will get the value of the assigned value(button) ? xD
    public void OnButtonClicked(btn_tower chooseTower)
    {
        selectedTower = chooseTower;

        if (currentTile != null)
        {
            GameObject toBuild = GetTower("build");

            // If there is a building, delete it if deletion is selected. Otherwise, place one if the tile is empty.
            if (toBuild.name != "Empty")
            {
                //Debug.Log(toBuild.name);
                if (toBuild.GetComponentInChildren<Building>() != null)
                {
                    Building b = toBuild.GetComponentInChildren<Building>();
                    if (currentTile.Building == null && (currentTile.Resource == null || b.BuildingType == BuildingType.Harvester))
                    {
                        if (b.BuildingType == BuildingType.Generator)
                        {
                            if (ResourceController.Instance.Generators.Count >= ObjectiveController.Instance.GeneratorLimit) // the +1 accounts for the fact that the generator hologram, which has the buildingtype generator, will be on the board with the actual generators (not needed anymore)
                            {
                                Debug.Log("If you want to build more generators, collect more ship components first.");
                                return;
                            }
                        }

                        currentTile.PlacedTower = toBuild;
                        selectedTower = null;
                        if (ResourceController.Instance.StoredMineral > b.MineralCost) Destroy(hologramTower);
                        MouseController.Instance.Build(currentTile.PlacedTower, currentTile, 0f);
                    }
                }
                else
                {
                    Debug.Log("toBuild's building component is non-existant");
                }
            }
            else
            {
                Building removeBuilding = MouseController.Instance.ReturnCost(currentTile);
                MouseController.Instance.RemoveBulding(removeBuilding);
            }
        }
        //WorldController.Instance.InBuildMode = true;
    }

    public void OnHoverEnter(btn_tower tower)
    {
        GameObject TowerToSpawn;

        if (currentTile != null && tower.Button.interactable)
        {
            int x = Mathf.RoundToInt(currentTile.X);
            int y = Mathf.RoundToInt(currentTile.Z);
            Vector3 spawnPos = new Vector3(x, 0, y);

            selectedTower = tower;
            TowerToSpawn = GetTower("holo");
            hologramTower = Instantiate(TowerToSpawn, spawnPos, Quaternion.identity);
        }
    }

    public void OnHoverExit(btn_tower tower)
    {
        selectedTower = null;
        Destroy(hologramTower);
        //this.SelectedTower = chooseTower;
        //buildingType = SelectedTower.TowerType;
        //WorldController.Instance.InBuildMode = true;
    }

    //DESC: return prefab object *replace with tower 3d later
    public GameObject GetTower(string version)
    {
        if (selectedTower != null)
        {
            switch (version)
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
        return buildingType;
    }

    public void CancelBuild()
    {
        selectedTower = null;
        currentTile = null;
        Destroy(hologramTower);
        if (UIController.instance.buildingSelector.Visible) UIController.instance.buildingSelector.ToggleVisibility();
        if (!UIController.instance.UpgradeWindowVisible) UIController.instance.buildingInfo.HideInfo();
        buildingType = BuildingType.None;
    }
}
