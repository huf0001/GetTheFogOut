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

    private Dictionary<string, int> batteryCosts = new Dictionary<string, int>();
    private Dictionary<string, int> generatorCosts = new Dictionary<string, int>();
    private Dictionary<string, int> harvesterCosts = new Dictionary<string, int>();
    private Dictionary<string, int> relayCosts = new Dictionary<string, int>();
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
        batteryCosts.Add("power", 30);
        batteryCosts.Add("mineral", 0);
        batteryCosts.Add("organic", 0);
        batteryCosts.Add("fuel", 0);

        generatorCosts.Add("power", 30);
        generatorCosts.Add("mineral", 0);
        generatorCosts.Add("organic", 0);
        generatorCosts.Add("fuel", 0);

        harvesterCosts.Add("power", 50);
        harvesterCosts.Add("mineral", 0);
        harvesterCosts.Add("organic", 0);
        harvesterCosts.Add("fuel", 0);

        relayCosts.Add("power", 10);
        relayCosts.Add("mineral", 0);
        relayCosts.Add("organic", 0);
        relayCosts.Add("fuel", 0);

        buildingsCosts.Add(BuildingType.Battery, batteryCosts);
        buildingsCosts.Add(BuildingType.Generator, generatorCosts);
        buildingsCosts.Add(BuildingType.Harvester, harvesterCosts);
        buildingsCosts.Add(BuildingType.Relay, relayCosts);

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
