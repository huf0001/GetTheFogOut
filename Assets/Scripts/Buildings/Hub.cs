using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : PowerSource
{
    //Serialized Fields
    [SerializeField] private int maxPower = 100, maxMineral = 100, maxOrganic = 100, maxFuel = 100;
    [SerializeField] private int storedPower = 0, storedMineral = 0, storedOrganic = 0, storedFuel = 0;
    [SerializeField] private int powerChange = 0, mineralChange = 0, organicChange = 0, fuelChange = 0;
    
    //[Header("Building Costs")]
    [SerializeField] private int batteryPowerCost = 30, batteryMineralCost = 0, batteryOrganicCost = 0, batteryFuelCost = 0;
    [SerializeField] private int generatorPowerCost = 30, generatorMineralCost = 0, generatorOrganicCost = 0, generatorFuelCost = 0;
    [SerializeField] private int harvesterPowerCost = 50, harvesterMineralCost = 0, harvesterOrganicCost = 0, harvesterFuelCost = 0;
    [SerializeField] private int relayPowerCost = 10, relayMineralCost = 0, relayOrganicCost = 0, relayFuelCost = 0;
    [SerializeField] private int defencePowerCost = 50, defenceMineralCost = 0, defenceOrganicCost = 0, defenceFuelCost = 0;

    //[SerializeField] private List<Harvester> harvesters = new List<Harvester>();
    [SerializeField] private List<Battery> batteries = new List<Battery>();
    
    //Non-serialized fields
    //private Resource ResourceOn;
    private bool powerFull = false, mineralFull = false, organicFull = false, fuelFull = false;

    private Dictionary<string, int> batteryCosts = new Dictionary<string, int>();
    private Dictionary<string, int> generatorCosts = new Dictionary<string, int>();
    private Dictionary<string, int> harvesterCosts = new Dictionary<string, int>();
    private Dictionary<string, int> relayCosts = new Dictionary<string, int>();
    private Dictionary<string, int> defenceCosts = new Dictionary<string, int>();
    private Dictionary<BuildingType, Dictionary<string, int>> buildingsCosts = 
        new Dictionary<BuildingType, Dictionary<string, int>>();

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
    
    //public List<Harvester> Harvesters { get => harvesters; set => harvesters = value; }
    public List<Battery> Batteries { get => batteries; set => batteries = value; }
    public Dictionary<BuildingType, Dictionary<string, int>> BuildingsCosts { get => buildingsCosts; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        powerSource = null;

        // Set all building costs
        batteryCosts.Add("power", batteryPowerCost);
        batteryCosts.Add("mineral", batteryMineralCost);
        batteryCosts.Add("organic", batteryOrganicCost);
        batteryCosts.Add("fuel", batteryFuelCost);

        generatorCosts.Add("power", generatorPowerCost);
        generatorCosts.Add("mineral", generatorMineralCost);
        generatorCosts.Add("organic", generatorOrganicCost);
        generatorCosts.Add("fuel", generatorFuelCost);

        harvesterCosts.Add("power", harvesterPowerCost);
        harvesterCosts.Add("mineral", harvesterMineralCost);
        harvesterCosts.Add("organic", harvesterOrganicCost);
        harvesterCosts.Add("fuel", harvesterFuelCost);

        relayCosts.Add("power", relayPowerCost);
        relayCosts.Add("mineral", relayMineralCost);
        relayCosts.Add("organic", relayOrganicCost);
        relayCosts.Add("fuel", relayFuelCost);

        defenceCosts.Add("power", defencePowerCost);
        defenceCosts.Add("mineral", defenceMineralCost);
        defenceCosts.Add("organic", defenceOrganicCost);
        defenceCosts.Add("fuel", defenceFuelCost);

        buildingsCosts.Add(BuildingType.Battery, batteryCosts);
        buildingsCosts.Add(BuildingType.Generator, generatorCosts);
        buildingsCosts.Add(BuildingType.Harvester, harvesterCosts);
        buildingsCosts.Add(BuildingType.Relay, relayCosts);
        buildingsCosts.Add(BuildingType.Defence, defenceCosts);

        InvokeRepeating("ProcessUpkeep", 1f, 1f);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    private void ProcessUpkeep()
    {
        // Change the power based on each connected building

        //Reset resource changes
        powerChange = 0;
        fuelChange = 0;
        organicChange = 0;
        mineralChange = 0;

        // Process batteries
        maxPower = (batteries.Count * 10) + 100;

        //Provide hub's power contribution
        storedPower += upkeep;
        powerChange += upkeep;

        //Get connected generators, account for the power they supply
        List<Generator> connectedGenerators = base.GetGenerators();

        if (connectedGenerators.Count > 0)
        {
            storedPower += connectedGenerators[0].Upkeep * connectedGenerators.Count;
            powerChange += connectedGenerators[0].Upkeep * connectedGenerators.Count;

            foreach (Generator g in FindObjectsOfType<Generator>())
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

        //Get connected relays, account for the power they consume
        List<Relay> connectedRelays= base.GetRelays();

        if (connectedRelays.Count > 0)
        {
            foreach (Relay r in FindObjectsOfType<Relay>())
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
            foreach (Defence d in FindObjectsOfType<Defence>())
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
            foreach (Harvester h in FindObjectsOfType<Harvester>())
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

        CheckLimit();
    }

    //private void ProcessUpkeep()
    //{
    //    // Change the power based on each connect building
    //    // TODO: Make buildings stop working if no power
    //    int powerUpkeep = 5;

    //    foreach (Building building in suppliedBuildings)
    //    {
    //        powerUpkeep += building.Upkeep;

    //        if (powerUpkeep >= 0 || storedPower > 0)
    //        {
    //            building.PowerUp();
    //        }
    //        else
    //        {
    //            building.PowerDown();
    //        }

    //        if (building.BuildingType == BuildingType.Relay)
    //        {
    //            PowerSource relay = (PowerSource)building;

    //            if (relay != null)
    //            {
    //                foreach (Building b in relay.SuppliedBuildings)
    //                {
    //                    powerUpkeep += b.Upkeep;

    //                    if (powerUpkeep >= 0 || storedPower > 0)
    //                    {
    //                        b.PowerUp();
    //                    }
    //                    else
    //                    {
    //                        b.PowerDown();
    //                    }
    //                }
    //            }
    //        }

    //    }

    //    // Process batteries
    //    maxPower = (batteries.Count * 10) + 100;
    //    int mChange = 0;
    //    int fChange = 0;
    //    int oChange = 0;
    //    int pChange = 0;

    //    if (harvesters.Count != 0)
    //    {
    //        foreach (Harvester harvester in harvesters)
    //        {
    //            if (harvester.Powered)
    //            {
    //                ResourceOn = harvester.Location.Resource.ResourceType;

    //                switch (ResourceOn)
    //                {
    //                    case Resource.Power:
    //                        pChange += harvester.HarvestAmt;
    //                        break;
    //                    case Resource.Organic:
    //                        oChange += harvester.HarvestAmt;
    //                        break;
    //                    case Resource.Mineral:
    //                        mChange += harvester.HarvestAmt;
    //                        break;
    //                    case Resource.Fuel:
    //                        fChange += harvester.HarvestAmt;
    //                        break;
    //                }
    //            }
    //        }
    //    }

    //    fuelChange = fChange;
    //    organicChange = oChange;
    //    mineralChange = mChange;
    //    powerChange = pChange + powerUpkeep;

    //    storedFuel += fuelChange;
    //    storedOrganic += organicChange;
    //    StoredMineral += mineralChange;
    //    StoredPower += powerChange;

    //    //storedPower += 5;
    //    CheckLimit();
    //}

    public override bool SupplyingPower()
    {
        return true;
    }

    public void AddBattery()
    {
        maxPower += 10;
    }

    public void RemoveBattery()
    {
        maxPower -= 10;
    }

    public void CheckLimit()
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
