using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : PowerSource
{
    [SerializeField] private int maxPower = 100, maxMineral = 100, maxOrganic = 100, maxFuel = 100;
    [SerializeField] private int storedPower = 0, storedMineral = 0, storedOrganic = 0, storedFuel = 0;
    [SerializeField] private int powerChange = 0, mineralChange = 0, organicChange = 0, fuelChange = 0;

    private bool powerFull = false, mineralFull = false, organicFull = false, fuelFull = false;
    
    [Header("Building Costs")]
    [SerializeField] private int batteryPowerCost = 30, batteryMineralCost = 0, batteryOrganicCost = 0, batteryFuelCost = 0;
    [SerializeField] private int generatorPowerCost = 30, generatorMineralCost = 0, generatorOrganicCost = 0, generatorFuelCost = 0;
    [SerializeField] private int harvesterPowerCost = 50, harvesterMineralCost = 0, harvesterOrganicCost = 0, harvesterFuelCost = 0;
    [SerializeField] private int relayPowerCost = 10, relayMineralCost = 0, relayOrganicCost = 0, relayFuelCost = 0;
    [SerializeField] private int defencePowerCost = 0, defenceMineralCost = 0, defenceOrganicCost = 0, defenceFuelCost = 0;

    private Resource ResourceOn;

    private Dictionary<string, int> batteryCosts = new Dictionary<string, int>();
    private Dictionary<string, int> generatorCosts = new Dictionary<string, int>();
    private Dictionary<string, int> harvesterCosts = new Dictionary<string, int>();
    private Dictionary<string, int> relayCosts = new Dictionary<string, int>();
    private Dictionary<string, int> defenceCosts = new Dictionary<string, int>();
    private Dictionary<BuildingType, Dictionary<string, int>> buildingsCosts = 
        new Dictionary<BuildingType, Dictionary<string, int>>();

    [SerializeField] private List<Harvester> harvesters = new List<Harvester>();
    [SerializeField] private List<Battery> batteries = new List<Battery>();

    public int MaxPower { get => maxPower; set => maxPower = value; }

    public int StoredPower { get => storedPower; set => storedPower = value; }
    public int StoredMineral { get => storedMineral; set => storedMineral = value; }
    public int StoredOrganic { get => storedOrganic; set => storedOrganic = value; }
    public int StoredFuel { get => storedFuel; set => storedFuel = value; }
    
    public int PowerChange { get => powerChange; set => powerChange = value; }
    public int MineralChange { get => mineralChange; set => mineralChange = value; }
    public int OrganicChange { get => organicChange; set => organicChange = value; }
    public int FuelChange { get => fuelChange; set => fuelChange = value; }
    
    public List<Harvester> Harvesters { get => harvesters; set => harvesters = value; }
    public List<Battery> Batteries { get => batteries; set => batteries = value; }

    public Dictionary<BuildingType, Dictionary<string, int>> BuildingsCosts { get => buildingsCosts; }


    // private Dictionary<Element, int> harvest = new Dictionary<Element, int>();

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        powerSource = null;

        // costs 0 to build

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
        // Change the power based on each connect building
        // TODO: Make buildings stop working if no power
        int powerUpkeep = 0;
        foreach (Building building in suppliedBuildings)
        {
            powerUpkeep += building.Upkeep;
            if (powerUpkeep >= 0 || storedPower > 0)
            {
                building.PowerUp();
            }
            else
            {
                building.PowerDown();
            }

            if (building.BuildingType == BuildingType.Relay)
            {
                PowerSource relay = (PowerSource)building;
                if (relay != null)
                {
                    foreach (Building b in relay.SuppliedBuildings)
                    {
                        powerUpkeep += b.Upkeep;
                        if (powerUpkeep >= 0 || storedPower > 0)
                        {
                            b.PowerUp();
                        }
                        else
                        {
                            b.PowerDown();
                        }
                    }
                }
            }

        }

        // Process batteries
        maxPower = (batteries.Count * 10) + 100;
        int mChange = 0;
        int fChange = 0;
        int oChange = 0;
        int pChange = 0;

        if (harvesters.Count != 0)
        {
            foreach (Harvester harvester in harvesters)
            {
                ResourceOn = harvester.Location.Resource.ResourceType;
                

                switch (ResourceOn)
                {
                    case Resource.Power:                   
                        pChange += Mathf.RoundToInt(harvester.HarvestAmt * harvester.Location.Resource.ResMultiplier);
                        break;
                    case Resource.Organic:
                        oChange += Mathf.RoundToInt(harvester.HarvestAmt * harvester.Location.Resource.ResMultiplier);
                        break;
                    case Resource.Mineral:
                        mChange += Mathf.RoundToInt(harvester.HarvestAmt * harvester.Location.Resource.ResMultiplier);
                        break;
                    case Resource.Fuel:
                        fChange += Mathf.RoundToInt(harvester.HarvestAmt * harvester.Location.Resource.ResMultiplier);
                        break;
                }
            }
        }
        fuelChange = fChange;
        organicChange = oChange;
        mineralChange = mChange;
        powerChange = pChange + powerUpkeep;

        storedFuel += fuelChange;
        storedOrganic += organicChange;
        StoredMineral += mineralChange;
        StoredPower += powerChange;

        //storedPower += 5;
        CheckLimit();
    }

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
