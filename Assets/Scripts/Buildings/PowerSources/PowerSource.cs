using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerSource : Building
{
    [SerializeField] protected float powerRange;

    [SerializeField] protected List<Building> suppliedBuildings = new List<Building>();

    public bool isConnectedToHub;

    public List<Building> SuppliedBuildings { get => suppliedBuildings; set => suppliedBuildings = value; }
    
    
    public override void Place()
    {
        base.Place();
        if (powerSource == WorldController.Instance.Hub) isConnectedToHub = true;
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
            else if (b.BuildingType == BuildingType.Extender)
            {
                Relay r = b as Relay;
                generators.AddRange(r.GetGenerators());
            }
        }

        return generators;
    }

    public List<Relay> GetExtenders()
    {
        List<Relay> relays = new List<Relay>();

        foreach (Building b in suppliedBuildings)
        {
            if (b.BuildingType == BuildingType.Extender)
            {
                Relay r = b as Relay;
                relays.Add(r);
                relays.AddRange(r.GetExtenders());
            }
        }

        return relays;
    }

    public List<ArcDefence> GetMortars()
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
                arcDefences.AddRange(r.GetMortars());
            }
        }

        return arcDefences;
    }

    public List<RepelFan> GetPulseDefences()
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
                repelFans.AddRange(r.GetPulseDefences());
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

    public void ActivateTiles()
    {
        powered = true;
        List<TileData> tiles = location.CollectTilesInRange((int)powerRange);
        tiles.Remove(location);

        foreach (TileData tile in tiles)
        {
            if (tile.Position != location.Position)
            {
                tile.PowerUp(this as PowerSource);
            }
            /*
            if ((tile.X == 28) || (tile.X == 29) || (tile.X == 30))
            {
                if ((tile.Z == 20) || (tile.Z == 21) || (tile.Z == 22))
                {
                    tile.PowerDown(this as PowerSource);
               //     WorldController.Instance.DisableTiles.Add(tile);
                    if (tile.X == 29 && tile.Z == 21) //this block is to re-enable the hub tile , removed this if you don't want the player to see the hub
                    {
                        WorldController.Instance.DisableTiles.Remove(tile);
                        tile.PowerUp(this as PowerSource);
                    }
                }
            }*/
            tile.PowerUp(this as PowerSource);
            if (!WorldController.Instance.ActiveTiles.Contains(tile))
{
    if (!WorldController.Instance.DisableTiles.Contains(tile))
    {
        WorldController.Instance.ActiveTiles.Add(tile);
//        if (tile.X == 29 && tile.Z == 21) // hide green tile in the middle of the hub.
//        {
 //           WorldController.Instance.ActiveTiles.Remove(tile);
  //      }
    }
}

            if (tile.Building)
            {
                if (tile.Building.BuildingType == BuildingType.Extender)
                {
                    PowerSource relay = (PowerSource) tile.Building;
                    if (relay.AreYouConnectedToHub() && !relay.powered)
                    {
                        relay.CreateWire();
                        if (relay.powerSource == this)
                        {
                            relay.ActivateTiles();   
                        }
                    }
                }
            }
        }
    }

    public void DeactivateTiles()
    {
        powered = false;
        List<TileData> tiles = location.CollectTilesInRange((int)powerRange);

        foreach (TileData tile in tiles)
        {
            tile.PowerDown(this as PowerSource);
            if (tile.PowerSource == null)
            {
                WorldController.Instance.ActiveTiles.Remove(tile);
            }

            if (tile.Building)
            {
                if (tile.Building.BuildingType == BuildingType.Extender && tile.Building.Powered)
                {
                    PowerSource relay = tile.Building as PowerSource;
                    if (!relay.AreYouConnectedToHub())
                    {
                        relay.DeactivateTiles();
                    }
                }
            }
        }

        //isConnectedToHub = false;
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
            if (!suppliedBuildings.Contains(newBuilding))
            {
                suppliedBuildings.Add(newBuilding); 
            }
        }

    }

    public void Unplug(Building unplug)
    {
        if (this != unplug)
        {
            suppliedBuildings.Remove(unplug);
        }
    }

    public abstract bool SupplyingPower();

    public override void DismantleBuilding()
    {
        isConnectedToHub = false;
        DestroyWires();
        
        DeactivateTiles();

        List<Building> toRemove = new List<Building>();
        foreach (var building in suppliedBuildings)
        {
            toRemove.Add(building);
        }
        
        Debug.Log(location.Position.x + location.Position.z);
        foreach (var building in toRemove)
        {
            building.DestroyWires();
            if (building.BuildingType == BuildingType.Extender)
            {
                PowerSource relay = (PowerSource) building;

                if (!relay.AreYouConnectedToHub())
                {
                    relay.DeactivateTiles();
                }
                else
                {
                    relay.ActivateTiles();
                }
            }
            building.CreateWire();
        }
        
        base.DismantleBuilding();
    }

    public bool AreYouConnectedToHub()
    {
        if (this == WorldController.Instance.Hub)
        {
            return true;
        }
        
        if (isConnectedToHub)
        {
            return true;
        }
        else
        {
            if (powerSource == null)
            {
                return false;
            }
            else
            {
                return powerSource.AreYouConnectedToHub();
            }
        }
    }
}
