using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceController : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private int maxPower;
    [SerializeField] private int maxMineral;
    [SerializeField] private float storedPower;
    [SerializeField] private float storedMineral;
    [SerializeField] private float powerChange;
    [SerializeField] private float mineralChange;

    [SerializeField] private List<ArcDefence> mortars = new List<ArcDefence>();
    [SerializeField] private List<RepelFan> pulseDefences = new List<RepelFan>();
    [SerializeField] private List<Generator> generators = new List<Generator>();
    [SerializeField] private List<Harvester> harvesters = new List<Harvester>();
    [SerializeField] private List<Relay> extenders = new List<Relay>();

    //Non-serialized fields
    private Hub hub = null;

    private List<Building> buildings = new List<Building>();
    private List<Harvester> activeHarvesters;

    public float powerChangePauseThreshold;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public static ResourceController Instance { get; protected set; }

    public int MaxMineral { get => maxMineral; set => maxMineral = value; }
    public float StoredPower { get => storedPower; set => storedPower = value; }
    public float StoredMineral { get => storedMineral; set => storedMineral = value; }
    public float PowerChange { get => powerChange; set => powerChange = value; }
    public float MineralChange { get => mineralChange; set => mineralChange = value; }

    public List<Building> Buildings { get => buildings; set => buildings = value; }
    public List<Relay> Extenders { get => extenders; }
    public List<Generator> Generators { get => generators; set => generators = value; }
    public List<Harvester> Harvesters { get => harvesters; set => harvesters = value; }
    public List<ArcDefence> Mortars { get => mortars; set => mortars = value; }
    public List<RepelFan> PulseDefences { get => pulseDefences; set => pulseDefences = value; }



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
        activeHarvesters = new List<Harvester>(harvesters);
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

        //Process power and power supplying
        CalculatePowerSupply();
        CalculateUpkeep();
        SupplyPower();        
        
        //Makes sure power and minerals don't exceed their maximum values
        CheckLimits();
    }

    //Calculates how much power is supplied by all buildings
    private void CalculatePowerSupply()
    {
        //Provide hub's power contribution
        powerChange += hub.Upkeep;
        
        //Get Generators' power contributions
        foreach (Generator generator in generators)
        {
            if (WorldController.Instance.ActiveTiles.Contains(generator.Location))
            {
                powerChange += generator.Upkeep * generator.OverclockValue;
            }
        }

        //Supply power to connected generators
        foreach (Generator g in generators)
        {
            if (WorldController.Instance.ActiveTiles.Contains(g.Location))
            {
                g.PowerUp();
            }
            else
            {
                g.PowerDown();
            }
        }

        //Supply power to connected extenders
        foreach (Relay r in extenders)
        {
            if (WorldController.Instance.ActiveTiles.Contains(r.Location))
            {
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

    //Calculates how much power is used by all buildings
    private void CalculateUpkeep()
    {
        //Gets the power drain by the mortars
        foreach (ArcDefence m in mortars)
        {
            if (WorldController.Instance.ActiveTiles.Contains(m.Location))
            {
                powerChange += m.Upkeep;
            }
        }

        //Gets the power drain by the pulse defences
        foreach (RepelFan pd in pulseDefences)
        {
            if (WorldController.Instance.ActiveTiles.Contains(pd.Location))
            {
                powerChange += pd.Upkeep;
            }
        }

        //Gets the power drain by the harvesters
        foreach (Harvester h in activeHarvesters)
        {
            if (WorldController.Instance.ActiveTiles.Contains(h.Location))
            {
                powerChange += h.Upkeep;
            }
        }

        if (powerChangePauseThreshold == -1)
        {
            storedPower += powerChange;
        }
        else if (powerChangePauseThreshold != -1)
        {
            if (storedPower >= powerChangePauseThreshold)
            {
                storedPower = Mathf.Max(storedPower + powerChange, powerChangePauseThreshold);
            }
            
            if (storedPower <= powerChangePauseThreshold)
            {
                powerChange = 0;
            }
        }
    }

    //Supplies powers to buildings based on the current stored power
    private void SupplyPower()
    {
        //Powers mortars

        if (storedPower > 75)
        {
            foreach (ArcDefence ac in mortars)
            {
                if (WorldController.Instance.ActiveTiles.Contains(ac.Location))
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

        //Powers pulse defences
        if (storedPower > 25)
        {
            foreach (RepelFan pd in pulseDefences)
            {
                if (WorldController.Instance.ActiveTiles.Contains(pd.Location))
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

        //Powers harvesters
        if (storedPower > 0)
        {
            foreach (Harvester h in harvesters)
            {
                if (WorldController.Instance.ActiveTiles.Contains(h.Location))
                {
                    if (h.Location.Resource.Health != 0)
                    {
                        h.PowerUp();
                    }

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
                                    Destroy(h.Location.Resource.gameObject);
                                    h.TurnOnMineralIndicator();
                                    h.ShutdownBuilding();
                                    activeHarvesters.Remove(h);
                                  //  RemoveBuilding(h);
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
    }

    //Checks that power and minerals are within their maximum values
    public void CheckLimits()
    {
        if (storedPower >= maxPower)
        {
            storedPower = maxPower;
            // if (!maxPowPlayed)
            // {
            //     audioMaxPower.Play();
            //     maxPowPlayed = true;
            // }
        }
        else
        {
            // maxPowPlayed = false;

            if (storedPower < 0)
            {
                storedPower = 0;
            }
        }

        if (storedMineral >= maxMineral)
        {
            storedMineral = maxMineral;
            // if (!maxMinPlayed)
            // {
            //     audioMaxMineral.Play();
            //     maxMinPlayed = true;
            // }
        }
        else
        {
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
                activeHarvesters.Add(b as Harvester);
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
                activeHarvesters.Remove(b as Harvester);
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

    //Pauses power drop once the power drops below a specified threshold
    public void PausePowerChange(float threshold)
    {
        powerChangePauseThreshold = threshold;
    }

    //Unpauses the pause to power drop
    public void UnPausePowerChange()
    {
        powerChangePauseThreshold = -1;
    }
}
