using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//TODO: replace a TILE object with an invinsible box and only show upon click on the button(tower)
//TODO: add Null ref / exception :(
[Serializable]
public class TileData
{
    //Serialized Fields
    [SerializeField] private List<PowerSource> powerSources = new List<PowerSource>();

    [SerializeField] private ResourceNode resource = null;
    [SerializeField] private Building building = null;

    [SerializeField] private Material onMaterial;
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material startMaterial;

    [SerializeField] private int x = 0;
    [SerializeField] private int z = 0;

    //Private fields
    private FogUnit fogUnit = null;
    private GameObject placedTower = null;
    private List<TileData> adjacentTiles = new List<TileData>();
    private List<TileData> allAdjacentTiles = new List<TileData>();

    private List<Building> observers = new List<Building>();
    private bool visited = false;

    //Public Fields
    public GameObject plane;

    //Public Properties
    public int X { get => x; set => x = value; }
    public int Z { get => z; set => z = value; }

    public FogUnit FogUnit { get => fogUnit; set => fogUnit = value; }
    public GameObject PlacedTower { get => placedTower; set => placedTower = value; }
    public ResourceNode Resource { get => resource; set => resource = value; }

    public string Name { get => "Tile (" + x + "," + z + ")"; }
    public bool Visited { get => visited; set => visited = value; }
    public List<TileData> AdjacentTiles { get => adjacentTiles; }
    public List<TileData> AllAdjacentTiles { get => allAdjacentTiles; }

    private bool buildable = true;
    //Altered public properties
    public Building Building
    {
        get => building;

        set
        {
            building = value;

            if (Resource != null)
            {
                UpdateResource();
            }
        }
    }

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

    public Vector3 Position
    {
        get
        {
            return new Vector3(x, 0, z);
        }
    }
   
    public TileData(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public PowerSource GetClosestPowerSource(Transform buildingTrans)
    {
        if (powerSources.Count > 0)
        {
            List<PowerSource> powerSourcesOrdered = powerSources.OrderBy(x => Vector2.Distance(buildingTrans.position, x.transform.position)).ToList();

            return powerSourcesOrdered[0];
        }
        else
        {
            return null;
        }
    }
    public void PowerUp(PowerSource power)
    {
        if (!powerSources.Contains(power))
        {
            powerSources.Add(power);
        }

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
        if (powerSources.Contains(power))
        {
            powerSources.Remove(power);
        }

        if (building != null)
        {
             building.SetPowerSource();
        }
    }

    public void AddObserver(Building observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(Building observer)
    {
        observers.Remove(observer);
    }

    void UpdateResource()
    {
        bool visTemp = Resource.Visible;

        if (Building == null && Resource.Health > 0)
        {
            Resource.Visible = true;
        }
        else if (Building == null && Resource.Health == 0)
        {
            Debug.Log("Harvester removed, no longer needed");
        }
        else
        {
            if (Building.BuildingType == BuildingType.Harvester) // || Resource.Health <= 0
            {
                Resource.Visible = false;
            }
        }

        if (visTemp != Resource.Visible)
        {
            MeshRenderer[] renderers = Resource.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = !renderer.enabled;
            }
        }
    }

    public List<TileData> CollectTilesInRange(int xc, int yc, int r)
    {
        if (r < 1) return null;

        List<TileData> tiles = new List<TileData>();
        TileData[,] worldTiles = WorldController.Instance.Tiles;
        int width = WorldController.Instance.Width;
        int height = WorldController.Instance.Length;
        int yoff = r;
        int xoff = 0;
        int b = -r;
        int p0, p1, w0, w1;

        while (xoff <= yoff)
        {
            p0 = xc - xoff;
            p1 = xc - yoff;
            w0 = xoff + xoff;
            w1 = yoff + yoff;

            hl(p0, yc - yoff, yc + yoff, w0);
            hl(p1, yc - xoff, yc + xoff, w1);

            if ((b += xoff+++xoff) >= 0)
            {
                b -= --yoff + yoff;
            }
        }

        return tiles;

        void hl(int x0, int y1, int y2, int w)
        {
            w++;
            int xw = 0;
            while (w-- > 0)
            {
                xw = x0 + w;
                AddPoint(xw, y1);
                AddPoint(xw, y2);
            }
        }

        void AddPoint(int x1, int y1)
        {
            if (x1 < width && y1 < height && x1 >= 0 && y1 >= 0)
            {
                tiles.Add(worldTiles[x1, y1]);
            }
        }
    }

    public void CollectTilesInRangeAlt(List<TileData> tiles, int range)
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
                    tile.CollectTilesInRangeAlt(tiles, range - 1);
                }
            }
        }
    }

    public TileData getself()
    {
        return this;
    }

}
