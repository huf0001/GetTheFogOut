using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] private FogUnit fogUnitPrefab;
    [SerializeField] private float fogHealthLimit = 100f;
    [SerializeField] private float fogGrowth = 5f;
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material invisibleMaterial;

    //Container object fields
    private List<Tile> fogCoveredTiles = new List<Tile>();                      //i.e. tiles currently covered by fog
    private List<GameObject> fogUnitsInPlay = new List<GameObject>();           //i.e. currently active fog units on the board
    private List<FogUnit> fogUnitsInPool = new List<FogUnit>();                 //i.e. currently inactive fog units waiting for spawning

    //Object fields
    private WorldController wc;

    //Value fields
    private int xMax;
    private int zMax;


    private float tick = 0;

    //Sets up the fog at the start of the game. Called by WorldController to actually have it work.
    public void SpawnFog()
    {
        wc = GameObject.Find("GameManager").GetComponent<WorldController>();
        xMax = wc.Width;
        zMax = wc.Length;

        if (fogUnitsInPool.Count == 0)
        {
            PopulateFogPool();
        }

        SpawnStartingFog();
    }

    //Create the max no. of fog units the game should need
    private void PopulateFogPool()
    {
        for (int i = 0; i < xMax * zMax; i++)
        {
            fogUnitsInPool.Add(CreateFogUnit());
        }
    }

    //Spawns the starting fog on the board
    private void SpawnStartingFog()
    {
        //TODO: Create more sophisticated algorithm for selecting the starting points for the fog,
        //      even if it's just starting in the four corners or on all four sides.

        //Spawns fog on one side of the board.
        for (int i = 0; i < zMax; i++)
        {
            SpawnFogUnit(i, zMax - 1);
        }
    }

    //Takes a fog unit and puts it on the board
    private void SpawnFogUnit(int x, int z)
    {
        GameObject fGO = GetFogUnit().gameObject;
        FogUnit f = fGO.GetComponent<FogUnit>();
        Tile t = wc.GetTileAt(x, z).GetComponent<Tile>();

        fGO.transform.position = new Vector3(x, t.transform.position.y + 0.3f, z);
        fGO.name = "FogUnit(" + x + "," + z + ")";

        f.gameObject.GetComponent<Renderer>().material = visibleMaterial;
        f.Location = t;
        f.Health = 0.0001f;
        f.Spilled = false;
        f.Lerp = true;
        t.FogUnit = f;

        fogUnitsInPlay.Add(fGO);
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

        foreach (Tile t in f.Location.AdjacentTiles)
        {
            if (t.FogUnit != null)
            {
                t.FogUnit.Spilled = false;
            }
        }

        f.gameObject.SetActive(false);
        f.Location.FogUnit = null;
        f.gameObject.GetComponent<Renderer>().material = invisibleMaterial;
        f.Spilled = true;

        fogUnitsInPool.Add(f);
        fogUnitsInPlay.Remove(f.gameObject);
        fogCoveredTiles.Remove(f.Location);
    }

    ///End pooling methods borrowed / adapted from example

    // Update is called once per frame
    void Update()
    {
        tick += Time.deltaTime;

        if (fogGrowth < 100)
        {
            fogGrowth += Time.deltaTime;
        }

        foreach (GameObject f in fogUnitsInPlay)
        {
            f.GetComponent<FogUnit>().Health += Time.deltaTime * fogGrowth;
        }

        if (tick >= 1)
        {
            tick -= 1;

            if (fogUnitsInPool.Count > 0)
            {
                ExpandFog();
            }
            else
            {
                if (fogUnitsInPlay.Count < xMax * zMax)
                {
                    Debug.Log("Ran out of fog units. If the board isn't full, there must be some overlapping.");
                }
                else if (fogUnitsInPlay.Count > xMax * zMax)
                {
                    Debug.Log("More fog units than board tiles. There must be some overlapping.");
                }
            }
        }
    }

    //Fog spills over onto adjacent tiles
    private void ExpandFog()
    {
        FogUnit f;
        List<Tile> newTiles = new List<Tile>();

        foreach (GameObject g in fogUnitsInPlay)
        {
            f = g.GetComponent<FogUnit>();

            if (f.Health >= fogHealthLimit && f.Spilled == false)
            {
                f.Spilled = true;

                foreach (Tile a in f.Location.AdjacentTiles)
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
            foreach (Tile n in newTiles)
            {
                SpawnFogUnit(n.X, n.Z);        //SpawnFogUnit adds the tile spawned on to the list fogTiles
            }
        }
    }
}
