using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FogExpansionDirection
{
    Orthogonal,
    OrthogonalAndDiagonal
}

public enum FogFillType
{
    Block,
    Fluid
}

public enum StartConfiguration
{
    OneSide,
    Corners,
    FourCompassPoints,
    EightCompassPoints,
    FourSides,
    SurroundingHub,
    FullBoard
}

public class Fog : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] private bool batching = false;
    [SerializeField] private FogUnit fogUnitPrefab;
    [SerializeField] private FogBatch fogBatchPrefab;
    [SerializeField] private StartConfiguration configuration;
    [SerializeField] private FogExpansionDirection expansionDirection;
    [SerializeField] private FogFillType fillType;
    [SerializeField] private bool fogAccelerates = true;
    [SerializeField] private float fogGrowth = 5f;
    [SerializeField] private float fogHealthLimit = 100f;
    [SerializeField] private float fogSpillThreshold = 50f;
    [SerializeField] private bool damageOn = false;
    [SerializeField] private float surroundingHubRange;

    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material invisibleMaterial;

    //Container object fields
    private List<TileData> fogCoveredTiles = new List<TileData>();                      //i.e. tiles currently covered by fog
    [SerializeField] private List<FogEntity> fogUnitsInPlay = new List<FogEntity>();                 //i.e. currently active fog units on the board
    private List<FogUnit> fogUnitsInPool = new List<FogUnit>();                 //i.e. currently inactive fog units waiting for spawning
    private List<FogBatch> fogBatches = new List<FogBatch>();                   //i.e. FogBatch objects in the scene

    //Object fields
    private WorldController wc;

    //Value fields
    private int xMax;
    private int zMax;

    //Public properties
    public FogExpansionDirection ExpansionDirection { get => expansionDirection; }
    public FogFillType FillType { get => fillType; }
    public static Fog Instance { get; protected set; }
    public float FogHealthLimit { get => fogHealthLimit; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should not be more than one Fog");
        }

        Instance = this;
    }

    //Create the max no. of fog units the game should need
    public void PopulateFogPool()
    {
        wc = WorldController.Instance;
        xMax = wc.Width;
        zMax = wc.Length;

        if (fogUnitsInPool.Count == 0)
        {
            for (int i = 0; i < xMax * zMax; i++)
            {
                fogUnitsInPool.Add(CreateFogUnit());
            }
        }
    }

    public void SpawnStartingFog()
    {
        if (batching)
        {
            SpawnFogBatches();
        }

        SpawnStartingFog(configuration);

        if (batching)
        {            
            foreach (FogBatch f in fogBatches)
            { 
                f.CheckBatching();
                //if (!f.Batched)
                //{
                //}
            }
        }
    }

    private void SpawnFogBatches()
    {
        for (int i = 0; i <= xMax - 5; i += 5)
        {
            for (int j = 0; j <= zMax - 5; j += 5)
            {
                List<TileData> tilesForBatch = GetAllTilesInSquareAt(i, j);

                if (tilesForBatch != null)
                {
                    FogBatch f = Instantiate<FogBatch>(fogBatchPrefab);
                    f.XMin = i;
                    f.XMax = i + 4;
                    f.ZMin = j;
                    f.ZMax = j + 4;
                    f.HealthLimit = fogHealthLimit;
                    f.transform.SetParent(this.transform, true);
                    fogBatches.Add(f);
                    f.transform.position = new Vector3(i + 2, 0.13f, j + 2);    // adding +2 to positions so that the batch is centred on the centre tile that it is a batch for.
                    f.gameObject.name = "FogBatch(" + i + "," + j + ")";
                    f.Tiles = tilesForBatch;

                    foreach (TileData t in f.Tiles)
                    {
                        t.FogBatch = f;

                        //if (t.X == f.XMin || t.X == f.XMax || t.Z == f.ZMin || t.Z == f.ZMax)
                        //{
                        //    f.EdgeTiles.Add(t);
                        //}
                    }
                }
            }
        }

        foreach (FogBatch f in fogBatches)
        {
            GetNeighboursOfFogBatch(f);
        }
    }

    //Tries to get all tiles that would be covered by the fog batch, returns null if the fog batch would spill over the edge of the board.
    private List<TileData> GetAllTilesInSquareAt(int x, int z)
    {
        List<TileData> tiles = new List<TileData>();

        for (int i = x; i < x + 5; i++)
        {
            for (int j = z; j < z + 5; j++)
            {
                if (WorldController.Instance.TileExistsAt(new Vector2(i, j)))
                {
                    tiles.Add(WorldController.Instance.GetTileAt(new Vector2(i, j)));
                }
                else
                {
                    return null;
                }
            }
        }

        return tiles;
    }

    private void GetNeighboursOfFogBatch(FogBatch f)
    {
        Vector2 pos = new Vector2(f.XMin - 1, f.ZMin);
        TileData t;

        if (WorldController.Instance.TileExistsAt(pos))
        {
            t = WorldController.Instance.GetTileAt(pos);

            if (t.FogBatch != null)
            {
                f.Neighbours.Add(WorldController.Instance.GetTileAt(pos).FogBatch);
            }
        }

        pos = new Vector2(f.XMin, f.ZMin - 1);

        if (WorldController.Instance.TileExistsAt(pos))
        {
            t = WorldController.Instance.GetTileAt(pos);

            if (t.FogBatch != null)
            {
                f.Neighbours.Add(WorldController.Instance.GetTileAt(pos).FogBatch);
            }
        }

        pos = new Vector2(f.XMax + 1, f.ZMax);

        if (WorldController.Instance.TileExistsAt(pos))
        {
            t = WorldController.Instance.GetTileAt(pos);

            if (t.FogBatch != null)
            {
                f.Neighbours.Add(WorldController.Instance.GetTileAt(pos).FogBatch);
            }
        }

        pos = new Vector2(f.XMax, f.ZMax + 1);

        if (WorldController.Instance.TileExistsAt(pos))
        {
            t = WorldController.Instance.GetTileAt(pos);

            if (t.FogBatch != null)
            {
                f.Neighbours.Add(WorldController.Instance.GetTileAt(pos).FogBatch);
            }
        }

        Debug.Log(f.name + ".Neighbours.Count is " + f.Neighbours.Count);
    }

    //Spawns the starting fog on the board
    public void SpawnStartingFog(StartConfiguration startConfiguration)
    {
        if (startConfiguration == StartConfiguration.OneSide)
        {
            //Spawns fog on one side of the board.
            for (int i = 0; i < xMax; i++)
            {
                SpawnFogUnit(i, zMax - 1);
            }
        }
        else if (startConfiguration == StartConfiguration.Corners)
        {
            //Corner spaces
            SpawnFogUnit(0, 0);
            SpawnFogUnit(0, zMax - 1);
            SpawnFogUnit(xMax - 1, 0);
            SpawnFogUnit(xMax - 1, zMax - 1);
        }
        else if (startConfiguration == StartConfiguration.FourCompassPoints)
        {
            //Four compass points
            SpawnFogUnit(Mathf.RoundToInt(xMax / 2), 0);
            SpawnFogUnit(Mathf.RoundToInt(xMax / 2), zMax - 1);
            SpawnFogUnit(0, Mathf.RoundToInt(zMax / 2));
            SpawnFogUnit(xMax - 1, Mathf.RoundToInt(zMax / 2));
        }
        else if (startConfiguration == StartConfiguration.EightCompassPoints)
        {
            //Corner spaces
            SpawnFogUnit(0, 0);
            SpawnFogUnit(0, zMax - 1);
            SpawnFogUnit(xMax - 1, 0);
            SpawnFogUnit(xMax - 1, zMax - 1);

            //Four compass points
            SpawnFogUnit(Mathf.RoundToInt(xMax / 2), 0);
            SpawnFogUnit(Mathf.RoundToInt(xMax / 2), zMax - 1);
            SpawnFogUnit(0, Mathf.RoundToInt(zMax / 2));
            SpawnFogUnit(xMax - 1, Mathf.RoundToInt(zMax / 2));
        }
        else if (startConfiguration == StartConfiguration.FourSides)
        {
            //Each side
            for (int i = 1; i < xMax - 1; i++)
            {
                SpawnFogUnit(i, 0);
                SpawnFogUnit(i, zMax - 1);
            }

            for (int i = 1; i < zMax - 1; i++)
            {
                SpawnFogUnit(0, i);
                SpawnFogUnit(xMax - 1, i);
            }

            //Corner spaces
            SpawnFogUnit(0, 0);
            SpawnFogUnit(0, zMax - 1);
            SpawnFogUnit(xMax - 1, 0);
            SpawnFogUnit(xMax - 1, zMax - 1);
        }
        else if (startConfiguration == StartConfiguration.SurroundingHub)
        {
            Vector3 hubPosition = GameObject.Find("Hub").transform.position;

            //Every space on the board
            for (int i = 0; i < xMax; i++)
            {
                for (int j = 0; j < zMax; j++)
                {
                    //Except those within a specified range of the hub
                    if (Vector3.Distance(hubPosition, new Vector3(i, hubPosition.y, j)) > surroundingHubRange)
                    {
                        //Debug.Log("Spawning Fog Unit at (" + i + "," + j + ").");
                        SpawnFogUnit(i, j);
                    }
                }
            }
        }
        else if (startConfiguration == StartConfiguration.FullBoard)
        {
            //Every space on the board
            for (int i = 0; i < xMax; i++)
            {
                for (int j = 0; j < zMax; j++)
                {
                    SpawnFogUnit(i, j);
                }
            }
        }

        if (fillType == FogFillType.Fluid)
        {
            foreach(FogUnit f in fogUnitsInPlay)
            {
                f.Health = fogHealthLimit;
            }
        }

        InvokeRepeating("UpdateFogExpansion", 0.1f, 0.5f);
    }

    private void SpawnFogUnit(int x, int z)
    {
        SpawnFogUnit(x, z, 0.0001f);
    }

    //Takes a fog unit and puts it on the board
    public void SpawnFogUnit(int x, int z, float health)
    {
        GameObject fGO = GetFogUnit().gameObject;
        FogUnit f = fGO.GetComponent<FogUnit>();
        Vector2 pos = new Vector2(x, z);
        TileData t = wc.GetTileAt(pos);

        fGO.transform.position = new Vector3(x, 0.13f, z);
        fGO.name = "FogUnit(" + x + "," + z + ")";

        f.gameObject.GetComponent<Renderer>().material = visibleMaterial;
        f.Location = t;
        f.Health = health;
        f.Spill = false;
        t.FogUnit = f;

        fogUnitsInPlay.Add(f);
        fogCoveredTiles.Add(t);
    }

    ///Start pooling methods borrowed / adapted from example
    //Pooling example: https://catlikecoding.com/unity/tutorials/object-pools/

    //Instantiates a fog unit that isn't on the board or in the pool
    private FogUnit CreateFogUnit()
    {
        FogUnit f = Instantiate<FogUnit>(fogUnitPrefab);
        f.transform.SetParent(this.transform, true);
        f.HealthLimit = fogHealthLimit;
        f.Fog = this;
        return f;
    }

    //Retrieves a fog unit from the pool, or asks for a new one if the pool is empty
    public FogUnit GetFogUnit()
    {
        FogUnit f;
        int lastAvailableIndex = fogUnitsInPool.Count - 1;

        if (lastAvailableIndex >= 0)
        {
            f = fogUnitsInPool[lastAvailableIndex];
            fogUnitsInPool.RemoveAt(lastAvailableIndex);
            f.gameObject.SetActive(true);
        }
        else
        {
            f = CreateFogUnit();
        }

        return f;
    }

    //Takes the fog unit off the board and puts it back in the pool
    public void ReturnFogUnitToPool(FogUnit f)
    {
        f.gameObject.name = "FogUnitInPool";

        foreach (TileData t in f.Location.AdjacentTiles)
        {
            if (t.FogUnit != null)
            {
                t.FogUnit.Spill = false;
            }
        }

        f.gameObject.SetActive(false);
        f.Location.FogUnit = null;
        f.gameObject.GetComponent<Renderer>().material = invisibleMaterial;
        f.Spill = true;

        fogUnitsInPool.Add(f);
        fogUnitsInPlay.Remove(f);
        fogCoveredTiles.Remove(f.Location);
    }

    ///End pooling methods borrowed / adapted from example

    // Update is called once per frame
    void Update()
    {
        UpdateFogFill();
    }

    private void UpdateFogFill()
    {
        Debug.Log("Updating Fog Fill");
        if (fogAccelerates && fogGrowth < 100)
        {
            fogGrowth += Time.deltaTime;
        }

        if (fillType == FogFillType.Block)
        {
            foreach (FogUnit f in fogUnitsInPlay)
            {
                f.Health += Time.deltaTime * fogGrowth;
            }
        }
        else if (fillType == FogFillType.Fluid)
        {
            foreach (FogUnit f in fogUnitsInPlay)
            {
                //Debug.Log(f.name + ". neighbours full " + f.NeighboursFull + ". f.Health " + f.Health + ". Spill Threshold: " + fogSpillThreshold);

                if (!f.NeighboursFull && f.Health >= fogSpillThreshold)
                {
                    int count = f.Location.AdjacentTiles.Count;
                    int fullCount = 0;

                    foreach (TileData t in f.Location.AdjacentTiles)
                    {
                        if (t.FogUnit != null)
                        {
                            if (t.FogUnit.Health < f.Health)
                            {
                                t.FogUnit.Health += Time.deltaTime * fogGrowth / count;
                            }
                            else if (t.FogUnit.Health >= fogHealthLimit)
                            {
                                fullCount++;
                            }
                        }
                        else if (t.FogBatch != null && t.FogBatch.Batched == true)
                        {
                            fullCount++;
                        }
                    }

                    if (count == fullCount)
                    {
                        f.NeighboursFull = true;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Invalid / unimplemented fog expansion type selected.");
        }
    }

    private void UpdateFogExpansion()
    {
        if (damageOn && fogUnitsInPlay.Count > 0)
        {
            foreach (FogUnit f in fogUnitsInPlay)
            {
                f.DamageBuilding();
            }
        }

        if (fogUnitsInPool.Count > 0 && configuration != StartConfiguration.FullBoard)
        {
            ExpandFog();
        }
        else if (fogUnitsInPlay.Count < xMax * zMax)
        {
            Debug.Log("Ran out of fog units. If the board isn't full, there must be some overlapping.");
        }
        else if (fogUnitsInPlay.Count > xMax * zMax)
        {
            Debug.Log("More fog units than board tiles. There must be some overlapping.");
        }
    }

    //Fog spills over onto adjacent tiles
    private void ExpandFog()
    {
        Debug.Log("ExpandFog()");
        List<TileData> newTiles = new List<TileData>();

        Debug.Log("FogUnitsInPlay: " + fogUnitsInPlay.Count);

        foreach (FogUnit f in fogUnitsInPlay)
        {
            Debug.Log(f.name + " Spill: " + f.Spill + ". Threshold: " + fogSpillThreshold + ". Health: " + f.Health + ".");
            if (f.Spill == false && f.Health >= fogSpillThreshold)
            {
                f.Spill = true;

                Debug.Log(f.name + " adjacent tiles: " + f.Location.AdjacentTiles.Count);

                foreach (TileData a in f.Location.AdjacentTiles)
                {
                    Debug.Log(a.Name + ". fogCoveredTiles.Contains(a): " + fogCoveredTiles.Contains(a) + ". newTiles.Contains(a): " + newTiles.Contains(a) + ".");
                    if ((!fogCoveredTiles.Contains(a)) && (!newTiles.Contains(a)) && (a.FogBatch == null || !a.FogBatch.Batched))
                    {
                        newTiles.Add(a);
                    }
                }
            }
        }

        //foreach (FogBatch f in fogBatches)
        //{
        //    if (f.Spill == false)
        //    {
        //        f.Spill = true;

        //        foreach (TileData a in f.EdgeTiles)
        //        {
        //            Debug.Log(a.Name + ". fogCoveredTiles.Contains(a): " + fogCoveredTiles.Contains(a) + ". newTiles.Contains(a): " + newTiles.Contains(a) + ".");
        //            if ((!fogCoveredTiles.Contains(a)) && (!newTiles.Contains(a)) && (a.FogBatch == null || !a.FogBatch.Batched))
        //            {
        //                newTiles.Add(a);
        //            }
        //        }
        //    }
        //}

        Debug.Log("New Tiles Count: " + newTiles.Count);

        if (newTiles.Count > 0)
        {
            foreach (TileData n in newTiles)
            {
                SpawnFogUnit(n.X, n.Z);        //SpawnFogUnit adds the tile spawned on to the list fogTiles
            }
        }
    }

    //private void UpdateFogUnitBatching()
    //{
    //    int interval = 5;

    //    for (int i = 0; i < xMax; i += interval)
    //    {
    //        for (int j = 0; i < zMax; j += interval)
    //        {
    //            UpdateFogUnitBatch(i, i + interval, j, j + interval);
    //        }
    //    }
    //}

    //private void UpdateFogUnitBatch(int minX, int maxX, int minY, int maxY)
    //{
    //    bool valid = true;
    //    int i = minX;
    //    int j = minY;

    //    while (valid && i < maxX)
    //    {
    //        while (valid && j < maxY)
    //        {
    //            if (WorldController.Instance.TileExistsAt(new Vector2(i, j)) && WorldController.Instance.GetTileAt(new Vector2(i,j)).FogUnit == null)
    //            {
    //                valid = false;
    //            }

    //            j++;
    //        }

    //        i++;
    //    }

    //    if (valid)
    //    {
    //        Debug.Log("Fog units (" + minX + "," + minY + ") to (" + minX + "," + minY + ") can be batched. If they're already batched, do nothing.");
    //    }
    //    else
    //    {
    //        Debug.Log("Fog units (" + minX + "," + minY + ") to (" + minX + "," + minY + ") can't be batched. If they are, they need to be un-batched");
    //    }
    //}
}
