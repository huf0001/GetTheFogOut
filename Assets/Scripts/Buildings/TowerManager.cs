using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerManager : MonoBehaviour
{
    
    [SerializeField]
    private btn_tower selectedTower;
    public btn_tower SelectedTower { get => selectedTower; set => selectedTower = value; }
    [SerializeField] private GameObject emptyprefab;

    //TODO: TOGGLE ?
    //DESC: click on the button will get the value of the assigned value(button) ? xD
    public void OnCLickedbtn(btn_tower chooseTower)
    {
         this.SelectedTower = chooseTower;
         WorldController.Instance.InBuildMode = true;
    }

    //DESC: return prefab object *replace with tower 3d later
    public GameObject GetTower()
    {
        if (selectedTower != null)
        {
            return this.SelectedTower.Obj_prefab;
        }
        else
            return emptyprefab;
    }


    public void EscToCancel()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            selectedTower = null;
        }
    }

}
