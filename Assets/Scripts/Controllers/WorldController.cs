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
    [SerializeField]private GameObject planeGridprefab;

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more world managers.");
        }

        InBuildMode = false;
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


                //set to true will render the tile
                tileGo.GetComponent<MeshRenderer>().enabled = false;
                MeshRendererTileChild(false);

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
                if (Physics.Raycast(ray, out hit))
                {
                    EnableMeshRendTile(tiletest);
                }
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
