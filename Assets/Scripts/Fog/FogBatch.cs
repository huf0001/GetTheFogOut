//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FogBatch : FogEntity
//{
//    //Serialized Fields
//    [SerializeField] private GameObject visibleFog;

//    //Private Fields
//    [SerializeField] private bool allUnitsFull = false;
//    [SerializeField] private bool batched = false;
//    //private bool neighboursFull = false;
//    private int xMin;
//    private int xMax;
//    private int zMin;
//    private int zMax;
//    [SerializeField] private List<Building> buildings = new List<Building>();
//    [SerializeField] private List<FogBatch> neighbours = new List<FogBatch>();
//    [SerializeField] private List<TileData> tiles = new List<TileData>();

//    //Public Properties
//    public bool AllUnitsFull { get => allUnitsFull; }
//    public bool Batched { get => batched; }
//    //public bool NeighboursFull { get => neighboursFull; set => neighboursFull = value; }
//    public int XMin { get => xMin; set => xMin = value; }
//    public int XMax { get => xMax; set => xMax = value; }
//    public int ZMin { get => zMin; set => zMin = value; }
//    public int ZMax { get => zMax; set => zMax = value; }
//    public List<Building> Buildings { get => buildings; set => buildings = value; }
//    public List<FogBatch> Neighbours { get => neighbours; set => neighbours = value; }
//    public List<TileData> Tiles { get => tiles; set => tiles = value; }

//    void Awake()
//    {
//        InvokeRepeating("CheckBatching", Random.Range(5f, 10f), Random.Range(5f, 10f));
//    }

//    public void CheckBatching()
//    {
//        int i = 0;
//        allUnitsFull = true;
//        //Debug.Log("Checking batching for batch " + gameObject.name + ". tiles.Count: " + tiles.Count);
//        foreach(TileData t in tiles)
//        {
//            if (t.FogUnit == null || t.FogUnit.Health < healthLimit)
//            {
//                allUnitsFull = false;
//                visibleFog.SetActive(false);

//            //    if (t.FogUnit == null)
//            //    {
//            //        Debug.Log(t.Name + " has no fog unit. Do not batch.");
//            //    }
//            //    else if (t.FogUnit.Health < healthLimit)
//            //    {
//            //        Debug.Log(t.FogUnit.name + " is below max health. Do not batch.");
//            //    }

//            //    return;
//            //}
//            //else
//            //{
//            //    Debug.Log(t.FogUnit.name + " is at max health. Check validity of next fog unit.");
//            }

//            i++;
//        }
        
//        //Reached only if all fog units are full and return was not called.
//        batched = true;

//        foreach (FogBatch f in neighbours)
//        {
//            if (!f.AllUnitsFull)
//            {
//                batched = false;
//                visibleFog.SetActive(false);
//                return;
//            }
//        }

//        //Reached only if all neighbours are full and return was not called.
//        //Debug.Log(gameObject.name + " batching valid. Pooling fog units and rendering batch.");
//        ToBatch();
//    }

//    private void ToBatch()
//    {
//        foreach (TileData t in tiles)
//        {
//            t.FogUnit.ReturnToFogPool();
//        }

//        visibleFog.SetActive(true);
//        CancelInvoke();
//    }

//    public override void DamageBuilding()
//    {
//        //foreach (TileData t in tiles)
//        //{
//        //    if (t.Building != null)
//        //    {
//        //        StartCoroutine(Location.Building.DamageBuilding(damage * (base.Health / HealthLimit)));
//        //    }
//        //}

//        if (buildings.Count > 0)
//        {
//            foreach (Building b in buildings)
//            {
//                StartCoroutine(b.DamageBuilding(damage * (base.Health / HealthLimit)));
//            }
//        }
//    }

//    public override void DealDamage(float damage, Vector3 location)
//    {
//        ToUnits();

//        TileData t = WorldController.Instance.GetTileAt(location);

//        if (t.FogUnit != null)
//        {
//            t.FogUnit.DealDamage(damage, location);
//        }

//        foreach (TileData aT in t.AllAdjacentTiles)
//        {
//            aT.FogUnit.DealDamage(damage, location);
//        }
//    }

//    //private void ToEdgeUnits()
//    //{
//    //    foreach (TileData t in edgeTiles)
//    //    {
//    //        fog.SpawnFogUnit(t.X, t.Z, healthLimit);
//    //    }
//    //}

//    private void ToUnits()
//    {
//        foreach (TileData t in tiles)
//        {
//            //if (t.FogUnit == null)
//            //{
//            fog.SpawnFogUnit(t.X, t.Z, healthLimit);
//            //}
//        }

//        visibleFog.SetActive(false);
//        InvokeRepeating("CheckBatching", 5f, 5f);
//    }
//}
