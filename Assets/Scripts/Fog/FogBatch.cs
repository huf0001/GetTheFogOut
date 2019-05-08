using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogBatch : FogEntity
{
    //Serialized Fields
    //[SerializeField] private Material visibleMaterial;
    //[SerializeField] private Material invisibleMaterial;
    [SerializeField] private GameObject visibleFog;

    //Private Fields
    private bool batched = false;
    private List<TileData> tiles = new List<TileData>();

    //Public Properties
    public bool Batched { get => batched; }
    public List<TileData> Tiles { get => tiles; set => tiles = value; }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("CheckBatching", 5f, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckBatching()
    {
        Debug.Log("Checking Batching");
        batched = true;
        int i = 0;

        while (batched && i < tiles.Count)
        {
            if (tiles[i].FogUnit == null || tiles[i].FogUnit.Health < healthLimit)
            {
                batched = false;
            }

            i++;
        }

        if (batched)
        {
            ToBatch();
        }
    }

    private void ToBatch()
    {
        foreach (TileData t in tiles)
        {
            t.FogUnit.ReturnToFogPool();
        }

        //GetComponent<Renderer>().material = visibleMaterial;
        visibleFog.SetActive(true);
    }

    public override void DamageBuilding()
    {
        foreach (TileData t in tiles)
        {
            if (t.Building != null)
            {
                StartCoroutine(Location.Building.DamageBuilding(damage * (base.Health / HealthLimit)));
            }
        }
        
    }

    public override void DealDamage(float damage, Vector3 location)
    {
        ToUnits();

        TileData t = WorldController.Instance.GetTileAt(location);

        if (t.FogUnit != null)
        {
            t.FogUnit.DealDamage(damage, location);
        }

        foreach (TileData aT in t.AllAdjacentTiles)
        {
            aT.FogUnit.DealDamage(damage, location);
        }
    }

    private void ToUnits()
    {
        foreach (TileData t in tiles)
        {
            fog.SpawnFogUnit(t.X, t.Z, healthLimit);
        }

        //GetComponent<Renderer>().material = invisibleMaterial;
        visibleFog.SetActive(false);
    }
}
