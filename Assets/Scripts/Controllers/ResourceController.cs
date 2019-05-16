using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceController : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private int maxPower = 100, maxMineral = 100, maxOrganic = 100, maxFuel = 100;
    [SerializeField] private int storedPower = 0, storedMineral = 0, storedOrganic = 0, storedFuel = 0;
    [SerializeField] private int powerChange = 0, mineralChange = 0, organicChange = 0, fuelChange = 0;

    [SerializeField] private List<Battery> batteries = new List<Battery>();
    [SerializeField] private List<Defence> defences = new List<Defence>();
    [SerializeField] private List<Generator> generators = new List<Generator>();
    [SerializeField] private List<Harvester> harvesters = new List<Harvester>();
    [SerializeField] private List<Relay> relays = new List<Relay>();
    private List<Building> buildings = new List<Building>();

    //Non-serialized fields
    private Hub hub = null;
    private bool powerFull = false, mineralFull = false, organicFull = false, fuelFull = false;

    //Public peroperties
    public static ResourceController Instance { get; protected set; }
    public int MaxPower { get => maxPower; set => maxPower = value; }

    public int StoredPower { get => storedPower; set => storedPower = value; }
    public int StoredMineral { get => storedMineral; set => storedMineral = value; }
    public int StoredOrganic { get => storedOrganic; set => storedOrganic = value; }
    public int StoredFuel { get => storedFuel; set => storedFuel = value; }

    public int PowerChange { get => powerChange; set => powerChange = value; }
    public int MineralChange { get => mineralChange; set => mineralChange = value; }
    public int OrganicChange { get => organicChange; set => organicChange = value; }
    public int FuelChange { get => fuelChange; set => fuelChange = value; }

    public List<Battery> Batteries { get => batteries; set => batteries = value; }
    public List<Defence> Defences { get => defences; set => defences = value; }
    public List<Generator> Generators { get => generators; set => generators = value; }
    public List<Harvester> Harvesters { get => harvesters; set => harvesters = value; }
    public List<Relay> Relays { get => relays; set => relays = value; }
    public List<Building> Buildings { get => buildings; set => buildings = value; }


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

    //Primary Logic of ResourceController------------------------------------------------------------------------------------------------------------

    private void ProcessUpkeep()
    {
        //Reset resource changes
        powerChange = 0;
        fuelChange = 0;
        organicChange = 0;
        mineralChange = 0;

        // Process batteries
        maxPower = (hub.GetBatteries().Count * 50) + 100;

        //Provide hub's power contribution
        storedPower += hub.Upkeep;
        powerChange += hub.Upkeep;

        //Get connected generators, account for the power they supply
        List<Generator> connectedGenerators = hub.GetGenerators();// hub.GetGenerators();
        //Debug.Log("ConnectedGenerators.Count is " + connectedGenerators.Count);

        if (connectedGenerators.Count > 0)
        {
            storedPower += connectedGenerators[0].Upkeep * connectedGenerators.Count;
            powerChange += connectedGenerators[0].Upkeep * connectedGenerators.Count;
        //    Debug.Log(generators.Count + " ge " + connectedGenerators.Count + " conge");
            foreach (Generator g in generators)
            {
                if (connectedGenerators.Contains(g))
                {
                    g.PowerUp();      
                }
                else
                {
                    //Debug.Log("Powering Down Generator from Hub.ProcessUpkeep");
                    g.PowerDown();
                }
            }
        }

        //Get connected relays, account for the power they consume
        List<Relay> connectedRelays = hub.GetRelays();

        if (connectedRelays.Count > 0)
        {
            foreach (Relay r in relays)
            {
                if (connectedRelays.Contains(r))
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

        //Get connected defences, account for the power they consume
        List<Defence> connectedDefences = hub.GetDefences();

        if (connectedDefences.Count > 0)
        {
            foreach (Defence d in defences)
            {
                if (connectedDefences.Contains(d))
                {
                    storedPower += d.Upkeep;
                    powerChange += d.Upkeep;

                    if (storedPower >= 0)
                    {
                        d.PowerUp();
                    }
                    else
                    {
                        d.PowerDown();
                    }
                }
                else
                {
                    d.PowerDown();
                }
            }
        }

        //Get connected harvesters, account for power they consume, store the resources they collect
        List<Harvester> connectedHarvesters = hub.GetHarvesters();
        //Debug.Log("ConnectedHarvesters.Count is " + connectedHarvesters.Count);

        if (connectedHarvesters.Count > 0)
        {
            foreach (Harvester h in harvesters)
            {
                if (connectedHarvesters.Contains(h))
                {
                    storedPower += h.Upkeep;
                    powerChange += h.Upkeep;

                    if (storedPower >= 0)
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
                                case Resource.Organic:
                                    organicChange += h.HarvestAmt * (int)h.Location.Resource.ResMultiplier;
                                    break;
                                case Resource.Mineral:
                                    mineralChange += h.HarvestAmt * (int)h.Location.Resource.ResMultiplier;
                                    h.Location.Resource.Health -= h.HarvestAmt;
                                    if (h.Location.Resource.Health == 0)
                                    {
                                        ResourceNode.Destroy(h.Location.Resource.gameObject);
                                    }
                                    break;
                                case Resource.Fuel:
                                    fuelChange += h.HarvestAmt * (int)h.Location.Resource.ResMultiplier;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        h.PowerDown();
                    }
                }
                else
                {
                    h.PowerDown();
                }
            }
        }

        storedFuel += fuelChange;
        storedOrganic += organicChange;
        StoredMineral += mineralChange;

        if (powerChange < 0)
        {
            // if (!overloadPlayed)
            // {
            //     audioOverload.Play();
            //     overloadPlayed = true;
            // }
        }
        // else
        // {
        //     overloadPlayed = false;
        // }
        CheckLimits();
    }

    public void AddBuilding(Building b)
    {
        buildings.Add(b);
        switch (b.BuildingType)
        {
            case BuildingType.Battery:
                batteries.Add(b as Battery);
                //maxPower += 10;
                break;
            case BuildingType.ArcDefence:
                defences.Add(b as Defence);
                break;
            case BuildingType.Generator:
                generators.Add(b as Generator);
                break;
            case BuildingType.Harvester:
                harvesters.Add(b as Harvester);
                break;
            case BuildingType.Relay:
                relays.Add(b as Relay);
                break;
            case BuildingType.RepelFan:
                defences.Add(b as Defence);
                break;
        }
    }

    public void RemoveBuilding(Building b)
    {
        Debug.Log("Removing building");
        Buildings.Remove(b);

        if (b.Location.PowerSource != null)
        {
            b.Location.PowerSource.SuppliedBuildings.Remove(b);
        }

        switch (b.BuildingType)
        {
            case BuildingType.Battery:
                batteries.Remove(b as Battery);
                if (batteries.Contains(b as Battery))
                {
                    Debug.Log("Battery removal failed");
                }
                //maxPower -= 10;
                break;
            case BuildingType.ArcDefence:
                defences.Remove(b as Defence);
                if (defences.Contains(b as Defence))
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
            case BuildingType.Relay:
                relays.Remove(b as Relay);
                if (relays.Contains(b as Relay))
                {
                    Debug.Log("Relay removal failed");
                }
                break;
            case BuildingType.RepelFan:
                defences.Remove(b as Defence);
                if (defences.Contains(b as Defence))
                {
                    Debug.Log("Defence removal failed");
                }
                break;
            default:
                Debug.Log("BuildingType not accepted");
                break;
        }
    }

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
        }

        if (storedFuel >= maxFuel)
        {
            storedFuel = maxFuel;
            fuelFull = true;
        }
        else
        {
            fuelFull = false;
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
        }

        if (storedOrganic >= maxOrganic)
        {
            storedOrganic = maxOrganic;
            organicFull = true;
        }
        else
        {
            organicFull = false;
        }

        if (storedPower < 0)
        {
            storedPower = 0;
        }

        if (storedFuel < 0)
        {
            storedFuel = 0;
        }

        if (storedMineral < 0)
        {
            storedMineral = 0;
        }

        if (storedOrganic < 0)
        {
            storedOrganic = 0;
        }
    }

    public bool IsWin()
    {
        if (powerFull && mineralFull && organicFull && fuelFull)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
