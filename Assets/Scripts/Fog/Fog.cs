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
    [SerializeField] private FogUnit fogUnitPrefab;
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
    private List<FogUnit> fogUnitsInPlay = new List<FogUnit>();                 //i.e. currently active fog units on the board
    private List<FogUnit> fogUnitsInPool = new List<FogUnit>();                 //i.e. currently inactive fog units waiting for spawning

    //Object fields
    private WorldController wc;

    //Value fields
    private int xMax;
    private int zMax;
    private float tick = 0;

    //Public properties
    public FogExpansionDirection ExpansionDirection { get => expansionDirection; }
    public FogFillType FillType { get => fillType; }

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
        SpawnStartingFog(configuration);
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
    }

    private void SpawnFogUnit(int x, int z)
    {
        SpawnFogUnit(x, z, 0.0001f);
    }

    //Takes a fog unit and puts it on the board
    private void SpawnFogUnit(int x, int z, float health)
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

        UpdateFogExpansion();

        UpdateFogUnitBatching();

        //foreach (FogUnit f in fogUnitsInPlay)
        //{
        //    FogOfWarManager.UpdatePosition(f.transform.position, 5);
        //}
    }

    private void UpdateFogFill()
    {
        tick += Time.deltaTime;

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
                if (f.Health >= fogSpillThreshold)
                {
                    int count = f.Location.AdjacentTiles.Count;

                    foreach (TileData t in f.Location.AdjacentTiles)
                    {
                        if (t.FogUnit != null)
                        {
                            if (t.FogUnit.Health + (Time.deltaTime * fogGrowth / count) <= f.Health)
                            {
                                t.FogUnit.Health += Time.deltaTime * fogGrowth / count;
                            }
                            else if (t.FogUnit.Health < f.Health)
                            {
                                t.FogUnit.Health += (f.Health - t.FogUnit.Health);
                            }
                        }
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
        if (tick >= 1)
        {
            tick -= 1;

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
    }

    //Fog spills over onto adjacent tiles
    private void ExpandFog()
    {
        List<TileData> newTiles = new List<TileData>();

        foreach (FogUnit f in fogUnitsInPlay)
        {
            if (f.Health >= fogSpillThreshold && f.Spill == false)
            {
                f.Spill = true;

                foreach (TileData a in f.Location.AdjacentTiles)
                {
                    if ((!fogCoveredTiles.Contains(a)) && (!newTiles.Contains(a)))
                    {
                        newTiles.Add(a);
                        continue;
                    }
                }
            }
        }

        if (newTiles.Count > 0)
        {
            foreach (TileData n in newTiles)
            {
                SpawnFogUnit(n.X, n.Z);        //SpawnFogUnit adds the tile spawned on to the list fogTiles
            }
        }
    }

    private void UpdateFogUnitBatching()
    {
        int interval = 5;

        for (int i = 0; i < xMax; i += interval)
        {
            for (int j = 0; i < zMax; j += interval)
            {
                UpdateFogUnitBatch(i, i + interval, j, j + interval);
            }
        }
    }

    private void UpdateFogUnitBatch(int minX, int maxX, int minY, int maxY)
    {
        bool valid = true;
        int i = minX;
        int j = minY;

        while (valid && i < maxX)
        {
            while (valid && j < maxY)
            {
                if (WorldController.Instance.TileExistsAt(new Vector2(i, j)) && WorldController.Instance.GetTileAt(new Vector2(i,j)).FogUnit == null)
                {
                    valid = false;
                }

                j++;
            }

            i++;
        }

        if (valid)
        {
            Debug.Log("Fog units (" + minX + "," + minY + ") to (" + minX + "," + minY + ") can be batched. If they're already batched, do nothing.");
        }
        else
        {
            Debug.Log("Fog units (" + minX + "," + minY + ") to (" + minX + "," + minY + ") can't be batched. If they are, they need to be un-batched");
        }
    }
}
