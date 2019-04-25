using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : PowerSource
{
    //Serialized Fields
    [SerializeField] private int maxPower = 100, maxMineral = 100, maxOrganic = 100, maxFuel = 100;
    [SerializeField] private int storedPower = 0, storedMineral = 0, storedOrganic = 0, storedFuel = 0;
    [SerializeField] private int powerChange = 0, mineralChange = 0, organicChange = 0, fuelChange = 0;

    [SerializeField] private List<Battery> batteries = new List<Battery>();
    [SerializeField] private List<Defence> defences = new List<Defence>();
    [SerializeField] private List<Generator> generators = new List<Generator>();
    [SerializeField] private List<Harvester> harvesters = new List<Harvester>();
    [SerializeField] private List<Relay> relays = new List<Relay>();    
    
    //Non-serialized fields
    private bool powerFull = false, mineralFull = false, organicFull = false, fuelFull = false;

    //Public peroperties
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

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        powerSource = null;

        InvokeRepeating("ProcessUpkeep", 1f, 1f);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    private void ProcessUpkeep()
    {
        //Reset resource changes
        powerChange = 0;
        fuelChange = 0;
        organicChange = 0;
        mineralChange = 0;

        // Process batteries
        maxPower = (base.GetBatteries().Count * 10) + 100;

        //Provide hub's power contribution
        storedPower += upkeep;
        powerChange += upkeep;

        //Get connected generators, account for the power they supply
        List<Generator> connectedGenerators = base.GetGenerators();     
        Debug.Log("ConnectedGenerators.Count is " + connectedGenerators.Count);

        if (connectedGenerators.Count > 0)
        {
            storedPower += connectedGenerators[0].Upkeep * connectedGenerators.Count;
            powerChange += connectedGenerators[0].Upkeep * connectedGenerators.Count;

            foreach (Generator g in generators)
            {
                if (connectedGenerators.Contains(g))
                {
                    g.PowerUp();
                }
                else
                {
                    Debug.Log("Powering Down Generator from Hub.ProcessUpkeep");
                    g.PowerDown();
                }
            }
        }

        //Get connected relays, account for the power they consume
        List<Relay> connectedRelays= base.GetRelays();

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
        List<Defence> connectedDefences = base.GetDefences();

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
        List<Harvester> connectedHarvesters = GetHarvesters();

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

        CheckLimits();
    }

    public override bool SupplyingPower()
    {
        return true;
    }

    public void AddBuilding(Building b)
    {
        switch (b.BuildingType)
        {
            case BuildingType.Battery:
                batteries.Add(b as Battery);
                //maxPower += 10;
                break;
            case BuildingType.Defence:
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
        }
    }

    public void RemoveBuilding(Building b)
    {
        switch (b.BuildingType)
        {
            case BuildingType.Battery:
                batteries.Remove(b as Battery);
                //maxPower -= 10;
                break;
            case BuildingType.Defence:
                defences.Remove(b as Defence);
                break;
            case BuildingType.Generator:
                generators.Remove(b as Generator);
                break;
            case BuildingType.Harvester:
                harvesters.Remove(b as Harvester);
                break;
            case BuildingType.Relay:
                relays.Remove(b as Relay);
                break;
        }
    }

    //public void AddBattery()
    //{
    //    maxPower += 10;
    //}

    //public void RemoveBattery()
    //{
    //    maxPower -= 10;
    //}

    public void CheckLimits()
    {
        if (storedPower >= maxPower)
        {
            storedPower = maxPower;
            powerFull = true;
        }
        else
        {
            powerFull = false;
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
        }
        else
        {
            mineralFull = false;
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

    public bool isDestroyed()
    {
        if (Health <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
