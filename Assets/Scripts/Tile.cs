using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//TODO: replace a TILE object with an invinsible box and only show upon click on the button(tower)
//TODO: add Null ref / exception :(
public class Tile : MonoBehaviour
{
    private GameObject placedtower;
    public GameObject Placedtower { get => placedtower; set => placedtower = value; }
    [SerializeField] private PowerSource powerSource;
    public PowerSource PowerSource { get => powerSource; /*set => powerSource = value;*/ }

    [SerializeField] private Resource resource;
    public Resource Resource { get => resource; set => resource = value; }
    [SerializeField] private Building building = null;
    public Building Building { get => building; set => building = value; }
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;
    //public Material OnMaterial { get => onMaterial; set => onMaterial = value; }

    public void PowerUp(PowerSource power)
    {
        if (powerSource == null)
        {
            this.gameObject.GetComponent<Renderer>().material = onMaterial;
            powerSource = power;
        }
    }

    public void PowerDown(PowerSource power)
    {
        if (powerSource != null)
        {
            this.gameObject.GetComponent<Renderer>().material = offMaterial;
            powerSource = null;
        }
    }

    // ALL THIS FUCTIONALLITY WAS MOVED TO MOUSE CONTROLLER
    //void OnMouseUp()
    //{
    //    //TODO: check the condition if the player has enough currency to build on this tile
    //        if (!EventSystem.current.IsPointerOverGameObject())
    //        {
    //            TowerManager tm = FindObjectOfType<TowerManager>();
    //            placedtower = tm.GetTower();
    //            placebuilding();
    //    //DESC: reset the button, so you can't spam/accidentally building
    //            //tm.SelectedTower = null;
    //        }
    //}

    //void placebuilding()
    //{ 
    //    //TODO: try to fix the position if replaced by 3d game object
    //    //DESC: will replace a prefab on top of the tile(parent) position/rotation.
    //    Instantiate(placedtower, transform.position, transform.rotation);

    //    //DESC: destroy tile upon replacing new object
    //    //Destroy(transform.parent.gameObject);

    //}
}
