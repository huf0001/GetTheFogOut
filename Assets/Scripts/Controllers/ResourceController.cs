using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceController : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private int maxPower;
    [SerializeField] private int maxMineral;
    [SerializeField] private int storedPower;
    [SerializeField] private int storedMineral;
    [SerializeField] private int powerChange;
    [SerializeField] private int mineralChange;

    [SerializeField] private List<ArcDefence> mortars = new List<ArcDefence>();
    [SerializeField] private List<RepelFan> pulseDefences = new List<RepelFan>();
    [SerializeField] private List<Generator> generators = new List<Generator>();
    [SerializeField] private List<Harvester> harvesters = new List<Harvester>();
    [SerializeField] private List<Relay> extenders = new List<Relay>();

    [SerializeField] private bool hvtSelfDestroy = false;

    //Non-serialized fields
    private Hub hub = null;
    private bool powerFull = false;
    private bool mineralFull = false;

    private List<Building> buildings = new List<Building>();

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public static ResourceController Instance { get; protected set; }

    //public int MaxPower { get => maxPower; set => maxPower = value; }
    public int StoredPower { get => storedPower; set => storedPower = value; }
    public int StoredMineral { get => storedMineral; set => storedMineral = value; }
    public int PowerChange { get => powerChange; set => powerChange = value; }
    public int MineralChange { get => mineralChange; set => mineralChange = value; }

    public List<Building> Buildings { get => buildings; set => buildings = value; }
    public List<Relay> Extenders { get => extenders; }
    public List<Generator> Generators { get => generators; set => generators = value; }
    public List<Harvester> Harvesters { get => harvesters; set => harvesters = value; }
    //public List<ArcDefence> Mortars { get => mortars; set => mortars = value; }
    //public List<RepelFan> PulseDefences { get => pulseDefences; set => pulseDefences = value; }


    // [SerializeField] protected AudioSource audioMaxPower;
    // [SerializeField] protected AudioSource audioMaxMineral;
    // [SerializeField] protected AudioSource audioOverload;
    // private bool maxPowPlayed = false, maxMinPlayed = false, overloadPlayed = false;

    //Start-Up Methods-------------------------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more resource managers.");
        }

        Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        hub = WorldController.Instance.Hub;
        InvokeRepeating("ProcessUpkeep", 1f, 1f);
    }

    //Recurring Methods - Processing Upkeep----------------------------------------------------------------------------------------------------------

    //Manages the buildings, calculating power supply and supplying it as needed
    private void ProcessUpkeep()
    {
        //Reset resource changes
        powerChange = 0;
        mineralChange = 0;

        //Get connected buildings
        List<Generator> connectedGenerators = hub.GetGenerators();
        List<Relay> connectedRelays = hub.GetExtenders();
        List<ArcDefence> connectedMortars = hub.GetMortars();
        List<RepelFan> connectedPulseDefences = hub.GetPulseDefences();
        List<Harvester> connectedHarvesters = hub.GetHarvesters();
        
        //Process power and power supplying
        CalculatePowerSupply(connectedGenerators, connectedRelays);
        CalculateUpkeep(connectedMortars, connectedPulseDefences, connectedHarvesters);
        SupplyPower(connectedMortars, connectedPulseDefences, connectedHarvesters);        
        
        //Makes sure power and minerals don't exceed their maximum values
        CheckLimits();
    }

    //Calculates how much power is supplied by all buildings
    private void CalculatePowerSupply(List<Generator> connectedGenerators, List<Relay> connectedExtenders)
    {
        //Provide hub's power contribution
        storedPower += hub.Upkeep;
        powerChange += hub.Upkeep;
        
        //Get Generators' power contributions
        foreach (Generator generator in connectedGenerators)
        {
            storedPower += generator.Upkeep * generator.OverclockValue;
            powerChange += generator.Upkeep * generator.OverclockValue;
        }

        //Supply power to connected generators
        if (connectedGenerators.Count > 0)
        {
            foreach (Generator g in generators)
            {
                if (connectedGenerators.Contains(g))
                {
                    g.PowerUp();
                }
                else
                {
                    g.PowerDown();
                }
            }
        }

        //Supply power to connected extenders
        if (connectedExtenders.Count > 0)
        {
            foreach (Relay r in extenders)
            {
                if (connectedExtenders.Contains(r))
                {
                    storedPower += r.Upkeep;
                    powerChange += r.Upkeep;

                    if (storedPower >= 0)
                    {
                        r.PowerUp();
                    }
                    else
                    {
                        r.PowerDown();
                    }
                }
                else
                {
                    r.PowerDown();
                }
            }
        }
    }

    //Calculates how much power is used by all buildings
    private void CalculateUpkeep(List<ArcDefence> connectedMortars, List<RepelFan> connectedPulseDefences, List<Harvester> connectedHarvesters)
    {
        //Gets the power drain by the mortars
        if (connectedMortars.Count > 0)
        {
            foreach (ArcDefence m in mortars)
            {
                if (connectedMortars.Contains(m))
                {
                    storedPower += m.Upkeep;
                    powerChange += m.Upkeep;
                }
            }
        }

        //Gets the power drain by the pulse defences
        if (connectedPulseDefences.Count > 0)
        {
            foreach (RepelFan pd in pulseDefences)
            {
                if (connectedPulseDefences.Contains(pd))
                {
                    storedPower += pd.Upkeep;
                    powerChange += pd.Upkeep;
                }
            }
        }

        //Gets the power drain by the harvesters
        if (connectedHarvesters.Count > 0)
        {
            foreach (Harvester h in harvesters)
            {
                if (connectedHarvesters.Contains(h))
                {
                    storedPower += h.Upkeep;
                    powerChange += h.Upkeep;                    
                }
            }
        }
    }

    //Supplies powers to buildings based on the current stored power
    private void SupplyPower(List<ArcDefence> connectedMortars, List<RepelFan> connectedPulseDefences, List<Harvester> connectedHarvesters)
    {
        //Powers mortars
        if (connectedMortars.Count > 0)
        {
            if (storedPower > 0)
            {
                foreach (ArcDefence ac in mortars)
                {
                    if (connectedMortars.Contains(ac))
                    {
                        ac.PowerUp();
                    }
                    else
                    {
                        ac.PowerDown();
                    }
                }
            }
            else
            {
                foreach (ArcDefence ac in mortars)
                {
                    ac.PowerDown();
                }
            }
        }

        //Powers pulse defences
        if (connectedPulseDefences.Count > 0)
        {
            if (storedPower > 25)
            {
                foreach (RepelFan pd in pulseDefences)
                {
                    if (connectedPulseDefences.Contains(pd))
                    {
                        pd.PowerUp();
                    }
                    else
                    {
                        pd.PowerDown();
                    }
                }
            }
            else
            {
                foreach (RepelFan pd in pulseDefences)
                {
                    pd.PowerDown();
                }
            }
        }

        //Powers harvesters
        if (connectedHarvesters.Count > 0)
        {
            bool needDestroy = false;

            if (storedPower > 75)
            {
                foreach (Harvester h in harvesters)
                {
                    if (connectedHarvesters.Contains(h))
                    {
                        h.PowerUp();

                        if (h.Location.Resource != null)
                        {
                            switch (h.Location.Resource.ResourceType)
                            {
                                case Resource.Power:
                                    storedPower += h.HarvestAmt * (int)h.Location.Resource.ResMultiplier;
                                    powerChange += h.HarvestAmt * (int)h.Location.Resource.ResMultiplier;
                                    break;
                                case Resource.Mineral:
                                    mineralChange += h.HarvestAmt * h.OverclockValue * (int)h.Location.Resource.ResMultiplier;
                                    h.Location.Resource.Health -= h.HarvestAmt * h.OverclockValue;

                                    if (h.Location.Resource.Health <= 0)
                                    {
                                        ResourceNode.Destroy(h.Location.Resource.gameObject);
                                        h.TurnOnMineralIndicator();
                                        h.ShutdownBuilding();
                                        needDestroy = true;
                                        //    h.Location.Building.ShutdownBuilding();   // if you dont want to destroy, this line, insteat, will turn power off
                                    }

                                    break;
                            }
                        }
                    }
                    else
                    {
                        h.PowerDown();
                    }
                }
                
                storedMineral += mineralChange;
            }
            else
            {
                foreach (Harvester h in harvesters)
                {
                    h.PowerDown();
                }
            }

            if (hvtSelfDestroy)
            {
                if (needDestroy)
                {
                    MouseController.Instance.ReturnCost(harvesters[0].Location);
                    MouseController.Instance.RemoveBulding(harvesters[0]);
                    //    harvesters[0].Location.Building.DismantleBuilding();  //alternatively
                }
            }
        }
    }

    //Checks that power and minerals are within their maximum values
    public void CheckLimits()
    {
        if (storedPower >= maxPower)
        {
            storedPower = maxPower;
            powerFull = true;
            // if (!maxPowPlayed)
            // {
            //     audioMaxPower.Play();
            //     maxPowPlayed = true;
            // }
        }
        else
        {
            powerFull = false;
            // maxPowPlayed = false;

            if (storedPower < 0)
            {
                storedPower = 0;
            }
        }

        if (storedMineral >= maxMineral)
        {
            storedMineral = maxMineral;
            mineralFull = true;
            // if (!maxMinPlayed)
            // {
            //     audioMaxMineral.Play();
            //     maxMinPlayed = true;
            // }
        }
        else
        {
            mineralFull = false;
            // maxMinPlayed = false;

            if (storedMineral < 0)
            {
                storedMineral = 0;
            }
        }
    }

    //Triggered Methods------------------------------------------------------------------------------------------------------------------------------

    //Adds a building to the list of its type of building
    public void AddBuilding(Building b)
    {
        buildings.Add(b);
        switch (b.BuildingType)
        {
            case BuildingType.AirCannon:
                mortars.Add(b as ArcDefence);
                break;
            case BuildingType.Generator:
                generators.Add(b as Generator);
                break;
            case BuildingType.Harvester:
                harvesters.Add(b as Harvester);
                break;
            case BuildingType.Extender:
                extenders.Add(b as Relay);
                break;
            case BuildingType.FogRepeller:
                pulseDefences.Add(b as RepelFan);
                break;
        }
    }

    //Removes a building from the list of its type of building
    public void RemoveBuilding(Building b)
    {
        Debug.Log("Removing building");
        buildings.Remove(b);

        if (b.Location.PowerSource != null)
        {
            b.Location.PowerSource.SuppliedBuildings.Remove(b);
        }

        switch (b.BuildingType)
        {
            case BuildingType.AirCannon:
                mortars.Remove(b as ArcDefence);
                if (mortars.Contains(b as ArcDefence))
                {
                    Debug.Log("Defence removal failed");
                }
                break;
            case BuildingType.Generator:
                generators.Remove(b as Generator);
                if (generators.Contains(b as Generator))
                {
                    Debug.Log("Generator removal failed");
                }
                break;
            case BuildingType.Harvester:
                harvesters.Remove(b as Harvester);
                if (harvesters.Contains(b as Harvester))
                {
                    Debug.Log("Harvester removal failed");
                }
                break;
            case BuildingType.Extender:
                extenders.Remove(b as Relay);
                if (extenders.Contains(b as Relay))
                {
                    Debug.Log("Relay removal failed");
                }
                break;
            case BuildingType.FogRepeller:
                pulseDefences.Remove(b as RepelFan);
                if (pulseDefences.Contains(b as RepelFan))
                {
                    Debug.Log("Defence removal failed");
                }
                break;
            default:
                Debug.Log("BuildingType not accepted");
                break;
        }
    }
}
