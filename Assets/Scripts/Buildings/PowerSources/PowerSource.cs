using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerSource : Building
{
    [SerializeField] protected float powerRange;

    [SerializeField] protected List<Building> suppliedBuildings = new List<Building>();

    public List<Building> SuppliedBuildings { get => suppliedBuildings; set => suppliedBuildings = value; }

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    //    showRange = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //ActivateTiles();
    }

    public override void Place()
    {
        base.Place();
        ActivateTiles();
    }

    public List<Battery> GetBatteries()
    {
        List<Battery> batteries = new List<Battery>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.Battery)
            {
                batteries.Add(b as Battery);
            }
            else if (b.BuildingType == BuildingType.Extender)
            {
                Relay r = b as Relay;
                batteries.AddRange(r.GetBatteries());
            }
        }

        return batteries;
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
            else if (b.BuildingType == BuildingType.Extender)
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
            if (b.BuildingType == BuildingType.Extender)
            {
                Relay r = b as Relay;
                relays.Add(r);
                relays.AddRange(r.GetRelays());
            }
        }

        return relays;
    }

    public List<ArcDefence> GetArcDefences()
    {
        List<ArcDefence> arcDefences = new List<ArcDefence>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.AirCannon)
            {
                arcDefences.Add(b as ArcDefence);
            }
            else if (b.BuildingType == BuildingType.Extender)
            {
                Relay r = b as Relay;
                arcDefences.AddRange(r.GetArcDefences());
            }
        }

        return arcDefences;
    }

    public List<RepelFan> GetRepelFans()
    {
        List<RepelFan> repelFans = new List<RepelFan>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.FogRepeller)
            {
                repelFans.Add(b as RepelFan);
            }
            else if (b.BuildingType == BuildingType.Extender)
            {
                Relay r = b as Relay;
                repelFans.AddRange(r.GetRepelFans());
            }
        }

        return repelFans;
    }

    public List<Defence> GetDefences()
    {
        List<Defence> defences = new List<Defence>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.AirCannon || b.BuildingType == BuildingType.FogRepeller )
            {
                defences.Add(b as Defence);
            }
            else if (b.BuildingType == BuildingType.Extender)
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
            else if (b.BuildingType == BuildingType.Extender)
            {
                Relay r = b as Relay;
                harvesters.AddRange(r.GetHarvesters());
            }
        }
        return harvesters;
    }

    public void UpdateTiles()
    { 
        foreach (TileData tile in WorldController.Instance.ActiveTiles)
            {
                tile.PowerUp(this as PowerSource);
            }

        foreach (TileData tile in WorldController.Instance.DisableTiles)
        {
            tile.PowerDown(this as PowerSource);
        }

        Debug.Log("both are off");
    }

    public void ActivateTiles()
    {
        List<TileData> tiles = location.CollectTilesInRange((int)powerRange);

        foreach (TileData tile in tiles)
        {
            tile.PowerUp(this as PowerSource);
            if ((tile.X == 28) || (tile.X == 29) || (tile.X == 30))
                {
                    if ((tile.Z == 20) || (tile.Z == 21) || (tile.Z == 22))
                    {
                        tile.PowerDown(this as PowerSource);
                        WorldController.Instance.DisableTiles.Add(tile);
                        if (tile.X == 29 && tile.Z == 21) //this block is to re-enable the hub tile , removed this if you don't want the player to see the hub
                        {
                            WorldController.Instance.DisableTiles.Remove(tile);
                            tile.PowerUp(this as PowerSource);
                        }
                    }
                }
            
            if (ObjectiveController.Instance.ShipComponent.activeSelf)
            {
                if ((tile.X == 45) || (tile.X == 46) || (tile.X == 47))
                {
                    if ((tile.Z == 19) || (tile.Z == 20) || (tile.Z == 21))
                    {
                        WorldController.Instance.tempPower = tile.PowerSource;
                        tile.PowerDown(tile.PowerSource);
                        WorldController.Instance.DisableTiles.Add(tile);
                        WorldController.Instance.ActiveTiles.Remove(tile);
                    }
                }
            }
            
            if (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
            {
                if ((tile.X == 45) || (tile.X == 46) || (tile.X == 47))
                {
                    if ((tile.Z == 19) || (tile.Z == 20) || (tile.Z == 21))
                    {
                        tile.PowerUp(tile.PowerSource);
                        Debug.Log("collected");
                    }
                }
            }
            
            if (!WorldController.Instance.ActiveTiles.Contains(tile))
            {
                if (!WorldController.Instance.DisableTiles.Contains(tile))
                {
                    WorldController.Instance.ActiveTiles.Add(tile);
                    if (tile.X == 29 && tile.Z == 21) // hide green tile in the middle of the hub.
                    {
                        WorldController.Instance.ActiveTiles.Remove(tile);
                    }
                }
            }
            
        }
    }

    public void DeactivateTiles()
    {
        List<TileData> tiles = location.CollectTilesInRange((int)powerRange);

        foreach (TileData tile in tiles)
        {
            tile.PowerDown(this as PowerSource);
            if (tile.PowerSource == null)
            {
                WorldController.Instance.ActiveTiles.Remove(tile);
            }
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
        if (this != newBuilding)
        {
            //Debug.Log("Plugging " + newBuilding.name + " into " + this.name);
            if (!suppliedBuildings.Contains(newBuilding))
            {
                suppliedBuildings.Add(newBuilding); 
            }
        }

        //if (suppliedBuildings.Contains(newBuilding))
        //{
        //    Debug.Log("plugged in successfully");
        //}
    }

    public void Unplug(Building unplug)
    {
        if (this != unplug)
        {
            //if (suppliedBuildings.Contains(unplug))
            //{
            //Debug.Log("Unplugging " + unplug.name + " from " + this.name);
            suppliedBuildings.Remove(unplug);
            //}

            //if (!suppliedBuildings.Contains(unplug))
            //{
            //    Debug.Log("Unplugged successfully");
            //}
        }
    }

    public abstract bool SupplyingPower();

    public void DismantlePowerSource()
    {
        DeactivateTiles();

        foreach(Building b in suppliedBuildings)
        {
            b.SetPowerSource();
        }
    }
}
