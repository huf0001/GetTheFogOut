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
        DeactivateTiles();
        //TODO: go through all objects being supplied power, and unplug from them; note that they
        //have to have their power sources replaced if there is one available, or be switched off if
        //there isn't one available
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

    public List<Generator> GetGenerators()
    {
        List<Generator> generators = new List<Generator>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.Generator)
            {
                generators.Add(b as Generator);
            }
            else if (b.BuildingType == BuildingType.Relay)
            {
                Relay r = b as Relay;
                generators.AddRange(r.GetGenerators());
            }
        }

        return generators;
    }

    public List<Relay> GetRelays()
    {
        List<Relay> relays = new List<Relay>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.Relay)
            {
                Relay r = b as Relay;
                relays.Add(r);
                relays.AddRange(r.GetRelays());
            }
        }

        return relays;
    }

    public List<Defence> GetDefences()
    {
        List<Defence> defences = new List<Defence>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.Defence)
            {
                defences.Add(b as Defence);
            }
            else if (b.BuildingType == BuildingType.Relay)
            {
                Relay r = b as Relay;
                defences.AddRange(r.GetDefences());
            }
        }

        return defences;
    }

    public List<Harvester> GetHarvesters()
    {
        List<Harvester> harvesters = new List<Harvester>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.Harvester)
            {
                harvesters.Add(b as Harvester);
            }
            else if (b.BuildingType == BuildingType.Relay)
            {
                Relay r = b as Relay;
                harvesters.AddRange(r.GetHarvesters());
            }
        }

        return harvesters;
    }

    private void ActivateTiles()
    {
        // NEEDS TO BE UPDATES TO NEW TILES
        //Collider[] tilesToActivate = Physics.OverlapSphere(transform.position, powerRange);

        //foreach (Collider c in tilesToActivate)
        //{
        //    if (c.gameObject.GetComponent<Tile>() != null)
        //    {
        //        c.gameObject.GetComponent<Tile>().PowerUp(this as PowerSource);
        //    }
        //}

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
        // NEEDS TO BE UPDATES TO NEW TILES
        //Collider[] tilesToDeactivate = Physics.OverlapSphere(transform.position, powerRange);

        //foreach (Collider c in tilesToDeactivate)
        //{
        //    if (c.gameObject.GetComponent<Tile>() != null)
        //    {
        //        c.gameObject.GetComponent<Tile>().PowerDown(this as PowerSource);
        //    }
        //}

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
