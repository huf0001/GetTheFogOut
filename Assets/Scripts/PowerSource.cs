using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerSource : Building
{
    [SerializeField] protected float powerRange;

    [SerializeField] protected List<Building> suppliedBuildings = new List<Building>();
    public List<Building> SuppliedBuildings { get => suppliedBuildings; }


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        if (placed)
        {
            DeactivateTiles();
        }

        foreach (Building building in suppliedBuildings)
        {
            building.SetPowerSource();
        }

        base.OnDestroy();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void Place()
    {
        base.Place();
        ActivateTiles();
    }

    private void ActivateTiles()
    {
        List<TileData> tiles = new List<TileData>();
        location.CollectTilesInRange(tiles, (int)powerRange);
        foreach (TileData tile in tiles)
        {
            tile.PowerUp(this as PowerSource);
            tile.Visited = false;
        }
    }

    private void DeactivateTiles()
    { 
        List<TileData> tiles = new List<TileData>();
        location.CollectTilesInRange(tiles, (int)powerRange);
        foreach (TileData tile in tiles)
        {
            tile.PowerDown(this as PowerSource);
            tile.Visited = false;
        }
    }

    public float PowerRange
    {
        get
        {
            return powerRange;
        }
    }

    public void PlugIn(Building newBuilding)
    {
        suppliedBuildings.Add(newBuilding);
    }

    public void Unplug(Building unplug)
    {
        if (suppliedBuildings.Contains(unplug))
        {
            suppliedBuildings.Remove(unplug);
        }
    }

    public abstract bool SupplyingPower();
}
