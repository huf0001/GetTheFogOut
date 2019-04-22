using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//TODO: replace a TILE object with an invinsible box and only show upon click on the button(tower)
//TODO: add Null ref / exception :(
public class TileData
{
    //Serialized Fields
    [SerializeField] private List<PowerSource> powerSources = new List<PowerSource>();

    [SerializeField] private ResourceNode resource = null;
    [SerializeField] private Building building = null;

    [SerializeField] private Material onMaterial;
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material startMaterial;

    //Private fields
    private int x = 0;
    private int z = 0;
    private FogUnit fogUnit = null;
    private GameObject placedtower = null;
    private List<TileData> adjacentTiles = new List<TileData>();
    private List<TileData> allAdjacentTiles = new List<TileData>();

    private List<Building> observers = new List<Building>();
    private bool visited = false;

    //Simple public properties
    public int X { get => x; set => x = value; }
    public int Z { get => z; set => z = value; }

    public ResourceNode Resource { get => resource; set => resource = value; }
    public FogUnit FogUnit { get => fogUnit; set => fogUnit = value; }
    public Building Building { get => building; set => building = value; }
    public GameObject Placedtower { get => placedtower; set => placedtower = value; }

    public List<TileData> AdjacentTiles { get => adjacentTiles; }
    public List<TileData> AllAdjacentTiles { get => allAdjacentTiles; }
    public bool Visited { get => visited; set => visited = value; }

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

    public TileData(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public void PowerUp(PowerSource power)
    {
        //this.gameObject.GetComponent<Renderer>().material = onMaterial;
        powerSources.Add(power);
        if (building != null)
        {
            if (!building.Powered)
            {
                building.SetPowerSource();
            }
        }
    }

    public void PowerDown(PowerSource power)
    {
        powerSources.Remove(power);

        if (powerSources.Count == 0)
        {
            //this.gameObject.GetComponent<Renderer>().material = visibleMaterial;
        }
    }

    public void AddObserver(Building observer)
    {
        observers.Add(observer);

        if (powerSources.Count == 0)
        {
            //this.gameObject.GetComponent<Renderer>().material = visibleMaterial;
        }
    }

    public void RemoveObserver(Building observer)
    {
        observers.Remove(observer);

        if (observers.Count == 0)
        {
            //this.gameObject.GetComponent<Renderer>().material = startMaterial;
        }
    }

    public void CollectTilesInRange(List<TileData> tiles, int range)
    {
        // Adds all tiles in a specified range to a List.
        // IMPORTANT!!! SET ALL TILES IN THE LIST '.VISITED' TO FALSE AFTER USE!!!
        if (!visited)
        {
            tiles.Add(this);
            visited = true;
            foreach (TileData tile in adjacentTiles)
            {
                if (range - 1 > 0)
                {
                    tile.CollectTilesInRange(tiles, range - 1);
                }
            }
        }
    }
}
