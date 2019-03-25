using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerManager : MonoBehaviour
{
    
    [SerializeField]
    private btn_tower selectedTower;
    public btn_tower SelectedTower { get => selectedTower; set => selectedTower = value; }
    
    //TODO: TOGGLE ?
    //DESC: click on the button will get the value of the assigned value(button) ? xD
    public void OnCLickedbtn(btn_tower chooseTower)
    {
        this.SelectedTower = chooseTower;
    }

    //DESC: return prefab object *replace with tower 3d later
    public GameObject GetTower()
    {
        return this.SelectedTower.Obj_prefab;
    }

}
