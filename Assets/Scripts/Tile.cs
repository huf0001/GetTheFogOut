using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//TODO: replace a TILE object with an invinsible box and only show upon click on the button(tower)
//TODO: add Null ref / exception :(
public class Tile : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] private List<PowerSource> powerSources = new List<PowerSource>();

    [SerializeField] private ResourceNode resource;
    [SerializeField] private Building building = null;

    [SerializeField] private Material onMaterial;
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material startMaterial;

    //Private fields
    private int x = 0;
    private int z = 0;
    private FogUnit fogUnit = null;
    private GameObject placedtower;
    private List<Tile> adjacentTiles = new List<Tile>();

    private List<Building> observers = new List<Building>();

    //Simple public properties
    public int X { get => x; set => x = value; }
    public int Z { get => z; set => z = value; }

    public ResourceNode Resource { get => resource; set => resource = value; }
    public FogUnit FogUnit { get => fogUnit; set => fogUnit = value; }
    public Building Building { get => building; set => building = value; }
    public GameObject Placedtower { get => placedtower; set => placedtower = value; }

    public List<Tile> AdjacentTiles { get => adjacentTiles; }

    //Altered public properties
    public PowerSource PowerSource
    {
        get
        {
            if (powerSources.Count == 0)
            {
                return null;
            }
            else
            {
                return powerSources[0];
            }
        }
    }

    public void PowerUp(PowerSource power)
    {
        this.gameObject.GetComponent<Renderer>().material = onMaterial;
        powerSources.Add(power);
    }

    public void PowerDown(PowerSource power)
    {
        powerSources.Remove(power);

        if (powerSources.Count == 0)
        { 
            this.gameObject.GetComponent<Renderer>().material = visibleMaterial;
        }
    }

    public void AddObserver(Building observer)
    {
        observers.Add(observer);

        if (powerSources.Count == 0)
        {
            this.gameObject.GetComponent<Renderer>().material = visibleMaterial;
        }
    }

    public void RemoveObserver(Building observer)
    {
        observers.Remove(observer);

        if (observers.Count == 0)
        {
            this.gameObject.GetComponent<Renderer>().material = startMaterial;
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
