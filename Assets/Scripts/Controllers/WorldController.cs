﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    private class Point2D
    {
        public int x, y;

        public Point2D(int a, int b)
        {
            x = a;
            y = b;
        }
    }

    // Used to get the instance of the WorldManager from anywhere.
    public static WorldController Instance { get; protected set; }
    
    [SerializeField] private int width = 31;
    [SerializeField] private int length = 31;
    //[SerializeField] private Tiles gameboard = null;
    [SerializeField] private GameObject gameboardPrefab;
    [SerializeField] private Terrain ground;

    [SerializeField] GameObject tilePrefab, hubPrefab, mineralPrefab, fuelPrefab, powerPrefab, organPrefab;

    [SerializeField] int mineralSpawnChance = 5, fuelSpawnChance = 5, powerSpawnChance = 5, organSpawnChance = 5;

    public int Width { get => width; }

    public int Length { get => length; }

    [SerializeField] private TileData[,] tiles;
    public TileData[,] Tiles { get => tiles; }

    private bool hubBuilt = false;
    [SerializeField] private Hub hub = null;
    public Hub Hub { get => hub; set => hub = value; }
    public Terrain Ground { get => ground; set => ground = value; }

    private GameObject temp, PlaneSpawn, TowerSpawn, TowerToSpawn, tiletest, tmp;
    private GameObject[] objs;
    private TowerManager tm;
    private Vector3 pos;
    public bool InBuildMode;
    [SerializeField] private GameObject planeGridprefab;

    //UI Controller
    UIController uiController;

    private bool isGameOver;

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more world managers.");
        }

        InBuildMode = false;
        Instance = this;
        isGameOver = false;

        //Get UIController currently used in scene
        uiController = GetComponent<UIController>();

        //if (gameboard == null)
        //{
        //    GameObject gbo = Instantiate(gameboardPrefab);
        //    gameboard = gbo.GetComponent<Tiles>();
        //}

        //tiles = gameboard.InstantiateTileArray();

        InstantiateTileArray();
        //SetupTiles();
        ConnectAdjacentTiles();
        SetBuildingsToTiles();
        GetComponent<Fog>().SpawnFog();
    }

    void SetBuildingsToTiles()
    {
        Building[] buildings = FindObjectsOfType<Building>();
        foreach (Building building in buildings)
        {
            TileData tile = GetTileAt(building.transform.position);
            building.Location = tile;
            building.Animator = building.GetComponentInChildren<Animator>();
            building.Animator.SetBool("Built", true);
            building.Place();
        }
    }

    private void InstantiateTileArray()
    {
        tiles = new TileData[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                tiles[x, z] = new TileData(x, z);
            }
        }
    }

    // OLD WAY OF DOING TILES AS GAME OBJECTS
    //private void SetupTiles()
    //{
    //    GameObject tileGO;
    //    Vector3 pos;

    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int z = 0; z < length; z++)
    //        {
    //            tileGO = tiles[x, z];

    //            tileGO.transform.SetParent(this.transform, true);
    //            tileGO.name = "Tile_" + x + "_" + z;
    //            tileGO.layer = 8;
    //            tileGO.tag = "Tile";
    //            tileGO.GetComponent<Tile>().X = x;
    //            tileGO.GetComponent<Tile>().Z = z;
    //            pos = tileGO.transform.position;

    //            //set to true will render the tile; set to false, and it won't
    //            tileGO.GetComponent<MeshRenderer>().enabled = true;
    //            MeshRendererTileChild(false);

    //            if (Random.Range(1, 100) < mineralSpawnChance)
    //            {                    
    //                pos.y += 0.2f;
    //                GameObject mineral = Instantiate(mineralPrefab, pos, tileGO.transform.rotation);
    //                tileGO.GetComponent<Tile>().Resource = mineral.GetComponentInChildren<ResourceNode>();
    //                mineral.transform.SetParent(tileGO.transform, true);
    //            }
    //            else if (Random.Range(1, 100) < fuelSpawnChance)
    //            {
    //                pos.y += 0.3f;
    //                GameObject fuel = Instantiate(fuelPrefab, pos, tileGO.transform.rotation);
    //                tileGO.GetComponent<Tile>().Resource = fuel.GetComponentInChildren<ResourceNode>();
    //                fuel.transform.SetParent(tileGO.transform, true);
    //            }
    //            else if (Random.Range(1, 100) < organSpawnChance)
    //            {
    //                pos.y += 0.3f;
    //                GameObject organ = Instantiate(organPrefab, pos, tileGO.transform.rotation * Quaternion.Euler(0f, 180f, 0f));
    //                tileGO.GetComponent<Tile>().Resource = organ.GetComponentInChildren<ResourceNode>();
    //                organ.transform.SetParent(tileGO.transform, true);
    //            }
    //            else if (Random.Range(1, 100) < powerSpawnChance)
    //            {
    //                pos.y += 0.2f;
    //                GameObject power = Instantiate(powerPrefab, pos, tileGO.transform.rotation * Quaternion.Euler(0f, 180f, 0f));
    //                tileGO.GetComponent<Tile>().Resource = power.GetComponentInChildren<ResourceNode>();
    //                power.transform.SetParent(tileGO.transform, true);
    //            }
    //        }
    //    }
    //}

    //Connects each tile to its orthogonally adjacent and diagonally adjacent neighbours
    private void ConnectAdjacentTiles()
    {
        TileData a;

        if (gameObject.GetComponent<Fog>().ExpansionDirection == FogExpansionDirection.Orthogonal)
        {
            foreach (TileData t in tiles)
            {

                Point2D[] pts = { new Point2D(t.X, t.Z - 1), new Point2D(t.X, t.Z + 1), new Point2D(t.X - 1, t.Z), new Point2D(t.X + 1, t.Z) };

                foreach (Point2D p in pts)
                {
                    if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < length)
                    {
                        Vector2 pos = new Vector2(p.x, p.y);
                        a = GetTileAt(pos);

                        if (!t.AdjacentTiles.Contains(a))
                        {
                            t.AdjacentTiles.Add(a);
                            continue;
                        }
                    }
                }
            }
        }
        else
        {
            foreach (TileData t in tiles)
            {
                for (int i = t.X - 1; i <= t.X + 1; i++)
                {
                    if (i >= 0 && i < width)
                    {
                        for (int j = t.Z - 1; j <= t.Z + 1; j++)
                        {
                            if (j >= 0 && j < length)
                            {
                                Vector2 pos = new Vector2(i, j);
                                a = GetTileAt(pos);

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
        }

        foreach (TileData t in tiles)
        {
            for (int i = t.X - 1; i <= t.X + 1; i++)
            {
                if (i >= 0 && i < width)
                {
                    for (int j = t.Z - 1; j <= t.Z + 1; j++)
                    {
                        if (j >= 0 && j < length)
                        {
                            Vector2 pos = new Vector2(i, j);
                            a = GetTileAt(pos);

                            if (!t.AllAdjacentTiles.Contains(a))
                            {
                                t.AllAdjacentTiles.Add(a);
                                continue;
                            }
                        }
                    }
                }
            }
        }
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
        else
        {
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
        if (!isGameOver)
        {
            //if (InBuildMode)
            //{
            //    MeshRendererTileChild(true);
            //    RenderTower();
            //    ShowTile();
            //}
            
            if(hub.IsWin() || hub.isDestroyed())
            {
                isGameOver = true;
            }
        }
        else
        {
            if(hub.IsWin())
            {
                //Display win UI
                uiController.EndGameDisplay("You win!");
            }
            else
            {
                //Display lose UI
                uiController.EndGameDisplay("You lose!");
            }
        }
    }
    //rotate to 90

    private void InstantiateStartHub()
    {
        //int x = GetHalf(width);
        //int y = GetHalf(length);
        //Tile startingTile = tiles[x, y].GetComponent<Tile>();
        //Vector3 PosToInst = new Vector3(startingTile.transform.position.x, startingTile.transform.position.y + 0.4125f, startingTile.transform.position.z);
        //GameObject hubGO = Instantiate(hubPrefab, PosToInst, startingTile.transform.rotation * Quaternion.Euler(0f, 180f, 0f));
        //Hub startHub = hubGO.GetComponentInChildren<Hub>();
        //hub = startHub;

        //hubGO.transform.SetParent(startingTile.transform);
        //startingTile.Building = startHub;
        //hub.Location = startingTile;
    }

    private int GetHalf(int n)
    {
        if (n % 2 == 0)
        {
            return Mathf.FloorToInt(n / 2);
        }
        else
        {
            return Mathf.FloorToInt((n - 1) / 2);
        }
    }

    public TileData GetTileAt(Vector2 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);

        if (x >= width || x < 0 || y >= length || y < 0)    //with by length array, the last value will be at position (width - 1, length - 1) cause arrays love 0s.
        {
            Debug.LogError("Tile (" + x + "," + y + ") is out of range.");
            return null;
        }
        return tiles[x, y];
    }

    public TileData GetTileAt(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.z);

        if (x >= width || x < 0 || y >= length || y < 0)    //with by length array, the last value will be at position (width - 1, length - 1) cause arrays love 0s.
        {
            Debug.LogError("Tile (" + x + "," + y + ") is out of range.");
            return null;
        }
        return tiles[x, y];
    }
}
