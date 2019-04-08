﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    private class Point2D
    {
        public int x;
        public int y;

        public Point2D(int a, int b)
        {
            x = a;
            y = b;
        }
    }

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
    private Hub hub = null;
    public Hub Hub { get => hub; set => hub = value; }

    private GameObject temp;
    private GameObject PlaneSpawn;
    private GameObject TowerSpawn;
    private GameObject TowerToSpawn;
    private GameObject tiletest;
    private GameObject tmp;
    private GameObject[] objs;
    private TowerManager tm;
    private Vector3 pos;
    public bool InBuildMode;
    [SerializeField] private GameObject planeGridprefab;

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more world managers.");
        }

        InBuildMode = false;
        Instance = this;

        InstantiateTileArray();

        ConnectAdjacentTiles();

        GetComponent<Fog>().SpawnFog();
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


                //set to true will render the tile; set to false, and it won't
                tileGo.GetComponent<MeshRenderer>().enabled = true;
                MeshRendererTileChild(true);

                if (Random.Range(1, 100) < mineralSpawnChance)
                {
                    pos.y += 0.2f;
                    GameObject mineral = Instantiate(mineralPrefab, pos, tileGo.transform.rotation);
                    tileGo.GetComponent<Tile>().Resource = mineral.GetComponentInChildren<ResourceNode>();
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

    //Connects each tile to its orthogonally adjacent and diagonally adjacent neighbours
    private void ConnectAdjacentTiles()
    {
        Tile t;
        Tile a;

        //if (gameObject.GetComponent<Fog>().Expansion == FogExpansion.Orthogonal)
        //{
        //    foreach (GameObject o in tiles)
        //    {
        //        t = o.GetComponent<Tile>();

        //        Point2D[] pts = { new Point2D(t.X, t.Z - 1), new Point2D(t.X, t.Z + 1), new Point2D(t.X - 1, t.Z), new Point2D(t.X - 1, t.Z) };

        //        foreach (Point2D p in pts)
        //        {
        //            a = GetTileAt(p.x, p.y).GetComponent<Tile>();

        //            if (!t.AdjacentTiles.Contains(a))
        //            {
        //                t.AdjacentTiles.Add(a);
        //                continue;
        //            }
        //        }
        //    }
        //}
        //else
        //{
            foreach (GameObject o in tiles)
            {
                t = o.GetComponent<Tile>();

                for (int i = t.X - 1; i <= t.X + 1; i++)
                {
                    if (i >= 0 && i < width)
                    {
                        for (int j = t.Z - 1; j <= t.Z + 1; j++)
                        {
                            if (j >= 0 && j < length)
                            {
                                a = GetTileAt(i, j).GetComponent<Tile>();

                                if (!t.AdjacentTiles.Contains(a))
                                {
                                    t.AdjacentTiles.Add(a);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        //}
    }

    private void MeshRendererTileChild(bool toggle)
    {
        objs = GameObject.FindGameObjectsWithTag("Tile");
       
        //Caution child can be more than 4!!!
        foreach (GameObject obj in objs)
        {
            for (int i = 0; i < 4; i++)
            {
                obj.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = toggle;
            }
        }
    }

    private void EnableMeshRendTile(GameObject tile_obj)
    {
        pos = new Vector3(tile_obj.transform.position.x, tile_obj.transform.position.y + 0.1f, tile_obj.transform.position.z);
        if (temp == null)
        {   
            PlaneSpawn = Instantiate(planeGridprefab,pos,tile_obj.transform.rotation);           
            temp = tile_obj;
        }
        else {
            if (temp != tile_obj)
            {
                Destroy(PlaneSpawn);             
                Destroy(TowerSpawn);
                temp = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(PlaneSpawn);
            Destroy(TowerSpawn);
            tm.EscToCancel();
            MeshRendererTileChild(false);
            InBuildMode = false;
        }
    }

    private void RenderTower()
    {      
        TowerToSpawn = tm.GetTower();

        if (TowerSpawn == null)
        {
            TowerSpawn = Instantiate(TowerToSpawn, pos, Quaternion.identity);
        }
        else 
        {
            if (TowerSpawn != TowerToSpawn)
            { 
                Destroy(TowerSpawn);
                TowerSpawn = Instantiate(TowerToSpawn, pos, Quaternion.identity);
            }
        }
    }

    private void ShowTile()
    {
        RaycastHit[] hits;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hits = Physics.RaycastAll(ray, 100.0f);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.transform.gameObject.tag == "Tile")
            {
                tiletest = hit.transform.gameObject;

                EnableMeshRendTile(tiletest);

            }
        }
    }

    public void SetToBuildMode()
    {
        InBuildMode = true;
    }

    private void Awake()
    {
        tm = FindObjectOfType<TowerManager>();
    }

    private void Update()
    {
        if (!hubBuilt)
        {
            InstantiateStartHub();
            hubBuilt = true;
        }
        if (InBuildMode)
        {
            MeshRendererTileChild(true);
            RenderTower();
            ShowTile();
        }
    }

    private void InstantiateStartHub()
    {
        Tile startingTile = tiles[Mathf.FloorToInt(width / 2), Mathf.FloorToInt(length / 2)].GetComponent<Tile>();
        Vector3 PosToInst = new Vector3(startingTile.transform.position.x, startingTile.transform.position.y + 0.4125f, startingTile.transform.position.z);
        GameObject hubGO = Instantiate(hubPrefab, PosToInst, startingTile.transform.rotation);
        Hub startHub = hubGO.GetComponentInChildren<Hub>();
        hub = startHub;

        hubGO.transform.SetParent(startingTile.transform);
        startingTile.Building = startHub;
        hub.Location = startingTile;
    }

    public GameObject GetTileAt(int x, int y)
    {
        if (x >= width || x < 0 || y >= length || y < 0)    //with by length array, the last value will be at position (width - 1, length - 1) cause arrays love 0s.
        {
            //Debug.Log("Tile (" + x + "," + y + ") is out of range.");
            Debug.LogError("Tile (" + x + "," + y + ") is out of range.");
            return null;
        }

        //Debug.Log("Getting tile at (" + x + "," + y + "). Width: " + width + ". Length: " + length + ".");
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
