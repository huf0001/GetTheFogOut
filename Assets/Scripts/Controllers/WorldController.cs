using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class ShipComponentState
{
    [SerializeField] private ShipComponentsEnum component;
    [SerializeField] private bool collected;

    public ShipComponentsEnum Component { get => component; set => component = value; }
    public bool Collected { get => collected; set => collected = value; }

    public ShipComponentState(ShipComponentsEnum e, bool b)
    {
        component = e;
        collected = b;
    }
}

public enum TutorialStage
{
    None,
    CrashLanding,
    ShipPartsCrashing,
    ZoomBackToShip,
    ExplainSituation,
    BuildHarvester,
    BuildGenerator,
    BuildRelay,
    FogIsHazard,
    BuildClusterFan,
    Finished
}

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

    // Serialized fields
    [Header("World Spawning Rules")]
    [SerializeField] private int width = 31;
    [SerializeField] private int length = 31;
    [SerializeField] private bool spawnResources = false;
    [SerializeField] int mineralSpawnChance = 5, fuelSpawnChance = 5, powerSpawnChance = 5, organSpawnChance = 5;
    //[SerializeField] private Tiles gameboard = null;

    [Header("Prefab/Gameobject assignment")]
    [SerializeField] private Terrain ground;

    [SerializeField] GameObject tilePrefab, hubPrefab, mineralPrefab, fuelPrefab, powerPrefab, organPrefab;

    [SerializeField] private GameObject planeGridprefab;
    [SerializeField] private Hub hub = null;
    [SerializeField] private TileData[,] tiles;
    [SerializeField] private ShipComponentState[] shipComponents;
    public GameObject pause;

    [Header("Public variable?")]
    public bool InBuildMode;


    // Non serialized fields
    private GameObject temp, PlaneSpawn, TowerSpawn, TowerToSpawn, tiletest, tmp;
    private GameObject[] objs;
    private TowerManager tm;
    private Vector3 pos;
    private TutorialStage tutorialStage = TutorialStage.CrashLanding;


    // UI & Mouse Controller
    UIController uiController;
    MouseController mouseController;

    // Flags
    private bool hubBuilt = false;
    private bool isGameOver;

    // Public properties
    // public static WorldController used to get the instance of the WorldManager from anywhere.
    public static WorldController Instance { get; protected set; }
    public TileData[,] Tiles { get => tiles; }
    public Terrain Ground { get => ground; set => ground = value; }
    public int Width { get => width; }
    public int Length { get => length; }
    public Hub Hub { get => hub; set => hub = value; }
    public ShipComponentState[] ShipComponents { get => shipComponents; }
    public TutorialStage TutorialStage { get => tutorialStage;  }

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

        InstantiateTileArray();
        //SetupTiles();
        ConnectAdjacentTiles();
        SetResourcesToTiles();
        SetBuildingsToTiles();
        GetComponent<Fog>().SpawnFog();
    }

    void SetResourcesToTiles()
    // Collect all Resources in the scene and assign them to the closest tile
    {
        ResourceNode[] resources = FindObjectsOfType<ResourceNode>();

        foreach (ResourceNode resourceNode in resources)
        {
            TileData tile = GetTileAt(resourceNode.transform.position);
            tile.Resource = resourceNode;
            resourceNode.Location = tile;
        }
    }

    void SetBuildingsToTiles()
    // Collect all Buildings in the scene and assign them to the closest tile
    {
        Building[] buildings = FindObjectsOfType<Building>();

        foreach (Building b in buildings)
        {
            if (b.BuildingType == BuildingType.Hub)
            {
                //TileData tile = GetTileAt(building.transform.position);
                TileData tile = GetTileAt(b.transform.parent.position);
                tile.Building = b;
                b.Location = tile;
                b.Animator = b.GetComponentInChildren<Animator>();
                b.Animator.SetBool("Built", true);
                b.Place();
                break;
            }
        }

        foreach (Building b in buildings)
        {
            if (b.BuildingType != BuildingType.Hub)
            {
                //TileData tile = GetTileAt(b.transform.position);
                TileData tile = GetTileAt(b.transform.parent.position);
                b.Location = tile;
                b.Animator = b.GetComponentInChildren<Animator>();
                b.Animator.SetBool("Built", true);
                b.Place();
            }
        }
    }

    private void InstantiateTileArray()
    {
        tiles = new TileData[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                TileData tile = new TileData(x, z);
                tiles[x, z] = tile;
                pos = new Vector3(tile.X, 0, tile.Z);

                //set to true will render the tile; set to false, and it won't
                //tileGO.GetComponent<MeshRenderer>().enabled = true;
                //MeshRendererTileChild(false);

                if (spawnResources)
                {
                    if (UnityEngine.Random.Range(1, 100) < mineralSpawnChance)
                    {
                        pos.y += 0.2f;
                        GameObject mineral = Instantiate(mineralPrefab, pos, Quaternion.Euler(0f, 0f, 0f));
                        tile.Resource = mineral.GetComponentInChildren<ResourceNode>();
                        mineral.GetComponentInChildren<ResourceNode>().Location = tile;
                        mineral.transform.SetParent(ground.transform, true);
                    }
                    else if (UnityEngine.Random.Range(1, 100) < fuelSpawnChance)
                    {
                        pos.y += 0.3f;
                        GameObject fuel = Instantiate(fuelPrefab, pos, Quaternion.Euler(0, 0, 0));
                        tile.Resource = fuel.GetComponentInChildren<ResourceNode>();
                        fuel.GetComponentInChildren<ResourceNode>().Location = tile;
                        fuel.transform.SetParent(ground.transform, true);
                    }
                    else if (UnityEngine.Random.Range(1, 100) < organSpawnChance)
                    {
                        pos.y += 0.3f;
                        GameObject organ = Instantiate(organPrefab, pos, Quaternion.Euler(0f, 180f, 0f));
                        tile.Resource = organ.GetComponentInChildren<ResourceNode>();
                        organ.GetComponentInChildren<ResourceNode>().Location = tile;
                        organ.transform.SetParent(ground.transform, true);
                    }
                    else if (UnityEngine.Random.Range(1, 100) < powerSpawnChance)
                    {
                        pos.y += 0.2f;
                        GameObject power = Instantiate(powerPrefab, pos, Quaternion.Euler(0f, 180f, 0f));
                        tile.Resource = power.GetComponentInChildren<ResourceNode>();
                        power.GetComponentInChildren<ResourceNode>().Location = tile;
                        power.transform.SetParent(ground.transform, true);
                    }
                }
            }
        }
    }

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
        TowerToSpawn = tm.GetTower("holo");

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (WorldController.Instance.Ground.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
        {
            if (TileExistsAt(hit.point))
            {
                TileData tile = GetTileAt(hit.point);
                int x = Mathf.RoundToInt(hit.point.x);
                int y = Mathf.RoundToInt(hit.point.z);
                Vector3 spawnPos = new Vector3(x, 0, y);

                if (TowerSpawn == null)
                {
                    TowerSpawn = Instantiate(TowerToSpawn, spawnPos, Quaternion.identity);
                }
                else
                {
                    if (TowerSpawn != TowerToSpawn)
                    {
                        Destroy(TowerSpawn);
                        TowerSpawn = Instantiate(TowerToSpawn, spawnPos, Quaternion.identity);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(PlaneSpawn);
            Destroy(TowerSpawn);
            tm.EscToCancel();
            InBuildMode = false;
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
            GameUpdate();
        }
        else
        {
            GameOverUpdate();
        }
    }

    private void GameUpdate()
    {
        if (InBuildMode)
        {
            //    MeshRendererTileChild(true);
            RenderTower();
            //    ShowTile();
        }

        // TEMP FIX, SHOULD BE REMOVED LATER
        if (hub == null)
        {
            hub = FindObjectOfType<Hub>();
        }

        if (Input.GetKeyDown("p"))
        {
            if (Time.timeScale == 1.0f)
            {
                Time.timeScale = 0.0f;
                pause.SetActive(true);
            }
            else
            {
                Time.timeScale = 1.0f;
                pause.SetActive(false);
            }

            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            // TEST CODE: Stopping Mouse from placing/deleting buildings in Game Pause/Over.
            // mouseController.GamePlayStop();
        }

        if (hub.IsWin() || hub.isDestroyed())
        {

            Time.timeScale = 0.2f;
            isGameOver = true;
            InBuildMode = false;
        }
    }

    private void GameOverUpdate()
    {
        if (hub.IsWin())
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
    //rotate to 90

    //private void InstantiateStartHub()
    //{
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
    //}

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

        if (x >= width || x < 0 || y >= length || y < 0)    //width by length array, the last value will be at position (width - 1, length - 1) 'cause arrays love 0s.
        {
            Debug.LogError("Tile (" + x + "," + y + ") is out of range.");
            return null;
        }

        return tiles[x, y];
    }

    public ShipComponentState GetShipComponent(ShipComponentsEnum c)
    {
        foreach (ShipComponentState s in ShipComponents)
        {
            if (s.Component == c)
            {
                return s;
            }
        }
        return null;
    }

    public int GetRecoveredComponentCount()
    {
        int count = 0;

        foreach (ShipComponentState s in ShipComponents)
        {
            if (s.Collected)
            {
                count += 1;
            }
        }

        return count;
    }

    public bool TileExistsAt(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.z);

        if (x >= width || x < 0 || y >= length || y < 0)    //with by length array, the last value will be at position (width - 1, length - 1) cause arrays love 0s.
        {
            //Debug.Log("Tile (" + x + "," + y + ") doesn't exist. Probably want to double check that it isn't supposed to exist and you haven't fucked up the code somewhere.");
            return false;
        }
        else
        {
            return true;
        }
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
