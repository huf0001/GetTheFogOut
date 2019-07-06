using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FogExpansionDirection
{
    Orthogonal,
    OrthogonalAndDiagonal
}

public enum StartConfiguration
{
    OneSide,
    Corners,
    FourCompassPoints,
    EightCompassPoints,
    FourSides,
    SurroundingHub,
    FullBoard
}

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class Fog : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Prefabs")]
    [SerializeField] private FogSphere fogSpherePrefab;
    [SerializeField] private FogUnit fogUnitPrefab;

    [Header("Materials")]
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material invisibleMaterial;

    [Header("Settings")]
    [SerializeField] private StartConfiguration configuration;
    [SerializeField] private Difficulty difficulty;
    [SerializeField] private FogExpansionDirection expansionDirection;

    [Header("Fog Expansion")]
    [SerializeField] private bool fogAccelerates;
    [SerializeField] private bool fogUnitsGrow;
    [SerializeField] private bool fogSpheresGrow;
    [SerializeField] private float minSphereDistanceFromHub;
    [SerializeField] private float minFogSphereRange;
    [SerializeField] private float maxFogSphereRange;
    [SerializeField] private float fogGrowth;
    [SerializeField] private float fogSpillThreshold;
    [SerializeField] private float minHealth;
    [SerializeField] private float maxHealth;

    [Header("Update Intervals")]
    [SerializeField] private float fogFillInterval;
    [SerializeField] private float fogDamageInterval;
    [SerializeField] private float fogExpansionInterval;
    [SerializeField] private float fogSphereInterval;

    [Header("Other")]
    [SerializeField] private bool angry;
    [SerializeField] private bool damageOn;
    [SerializeField] private float surroundingHubRange;

    //Private Value Fields
    private int xMax;
    private int zMax;
    private float fogDamage;
    private Vector3 hubPosition;

    //Private Collection Fields
    private List<TileData>  fogCoveredTiles = new List<TileData>();                 //i.e. tiles currently covered by fog
    private List<TileData>  fogFreeTiles = new List<TileData>();                    //i.e. tiles not currently covered by fog
    private List<TileData>  noSpheresSpawning;

    private List<FogUnit>   fogUnitsInPlay = new List<FogUnit>();                   //i.e. currently active fog units on the board
    private List<FogUnit>   fogUnitsToReturnToPool = new List<FogUnit>();           //i.e. currently waiting to be re-pooled
    private List<FogUnit>   fogUnitsInPool = new List<FogUnit>();                   //i.e. currently inactive fog units waiting for spawning

    private List<FogSphere> fogSpheresInPlay = new List<FogSphere>();               //i.e. currently active fog spheres on the board
    private List<FogSphere> fogSpheresToReturnToPool = new List<FogSphere>();       //i.e. currently waiting to be re-pooled
    private List<FogSphere> fogSpheresInPool = new List<FogSphere>();               //i.e. currently inactive fog spheres waiting for spawning

    //Public Properties
    public static Fog            Instance { get; protected set; }
    public bool                  DamageOn { get => damageOn; set => damageOn = value; }
    //public bool                  FogAccelerates { get => fogAccelerates; set => fogAccelerates = value; }
    public FogExpansionDirection ExpansionDirection { get => expansionDirection; }
    public List<TileData>        FogFreeTiles { get => fogFreeTiles; }
    public float                 FogGrowth { get => fogGrowth; set => fogGrowth = value; }
    public Difficulty            Difficulty { get => difficulty; set => difficulty = value; }
    //public float                 MaxHealth { get => maxHealth; set => maxHealth = value; }
    //public float                 MinHealth { get => minHealth; set => minHealth = value; }

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    //Fog's awake method sets the static instance of Fog
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should not be more than one Fog");
        }

        Instance = this;
    }

    // Sets the difficulty of the fog on start
    void Start()
    {
        SetDifficulty();
    }

    //Sets the difficulty according to what the player selects
    public void SetDifficulty()
    {
        fogDamage = fogUnitPrefab.Damage;

        switch (difficulty)
        {
            case Difficulty.Easy:
                fogDamage /= 1.40f;
                break;
            case Difficulty.Normal:
                break;
            case Difficulty.Hard:
                fogDamage *= 2f;
                break;
        }
    }

    //Spawns the starting fog on the board with the configuration set in the inspector
    public void SpawnStartingFog()
    {
        SpawnStartingFog(configuration);
    }

    //Spawns the starting fog on the board with the configuration passed to it
    private void SpawnStartingFog(StartConfiguration startConfiguration)
    {
        //Get dimensions of tile array
        xMax = WorldController.Instance.Width;
        zMax = WorldController.Instance.Length;
        hubPosition = GameObject.Find("Hub").transform.position;
        noSpheresSpawning = WorldController.Instance.GetTileAt(hubPosition).CollectTilesInRange((int)hubPosition.x, (int)hubPosition.z, (int)minSphereDistanceFromHub);

        foreach (TileData t in WorldController.Instance.Tiles)
        {
            if (t != null)
            {
                fogFreeTiles.Add(t);
            }
        }

        //Populate fog unit pool with fog units
        if (fogUnitsInPool.Count == 0)
        {
            for (int i = 0; i < xMax * zMax; i++)
            {
                fogUnitsInPool.Add(CreateFogUnit());
            }
        }

        //Populate fog sphere pool with fog spheres
        if (fogSpheresInPool.Count == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                fogSpheresInPool.Add(CreateFogSphere());
            }
        }

        //Arrange fog units on the board according to the passed configuration
        switch (startConfiguration)
        {
            case StartConfiguration.OneSide:
                //Spawns fog on one side of the board.
                for (int i = 0; i < xMax; i++)
                {
                    SpawnFogUnit(i, zMax - 1, maxHealth);
                }

                break;
            case StartConfiguration.Corners:
                //Corner spaces
                SpawnFogUnit(0, 0, maxHealth);
                SpawnFogUnit(0, zMax - 1, maxHealth);
                SpawnFogUnit(xMax - 1, 0, maxHealth);
                SpawnFogUnit(xMax - 1, zMax - 1, maxHealth);
                break;

            case StartConfiguration.FourCompassPoints:
                //Four compass points
                SpawnFogUnit(Mathf.RoundToInt(xMax / 2), 0, maxHealth);
                SpawnFogUnit(Mathf.RoundToInt(xMax / 2), zMax - 1, maxHealth);
                SpawnFogUnit(0, Mathf.RoundToInt(zMax / 2), maxHealth);
                SpawnFogUnit(xMax - 1, Mathf.RoundToInt(zMax / 2), maxHealth);
                break;
            case StartConfiguration.EightCompassPoints:
                //Corner spaces
                SpawnFogUnit(0, 0, maxHealth);
                SpawnFogUnit(0, zMax - 1, maxHealth);
                SpawnFogUnit(xMax - 1, 0, maxHealth);
                SpawnFogUnit(xMax - 1, zMax - 1, maxHealth);

                //Four compass points
                SpawnFogUnit(Mathf.RoundToInt(xMax / 2), 0, maxHealth);
                SpawnFogUnit(Mathf.RoundToInt(xMax / 2), zMax - 1, maxHealth);
                SpawnFogUnit(0, Mathf.RoundToInt(zMax / 2), maxHealth);
                SpawnFogUnit(xMax - 1, Mathf.RoundToInt(zMax / 2), maxHealth);

                break;
            case StartConfiguration.FourSides:
                //Each side
                for (int i = 1; i < xMax - 1; i++)
                {
                    SpawnFogUnit(i, 0, maxHealth);
                    SpawnFogUnit(i, zMax - 1, maxHealth);
                }

                for (int i = 1; i < zMax - 1; i++)
                {
                    SpawnFogUnit(0, i, maxHealth);
                    SpawnFogUnit(xMax - 1, i, maxHealth);
                }

                //Corner spaces
                SpawnFogUnit(0, 0, maxHealth);
                SpawnFogUnit(0, zMax - 1, maxHealth);
                SpawnFogUnit(xMax - 1, 0, maxHealth);
                SpawnFogUnit(xMax - 1, zMax - 1, maxHealth);

                break;
            case StartConfiguration.SurroundingHub:
                //Every space on the board
                for (int i = 0; i < xMax; i++)
                {
                    for (int j = 0; j < zMax; j++)
                    {
                        //Except those within a specified range of the hub
                        if (Vector3.Distance(hubPosition, new Vector3(i, hubPosition.y, j)) > surroundingHubRange)
                        {
                            SpawnFogUnit(i, j, maxHealth);
                        }
                    }
                }

                break;
            case StartConfiguration.FullBoard:
                //Every space on the board
                for (int i = 0; i < xMax; i++)
                {
                    for (int j = 0; j < zMax; j++)
                    {
                        SpawnFogUnit(i, j, maxHealth);
                    }
                }
                break;
        }
    }

    //Instantiates a fog unit that isn't on the board or in the pool
    private FogUnit CreateFogUnit()
    {
        FogUnit f = Instantiate<FogUnit>(fogUnitPrefab, transform, true);
        f.transform.position = transform.position;
        f.MaxHealth = maxHealth;
        f.Damage = fogDamage;
        f.Fog = this;
        return f;
    }

    //Retrieves a fog unit from the pool, or asks for a new one if the pool is empty
    private FogUnit GetFogUnit()
    {
        FogUnit f;

        if (fogUnitsInPool.Count > 0)
        {
            f = fogUnitsInPool[0];
            fogUnitsInPool.Remove(f);
            f.gameObject.SetActive(true);
        }
        else
        {
            f = CreateFogUnit();
        }

        return f;
    }

    //Take a fog unit and puts it on the board with minimum health
    public void SpawnFogUnitWithMinHealth(TileData t)
    {
        SpawnFogUnit(t.X, t.Z, minHealth);
    }

    ////Take a fog unit and puts it on the board with minimum health
    //private void SpawnFogUnitWithMinHealth(int x, int z)
    //{
    //    SpawnFogUnit(x, z, minHealth);
    //}

    ////Take a fog unit and puts it on the board with maximum health
    //private void SpawnFogUnitWithMaxHealth(int x, int z)
    //{
    //    SpawnFogUnit(x, z, maxHealth);
    //}

    //Takes a fog unit and puts it on the board
    private void SpawnFogUnit(int x, int z, float health)
    {
        GameObject fGO = GetFogUnit().gameObject;
        FogUnit f = fGO.GetComponent<FogUnit>();
        Vector2 pos = new Vector2(x, z);
        TileData t = WorldController.Instance.GetTileAt(pos);
        Transform ft = fGO.transform;

        ft.position = new Vector3(x, 0.13f, z);
        ft.SetPositionAndRotation(new Vector3(x, 0.13f, z), Quaternion.Euler(ft.rotation.eulerAngles.x, Random.Range(0, 360), ft.rotation.eulerAngles.z));
        fGO.name = "FogUnit(" + x + "," + z + ")";

        f.FogRenderer.material = visibleMaterial;
        f.Location = t;
        f.Health = health;
        f.Spill = false;
        f.SetStartEmotion(angry);
        t.FogUnit = f;

        fogUnitsInPlay.Add(f);
        fogCoveredTiles.Add(t);
        fogFreeTiles.Remove(t);

        f.RenderOpacity();
    }

    //Instantiates a fog sphere that isn't on the board or in the pool
    private FogSphere CreateFogSphere()
    {
        FogSphere f = Instantiate<FogSphere>(fogSpherePrefab, transform, true);
        f.transform.position = transform.position;
        f.MaxHealth = maxHealth;
        f.Fog = this;
        return f;
    }

    //Retrieves a fog sphere from the pool, or asks for a new one if the pool is empty
    private FogSphere GetFogSphere()
    {
        FogSphere f;

        if (fogSpheresInPool.Count > 0)
        {
            f = fogSpheresInPool[0];
            fogSpheresInPool.Remove(f);
            f.gameObject.SetActive(true);
        }
        else
        {
            f = CreateFogSphere();
        }

        return f;
    }

    //Takes a fog unit and puts it on the board
    private void SpawnFogSphere()
    {
        GameObject fGO = GetFogSphere().gameObject;
        FogSphere f = fGO.GetComponent<FogSphere>();
        //fGO.transform.SetPositionAndRotation(pos, rot);
        fGO.name = "FogSphereInPlay";
        fGO.transform.position = GetFogSpherePosition(f);
        f.FogRenderer.material = visibleMaterial;
        f.Health = minHealth;
        f.FogUnitMinHealth = minHealth;
        f.FogUnitMaxHealth = maxHealth;
        f.State = FogSphereState.Filling;
        f.SetStartEmotion(angry);
        fogSpheresInPlay.Add(f);
        f.UpdateHeight();
        f.RenderOpacity();
    }

    //Calculates the position for the fog sphere
    private Vector3 GetFogSpherePosition(FogSphere s)
    {
        return SphereRange(s);
        //return IterateIntoFog(s);
    }

    private Vector3 SphereRange(FogSphere s)
    {
        FogUnit f;
        bool finished = false;
        WorldController wc = WorldController.Instance;

        do
        {
            f = fogUnitsInPlay[Random.Range(0, fogUnitsInPlay.Count - 1)];
            TileData t = f.Location;

            if (!noSpheresSpawning.Contains(t))
            {
                if (fogFreeTiles.Count == 0)
                {
                    return f.transform.position;
                }

                bool valid = true;

                foreach (TileData a in t.AdjacentTiles)
                {
                    if (t.FogUnit == null)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    int i = (int)Mathf.Max(t.X - maxFogSphereRange, 0);
                    int j = (int)Mathf.Max(t.Z - maxFogSphereRange, 0);
                    float iMax = Mathf.Min(xMax, t.X + maxFogSphereRange);
                    float jMax = Mathf.Min(xMax, t.Z + maxFogSphereRange);

                    int iMinLeft = (int)Mathf.Max(t.X - minFogSphereRange, 0);
                    int jMinLeft = (int)Mathf.Max(t.Z - minFogSphereRange, 0);
                    float iMinRight = Mathf.Min(xMax, t.X + minFogSphereRange);
                    float jMinRight = Mathf.Min(xMax, t.Z + minFogSphereRange);
                    
                    //Foreach tile that exists in a unitTileProximityRange * unitTileProximityRange grid
                    while (i < iMax && !finished)
                    {
                        while (j < jMax && !finished)
                        {
                            //If it's not too close to the prospective tile for the fog sphere
                            if (wc.TileExistsAt(i, j) && (i < iMinLeft || i > iMinRight || j < jMinLeft || j > jMinRight))
                            {
                                TileData n = wc.GetTileAt(i, j);

                                //If it and all its neighbours and neighbour's neighbours have no fog, then there will probably be a valid target for the FogSphere to pursue.
                                if (n.FogUnit == null)
                                {
                                    //finished = true;
                                    valid = true;

                                    foreach (TileData a in n.AdjacentTiles)
                                    {
                                        if (a.FogUnit != null)
                                        {
                                            foreach (TileData b in a.AdjacentTiles)
                                            {
                                                if (b.FogUnit != null)
                                                {
                                                    valid = false;
                                                    break;
                                                }
                                            }
                                        }

                                        if (!valid)
                                        {
                                            break;
                                        }
                                    }

                                    if (valid)
                                    {
                                        finished = true;
                                        s.TilesInRange = t.CollectTilesInRange(t.X, t.Z, (int)maxFogSphereRange);
                                    }
                                }
                            }

                            j++;
                        }

                        i++;
                    }
                }
            }
        } while (!finished);

        return f.transform.position;
    }

    //private Vector3 IterateIntoFog()
    //{
    //    //Iterates from a fog unit into the fog a bit
    //    bool finished = false;
    //    FogUnit f;
    //    Vector3 posFirst;
    //    Vector3 posCurrent;
    //    List<TileData> adjacentTiles;

    //    int highestCount = 0;
    //    int newCount;
    //    int distanceCount = 0;

    //    //Find a fog unit where not all adjacent tiles have fog units, and it is far enough away from the hub.
    //    do
    //    {
    //        f = fogUnitsInPlay[Random.Range(0, fogUnitsInPlay.Count - 1)];
            
    //        if (noSpheresSpawning.Contains(f.Location))
    //        {
    //            continue;
    //        }

    //        //Debug.Log($"Selected {f.name}");

    //        foreach (TileData t in f.Location.AdjacentTiles)
    //        {
    //            if (t.FogUnit == null)
    //            {
    //                //Debug.Log($"{t.Name} does not have a fog unit. Breaking foreach loop.");
    //                finished = true;
    //                break;
    //            }
    //            //else
    //            //{
    //            //    Debug.Log($"{t.Name} has a fog unit.");
    //            //}
    //        }
    //    } while (!finished);

    //    //Get selected unit's position
    //    posCurrent = f.transform.position;
    //    posFirst = posCurrent;

    //    //Get it's number of adjacent fog units
    //    adjacentTiles = f.Location.AdjacentTiles;

    //    foreach (TileData t in adjacentTiles)
    //    {
    //        if (t.FogUnit != null)
    //        {
    //            highestCount++;
    //        }
    //    }

    //    //If possible, select a fog unit with more neighbouring tiles with fog units on them
    //    do
    //    {
    //        //Debug.Log($"Iterating closer from {f.name}");
    //        finished = true;

    //        //For each adjacent tile
    //        foreach (TileData t in adjacentTiles)
    //        {
    //            //Check how many of it's adjacent tiles have fog units
    //            newCount = 0;

    //            foreach (TileData ta in t.AdjacentTiles)
    //            {
    //                if (ta.FogUnit != null)
    //                {
    //                    newCount++;
    //                }
    //            }

    //            //If better than the current fog unit, keep it
    //            if (newCount > highestCount || (newCount == highestCount && distanceCount < 3 && Vector3.Distance(posFirst, t.FogUnit.transform.position) > Vector3.Distance(posCurrent, posFirst)))
    //            {
    //                if (newCount == highestCount)
    //                {
    //                    distanceCount++;
    //                }

    //                f = t.FogUnit;
    //                posCurrent = f.transform.position;
    //                adjacentTiles = t.AdjacentTiles;
    //                highestCount = newCount;
    //                finished = false;
    //            }
    //        }
    //    } while (!finished);

    //    return f.transform.position; ;
    //}

    //Invokes the ActivateFog method according to the parameter passed to it.
    public void InvokeActivateFog(int delay)
    {
        Invoke(nameof(ActivateFog), delay);
    }

    //Invokes the "update" methods of Fog according to the intervals set in the inspector
    public void ActivateFog()
    {
        InvokeRepeating(nameof(UpdateFogUnitFill), 0.1f, fogFillInterval);
        InvokeRepeating(nameof(CheckExpandFog), 0.3f, fogExpansionInterval);
        InvokeRepeating(nameof(UpdateDamageToFogUnits), 0.5f, fogDamageInterval);
        InvokeRepeating(nameof(UpdateFogSpheres), 1f, fogSphereInterval);
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

    //Handles the damaging of fog units
    private void UpdateDamageToFogUnits()
    {
        List<FogUnit> toRender = new List<FogUnit>();

        foreach (FogUnit f in fogUnitsInPlay)
        {
            if (f.TakingDamage)
            {
                f.UpdateDamageToFogUnit(fogDamageInterval);
                toRender.Add(f);
            }
        }

        if (fogUnitsToReturnToPool.Count > 0)
        {
            foreach (FogUnit f in fogUnitsToReturnToPool)
            {
                toRender.Remove(f);
                ReturnFogUnitToPool(f);
            }

            fogUnitsToReturnToPool = new List<FogUnit>();
        }

        foreach (FogUnit f in toRender)
        {
            f.RenderOpacity();
        }

        if (fogUnitsInPlay.Count == 0)
        {
            ObjectiveController.Instance.FogDestroyed();
            CancelInvoke();
        }
    }

    //Fills the health of fog units
    private void UpdateFogUnitFill()
    {
        List<FogUnit> toRenderOpacity = new List<FogUnit>();

        if (fogAccelerates && fogGrowth < 100)
        {
            fogGrowth += fogFillInterval;
        }

        if (fogUnitsGrow)
        {
            foreach (FogUnit f in fogUnitsInPlay)
            {
                f.RenderColour();

                if (f.NeighboursFull || f.Health < fogSpillThreshold)
                {
                    continue;
                }

                int count = f.Location.AdjacentTiles.Count;
                int fullCount = 0;

                foreach (TileData t in f.Location.AdjacentTiles)
                {
                    FogUnit af = t.FogUnit;

                    if (af == null)
                    {
                        continue;
                    }
                    else if (af.Health < f.Health)
                    {
                        af.Health += fogFillInterval * fogGrowth / count;

                        if (!toRenderOpacity.Contains(af))
                        {
                            toRenderOpacity.Add(af);
                        }
                    }
                    else if (af.Health >= maxHealth)
                    {
                        fullCount++;
                    }
                }

                if (count == fullCount)
                {
                    f.NeighboursFull = true;
                }
            }

            foreach (FogUnit f in toRenderOpacity)
            {
                f.RenderOpacity();
            }
        }
    }

    //Checks if the fog is able to spill over into adjacent tiles
    private void CheckExpandFog()
    {
        if (fogUnitsInPool.Count > 0 && configuration != StartConfiguration.FullBoard)
        {
            ExpandFog();
        }
        else if (fogUnitsInPlay.Count < xMax * zMax)
        {
            Debug.Log("Ran out of fog units. If the board isn't full, there must be some overlapping.");
        }
        else if (fogUnitsInPlay.Count > xMax * zMax)
        {
            Debug.Log("More fog units than board tiles. There must be some overlapping.");
        }
    }

    //Fog spills over onto adjacent tiles
    private void ExpandFog()
    {
        List<TileData> newTiles = new List<TileData>();

        foreach (FogUnit f in fogUnitsInPlay)
        {
            if (f.Spill == false && f.Health >= fogSpillThreshold)
            {
                f.Spill = true;

                foreach (TileData a in f.Location.AdjacentTiles)
                {
                    if ((!fogCoveredTiles.Contains(a)) && (!newTiles.Contains(a)))
                    {
                        newTiles.Add(a);
                    }
                }
            }
        }

        if (newTiles.Count > 0)
        {
            foreach (TileData n in newTiles)
            {
                SpawnFogUnit(n.X, n.Z, minHealth);        //SpawnFogUnit adds the tile spawned on to the list fogTiles
            }
        }
    }

    //Handles the fog spheres
    private void UpdateFogSpheres()
    {
        if (fogSpheresGrow)
        {
            foreach (FogSphere f in fogSpheresInPlay)
            {
                f.RenderColour();

                switch (f.State)
                {
                    case FogSphereState.Damaged:
                        f.UpdateDamageToFogSphere(fogSphereInterval);
                        f.UpdateHeight();
                        f.RenderOpacity();
                        break;
                    case FogSphereState.Filling:
                        if (f.Health < f.MaxHealth)
                        {
                            f.Health += fogSphereInterval * fogGrowth * 0.5f;
                            f.UpdateHeight();
                            f.RenderOpacity();
                        }

                        break;
                    case FogSphereState.Full:
                        f.Throw();
                        break;
                    case FogSphereState.Throwing:
                        f.Move(fogSphereInterval * 0.25f);
                        break;
                    case FogSphereState.Attacking:
                        f.Attack(fogSphereInterval * 0.75f);
                        break;
                    case FogSphereState.Spilling:
                        f.Spill(fogSphereInterval * 0.5f);
                        break;
                }
            }

            if (fogSpheresToReturnToPool.Count > 0)
            {
                foreach(FogSphere f in fogSpheresToReturnToPool)
                {
                    ReturnFogSphereToPool(f);
                }

                fogSpheresToReturnToPool = new List<FogSphere>();
            }
        }

        if (fogSpheresInPlay.Count == 0)
        {
            SpawnFogSphere();
        }

        
    }

    //Re-Pooling Methods-----------------------------------------------------------------------------------------------------------------------------

    //Puts the fog unit in the list of fog units to be put back in the pool
    public void QueueFogUnitForPooling(FogUnit f)
    {
        //Debug.Log("Returning sphere to fog pool");
        fogUnitsToReturnToPool.Add(f);
    }

    //Takes the fog unit off the board and puts it back in the pool
    private void ReturnFogUnitToPool(FogUnit f)
    {
        f.gameObject.name = "FogUnitInPool";

        if (f.Location != null)
        {
            foreach (TileData t in f.Location.AdjacentTiles)
            {
                if (t.FogUnit != null)
                {
                    t.FogUnit.Spill = false;
                }
            }
        }

        foreach (FogSphere s in fogSpheresInPlay)
        {
            if (s.SpiltFog.Contains(f))
            {
                s.SpiltFog.Remove(f);
            }
        }

        f.FillingFromFogSphere = false;
        f.gameObject.SetActive(false);
        f.Location.FogUnit = null;
        f.FogRenderer.material = invisibleMaterial;
        f.Spill = true;
        f.transform.position = transform.position;

        fogUnitsInPool.Add(f);
        fogUnitsInPlay.Remove(f);
        fogCoveredTiles.Remove(f.Location);
        fogFreeTiles.Add(f.Location);
    }

    //Puts the fog sphere in the list of fog spheres to be put back in the pool
    public void QueueFogSphereForPooling(FogSphere f)
    {
        //Debug.Log("Returning sphere to fog pool");
        fogSpheresToReturnToPool.Add(f);
    }

    //Takes the fog unit off the board and puts it back in the pool
    private void ReturnFogSphereToPool(FogSphere f)
    {
        f.gameObject.name = "FogSphereInPool";
        f.gameObject.SetActive(false);
        f.FogRenderer.material = invisibleMaterial;
        f.transform.position = transform.position;

        foreach (FogUnit u in f.SpiltFog)
        {
            u.FillingFromFogSphere = false;
        }

        f.SpiltFog = new List<FogUnit>();

        fogSpheresInPool.Add(f);
        fogSpheresInPlay.Remove(f);
    }

    //Other Methods----------------------------------------------------------------------------------------------------------------------------------

    //Switches the fog between angry and not angry
    public void ToggleAnger()
    {
        angry = !angry;

        foreach (FogUnit f in fogUnitsInPlay)
        {
            f.Angry = angry;
        }

        foreach (FogSphere f in fogSpheresInPlay)
        {
            f.Angry = angry;
        }
    }
}
