using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : PowerSource
{
    [SerializeField] private int maxPower = 100;
    [SerializeField] private int storedPower = 0;
    [SerializeField] private int powerChange = 0;

    [SerializeField] private int storedOrganic = 0;
    [SerializeField] private int storedMineral = 0;
    [SerializeField] private int mineralChange = 0;
    [SerializeField] private int storedFuel = 0;

    [Header("Building Costs")]
    [SerializeField] private int batteryPowerCost = 30;
    [SerializeField] private int batteryMineralCost = 0;
    [SerializeField] private int batteryOrganicCost = 0;
    [SerializeField] private int batteryFuelCost = 0;
    [SerializeField] private int generatorPowerCost = 30;
    [SerializeField] private int generatorMineralCost = 0;
    [SerializeField] private int generatorOrganicCost = 0;
    [SerializeField] private int generatorFuelCost = 0;
    [SerializeField] private int harvesterPowerCost = 50;
    [SerializeField] private int harvesterMineralCost = 0;
    [SerializeField] private int harvesterOrganicCost = 0;
    [SerializeField] private int harvesterFuelCost = 0;
    [SerializeField] private int relayPowerCost = 10;
    [SerializeField] private int relayMineralCost = 0;
    [SerializeField] private int relayOrganicCost = 0;
    [SerializeField] private int relayFuelCost = 0;
    [SerializeField] private int defencePowerCost = 0;
    [SerializeField] private int defenceMineralCost = 0;
    [SerializeField] private int defenceOrganicCost = 0;
    [SerializeField] private int defenceFuelCost = 0;


    private Dictionary<string, int> batteryCosts = new Dictionary<string, int>();
    private Dictionary<string, int> generatorCosts = new Dictionary<string, int>();
    private Dictionary<string, int> harvesterCosts = new Dictionary<string, int>();
    private Dictionary<string, int> relayCosts = new Dictionary<string, int>();
    private Dictionary<string, int> defenceCosts = new Dictionary<string, int>();
    private Dictionary<BuildingType, Dictionary<string, int>> buildingsCosts = 
        new Dictionary<BuildingType, Dictionary<string, int>>();

    [SerializeField] private List<Harvester> harvesters = new List<Harvester>();
    [SerializeField] private List<Battery> batteries = new List<Battery>();

    public int StoredOrganic { get => storedOrganic; set => storedOrganic = value; }
    public int StoredMineral { get => storedMineral; set => storedMineral = value; }
    public int StoredFuel { get => storedFuel; set => storedFuel = value; }
    public int MaxPower { get => maxPower; set => maxPower = value; }
    public int StoredPower { get => storedPower; set => storedPower = value; }
    public int PowerChange { get => powerChange; set => powerChange = value; }
    public int MineralChange { get => mineralChange; set => mineralChange = value; }
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
    void Update()
    {
        
    }

    private void ProcessUpkeep()
    {
        // Change the power based on each connect building
        // TODO: Make buildings stop working if no power
        int totalUpkeep = 5;
        foreach (Building building in suppliedBuildings)
        {
            totalUpkeep += building.Upkeep;
            if (totalUpkeep >= 0 || storedPower > 0)
            {
                building.PowerUp();
            }
            else
            {
                building.PowerDown();
            }
        }

        powerChange = totalUpkeep;
        ChangePower(totalUpkeep);

        // Process batteries
        maxPower = (batteries.Count * 10) + 100;

        // Process minerals
        totalUpkeep = 0;
        foreach (Harvester harvester in harvesters)
        {
            totalUpkeep += harvester.HarvestAmt;
        }
        mineralChange = totalUpkeep;
        storedMineral += mineralChange;
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

    public void ChangePower(int change)
    {
        storedPower += change;

        if (storedPower > maxPower)
        {
            storedPower = maxPower;
        }

        if (storedPower < 0)
        {
            storedPower = 0;
        }
    }
}
