using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class WorldController : MonoBehaviour
{

    // Used to get the instance of the WorldManager from anywhere.
    public static WorldController Instance { get; protected set; }

    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject mineralPrefab;
    [SerializeField] GameObject hubPrefab;

    [SerializeField] int mineralSpawnChance = 5;

    [SerializeField] private int width = 30;
    public int Width { get => width; }

    [SerializeField] private int length = 30;
    public int Length { get => length; }

    private GameObject[,] tiles;

    private bool hubBuilt = false;

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more world managers.");
        }

        Instance = this;

        InstantiateTileArray();

        this.gameObject.GetComponent<Fog>().InstantiateFog();
    }

    private void InstantiateTileArray()
    {
        tiles = new GameObject[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                float y = 0.2f; // TODO: Account for terrain height
                GameObject tileGo = Instantiate(tilePrefab);
                Vector3 pos = new Vector3(x, y, z);
                tileGo.transform.position = pos;
                tileGo.transform.SetParent(this.transform, true);
                tileGo.name = "Tile_" + x + "_" + z;
                tileGo.layer = 9;
                tileGo.tag = "Tile";
                tileGo.GetComponent<Tile>().X = x;
                tileGo.GetComponent<Tile>().Z = z;

                if (Random.Range(1, 100) < mineralSpawnChance)
                {
                    tileGo.GetComponent<Tile>().Resource = Resource.Mineral;
                    pos.y += 0.2f;
                    GameObject mineral = Instantiate(mineralPrefab, pos, tileGo.transform.rotation);
                    mineral.transform.SetParent(tileGo.transform, true);
                }

                tiles[x, z] = tileGo;
            }
        }

        // OLD CODE, IGNORE. MAY USE LATER
        //// Create a game object for each tile
        //for (int x = 0; x < width; x++)
        //{
        //    for (int y = 0; y < length; y++)
        //    {
        //        Tile tileData = GetTileAt(x, y);

        //        GameObject tileGo = Instantiate(tilePrefab);
        //        tileGo.name = "Tile_" + x + "_" + y;
        //        tileGo.transform.position = new Vector3(tileData.X, tileData.Y, tileData.Z);
        //        tileGo.transform.SetParent(this.transform, true);

        //        tileData.RegisterTileBuildingChangedCallback(
        //            (tile, building) => { OnTileBuildingChanged(tile, tileGo, building); });
        //        tileData.RegisterTileResourceChangedCallback(
        //            (tile) => { OnTileResourceChanged(tile, tileGo); });
        //    }
        //}
    }

    private void Update()
    {
        if (!hubBuilt)
        {
            InstantiateStartHub();
            hubBuilt = true;
        }
    }

    private void InstantiateStartHub()
    {
        Tile startingTile = tiles[Mathf.FloorToInt(width / 2), Mathf.FloorToInt(length / 2)].GetComponent<Tile>();
        Vector3 PosToInst = new Vector3(startingTile.transform.position.x, startingTile.transform.position.y + 0.4125f, startingTile.transform.position.z);
        GameObject hubGO = Instantiate(hubPrefab, PosToInst, startingTile.transform.rotation);
        Hub hub = hubGO.GetComponentInChildren<Hub>();

        hubGO.transform.SetParent(startingTile.transform);
        startingTile.Building = hub;
        hub.Location = startingTile;
    }

    public GameObject GetTileAt(int x, int y)
    {
        if (x > width || x < 0 || y > length || y < 0)
        {
            Debug.LogError("Tile (" + x + "," + y + ") is out of range.");
            return null;
        }
        return tiles[x, y];
    }

    // OLD CODE, IGNORE. MAY USE LATER
    //void OnTileBuildingChanged (Tile tileData, GameObject tileGo, Building building)
    //{
    //    if (tileData.Building != null)
    //    {
    //        Destroy(tileData.Building);
    //    }
    //    Building newBuilding = Instantiate(building);
    //    newBuilding.transform.position = tileGo.transform.position;
    //    Vector3 pos = newBuilding.transform.position;

    //    // put ontop of tile. TODO: this should not be done with magic number
    //    pos.y += 0.2f;
    //    newBuilding.transform.position = pos;

    //    tileData.Building = newBuilding;
    //}

    //void OnTileResourceChanged(Tile tileData, GameObject tileGo)
    //{

    //}
}
