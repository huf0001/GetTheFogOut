using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

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
    Medium,
    Hard,
    VeryHard
}

[Serializable]
public struct FogDifficulty
{
    public float fogDamageMultiplier;
    public float earlyGameGrowthMultiplier;
    public float midGameGrowthMultiplier;
    public float lateGameGrowthMultiplier;
}

public class Fog : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Prefabs")]
    [SerializeField] private FogSphere fogSpherePrefab;
    [SerializeField] private FogUnit fogUnitPrefab;
    [SerializeField] private ParticleSystem fogEvaporation;
    [SerializeField] private bool usingFogEvaporation;

    [Header("Materials")]
    [SerializeField] private Material fogUnitVisibleMaterial;
    [SerializeField] private Material fogSphereVisibleMaterial;
    [SerializeField] private Material invisibleMaterial;

    [Header("General Settings")]
    [SerializeField] private bool angry;
    [SerializeField] private bool damageOn;

    [SerializeField] private StartConfiguration configuration;
    [SerializeField] private Difficulty difficulty;
    [SerializeField] private FogExpansionDirection expansionDirection;

    [SerializeField] private float surroundingHubRange;

    [Header("Fog Expansion")]
    [SerializeField] private bool fogAccelerates;
    [SerializeField] private bool fogFrozen;
    [SerializeField] private float fogGrowth;
    [SerializeField] private float fogSpillThreshold;

    [SerializeField] private GameObject fogSphereSpawnPointsParent;
    [SerializeField] private float maxFogSpheresCount;

    [Header("Fog Strength Over Time")]
    [SerializeField] private float earlyGameFogGrowth;
    [SerializeField] private float midGameFogGrowth;
    [SerializeField] private float lateGameFogGrowth;
    [SerializeField] private float earlyGameMaxFogSphereHealth;
    [SerializeField] private float midGameMaxFogSphereHealth;
    [SerializeField] private float lateGameMaxFogSphereHealth;

    [Header("Fog Strength Multipliers by Difficulty")]
    [SerializeField] private FogDifficulty easyMultipliers;
    [SerializeField] private FogDifficulty mediumMultipliers;
    [SerializeField] private FogDifficulty hardMultipliers;
    [SerializeField] private FogDifficulty veryHardMultipliers;

    [Header("Fog Sphere Size Over Time")]
    [SerializeField] private float earlyGameMaxFogSphereSize;
    [SerializeField] private float midGameMaxFogSphereSize;
    [SerializeField] private float lateGameMaxFogSphereSize;

    [Header("Health")]
    [SerializeField] private float fogSphereMinHealth;
    [SerializeField] private float fogUnitMinHealth;
    [SerializeField] private float fogUnitMaxHealth;

    [Header("Update Intervals")]
    [SerializeField] private float fogFillInterval;
    [SerializeField] private float fogDamageInterval;
    [SerializeField] private float fogExpansionInterval;
    [SerializeField] private float fogSphereInterval;

    //Private Value Fields
    private int xCount;
    private int zCount;
    private int xMax;
    private int zMax;

    private float fogDamage;
    private float fogSphereMaxHealth;
    private float fogSphereMaxSizeScale;
    private int intensity;

    private Vector3 hubPosition;
    private List<FogSphereWaypoint> fogSphereSpawnPoints;

    private float pauseTime;

    //Private Collection Fields
    private FogUnit[,]      fogUnits;                                           //i.e. all fog units being managed by the fog; effectively the pool of all fog units, pooled or in play

    private List<FogUnit>   fogUnitsInPlay = new List<FogUnit>();               //i.e. currently active fog units on the board
    //private List<FogUnit>   borderFogUnitsInPlay = new List<FogUnit>();       //i.e. currently active fog units around the edge of the board
    private List<FogUnit>   fogUnitsToReturnToPool = new List<FogUnit>();       //i.e. currently waiting to be re-pooled

    private List<FogSphere> fogSpheresInPlay = new List<FogSphere>();           //i.e. currently active fog spheres on the board
    private List<FogSphere> fogSpheresToReturnToPool = new List<FogSphere>();   //i.e. currently waiting to be re-pooled
    private List<FogSphere> fogSpheresInPool = new List<FogSphere>();           //i.e. currently inactive fog spheres waiting for spawning

    private List<FogLightning> lightningInPlay = new List<FogLightning>();       //i.e. currently active lightning effects
    private List<FogLightning> lightningInPool = new List<FogLightning>();       //i.e. currently inactive lightning effects in pool

    private List<ParticleSystem> evaporationInPlay = new List<ParticleSystem>(); //i.e. currently active fog evaporation effects
    private List<ParticleSystem> evaporationInPool = new List<ParticleSystem>(); //i.e. currently inactive fog evaporation effects

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public static Fog            Instance { get; protected set; }
    public bool                  DamageOn { get => damageOn; set => damageOn = value; }
    public FogExpansionDirection ExpansionDirection { get => expansionDirection; }
    public List<FogUnit>         FogUnitsInPlay { get => fogUnitsInPlay; }
    public List<FogLightning>    LightningInPlay { get => lightningInPlay; }
    public List<FogLightning>    LightningInPool { get => lightningInPool; }
    public int                   XMax { get => xMax; }
    public int                   ZMax { get => zMax; }

    //Altered Public Properties
    public int Intensity
    {
        get
        {
            return intensity;
        }

        set
        {
            if (intensity != value && value >= 0 && value <= 3)
            {
                intensity = value;

                switch (intensity)
                {
                    case 0:
                        fogGrowth = easyMultipliers.earlyGameGrowthMultiplier * 0.5f;
                        fogSphereMaxHealth = earlyGameMaxFogSphereHealth;
                        fogSphereMaxSizeScale = earlyGameMaxFogSphereSize;

                        if (angry)
                        {
                            ToggleAnger();
                        }

                        break;
                    case 1:
                        fogGrowth = earlyGameFogGrowth;
                        fogSphereMaxHealth = earlyGameMaxFogSphereHealth;
                        fogSphereMaxSizeScale = earlyGameMaxFogSphereSize;

                        if (angry)
                        {
                            ToggleAnger();
                        }

                        break;
                    case 2:
                        fogGrowth = midGameFogGrowth;
                        fogSphereMaxHealth = midGameMaxFogSphereHealth;
                        fogSphereMaxSizeScale = midGameMaxFogSphereSize;

                        if (angry)
                        {
                            ToggleAnger();
                        }

                        break;
                    case 3:
                        fogGrowth = lateGameFogGrowth;
                        fogSphereMaxHealth = lateGameMaxFogSphereHealth;
                        fogSphereMaxSizeScale = lateGameMaxFogSphereSize;

                        if (!angry)
                        {
                            ToggleAnger();
                        }

                        break;
                }
            }
            else if (value < 1)
            {
                Debug.Log("Fog.Instance.Intensity has to be >= 0 (and <= 3). Try again, you cabbage!");
            }
            else if (value > 3)
            {
                Debug.Log("Fog.Instance.Intensity has to be <= 3 (and >= 0). Try again, you cabbage!");
            }
        }
    }

    //Setup Methods - General------------------------------------------------------------------------------------------------------------------------

    //Fog's awake method sets the static instance of Fog
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should not be more than one Fog");
        }

        Instance = this;

        if (GlobalVars.LoadedFromMenu)
        {
            difficulty = (Difficulty)GlobalVars.Difficulty;
        }

        fogSphereSpawnPoints = new List<FogSphereWaypoint>(fogSphereSpawnPointsParent.GetComponentsInChildren<FogSphereWaypoint>());
        SetDifficulty();
        InitializePool(100);
        Intensity = 1;  //Sets the intensity-derived values to the default of their "Easy" values.
    }

    //Sets the difficulty according to what the player selects
    public void SetDifficulty()
    {
        fogDamage = fogUnitPrefab.Damage;

        switch (difficulty)
        {
            case Difficulty.Easy:
                fogDamage *= easyMultipliers.fogDamageMultiplier;
                earlyGameFogGrowth *= easyMultipliers.earlyGameGrowthMultiplier;
                midGameFogGrowth *= easyMultipliers.midGameGrowthMultiplier;
                lateGameFogGrowth *= easyMultipliers.lateGameGrowthMultiplier;
                break;
            case Difficulty.Medium:
                fogDamage *= mediumMultipliers.fogDamageMultiplier;
                earlyGameFogGrowth *= mediumMultipliers.earlyGameGrowthMultiplier;
                midGameFogGrowth *= mediumMultipliers.midGameGrowthMultiplier;
                lateGameFogGrowth *= mediumMultipliers.lateGameGrowthMultiplier;
                break;
            case Difficulty.Hard:
                fogDamage *= hardMultipliers.fogDamageMultiplier;
                earlyGameFogGrowth *= hardMultipliers.earlyGameGrowthMultiplier;
                midGameFogGrowth *= hardMultipliers.midGameGrowthMultiplier;
                lateGameFogGrowth *= hardMultipliers.lateGameGrowthMultiplier;
                break;
            case Difficulty.VeryHard:
                fogDamage *= veryHardMultipliers.fogDamageMultiplier;
                earlyGameFogGrowth *= veryHardMultipliers.earlyGameGrowthMultiplier;
                midGameFogGrowth *= veryHardMultipliers.midGameGrowthMultiplier;
                lateGameFogGrowth *= veryHardMultipliers.lateGameGrowthMultiplier;
                break;
        }
    }

    //Spawning Methods - All Fog---------------------------------------------------------------------------------------------------------------------

    //Spawns the starting fog on the board with the configuration set in the inspector
    public void SpawnStartingFog()
    {
        SpawnStartingFog(configuration);
    }

    //Spawns the starting fog on the board with the configuration passed to it
    private void SpawnStartingFog(StartConfiguration startConfiguration)
    {
        //Get dimensions of tile array
        xCount = WorldController.Instance.Width;
        zCount = WorldController.Instance.Length;
        xMax = xCount - 1;
        zMax = zCount - 1;
        hubPosition = GameObject.Find("Ship").transform.position;

        //Create fog unit array
        fogUnits = new FogUnit[xCount, zCount];

        //Populate fog unit array with fog units
        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < zCount; j++)
            {
                CreateFogUnit(i, j);    //creates the fog unit, matches it up with tile (i, j) and assigns it to fogUnits[i, j]. Assigns null if tile already has a fog unit or there is no tile
            }
        }

        //Arrange fog units on the board according to the passed configuration
        switch (startConfiguration)
        {
            case StartConfiguration.OneSide:
                //Spawns fog on one side of the board.
                for (int i = 0; i < xCount; i++)
                {
                    SpawnFogUnit(i, zMax, fogUnitMaxHealth);
                }

                break;
            case StartConfiguration.Corners:
                //Corner spaces
                SpawnFogUnit(0, 0, fogUnitMaxHealth);
                SpawnFogUnit(0, zMax, fogUnitMaxHealth);
                SpawnFogUnit(xMax, 0, fogUnitMaxHealth);
                SpawnFogUnit(xMax, zMax, fogUnitMaxHealth);
                break;

            case StartConfiguration.FourCompassPoints:
                //Four compass points
                SpawnFogUnit(Mathf.RoundToInt(xCount / 2), 0, fogUnitMaxHealth);
                SpawnFogUnit(Mathf.RoundToInt(xCount / 2), zMax, fogUnitMaxHealth);
                SpawnFogUnit(0, Mathf.RoundToInt(zCount / 2), fogUnitMaxHealth);
                SpawnFogUnit(xMax, Mathf.RoundToInt(zCount / 2), fogUnitMaxHealth);
                break;
            case StartConfiguration.EightCompassPoints:
                //Corner spaces
                SpawnFogUnit(0, 0, fogUnitMaxHealth);
                SpawnFogUnit(0, zMax, fogUnitMaxHealth);
                SpawnFogUnit(xMax, 0, fogUnitMaxHealth);
                SpawnFogUnit(xMax, zMax, fogUnitMaxHealth);

                //Four compass points
                SpawnFogUnit(Mathf.RoundToInt(xCount / 2), 0, fogUnitMaxHealth);
                SpawnFogUnit(Mathf.RoundToInt(xCount / 2), zMax, fogUnitMaxHealth);
                SpawnFogUnit(0, Mathf.RoundToInt(zCount / 2), fogUnitMaxHealth);
                SpawnFogUnit(xMax, Mathf.RoundToInt(zCount / 2), fogUnitMaxHealth);

                break;
            case StartConfiguration.FourSides:
                //Each side
                for (int i = 1; i < xMax; i++)
                {
                    SpawnFogUnit(i, 0, fogUnitMaxHealth);
                    SpawnFogUnit(i, zMax, fogUnitMaxHealth);
                }

                for (int i = 1; i < zMax; i++)
                {
                    SpawnFogUnit(0, i, fogUnitMaxHealth);
                    SpawnFogUnit(xMax, i, fogUnitMaxHealth);
                }

                //Corner spaces
                SpawnFogUnit(0, 0, fogUnitMaxHealth);
                SpawnFogUnit(0, zMax, fogUnitMaxHealth);
                SpawnFogUnit(xMax, 0, fogUnitMaxHealth);
                SpawnFogUnit(xMax, zMax, fogUnitMaxHealth);

                break;
            case StartConfiguration.SurroundingHub:
                //Every space on the board
                for (int i = 0; i < xCount; i++)
                {
                    for (int j = 0; j < zCount; j++)
                    {
                        //Except those within a specified range of the hub
                        if (Vector3.Distance(hubPosition, new Vector3(i, hubPosition.y, j)) > surroundingHubRange)
                        {
                            SpawnFogUnit(i, j, fogUnitMaxHealth);
                        }
                    }
                }

                break;
            case StartConfiguration.FullBoard:
                //Every space on the board
                for (int i = 0; i < xCount; i++)
                {
                    for (int j = 0; j < zCount; j++)
                    {
                        SpawnFogUnit(i, j, fogUnitMaxHealth);
                    }
                }
                break;
        }

        //Populate fog evaporation pool with evaporation particle effects
        while (evaporationInPool.Count < 30)
        {
            evaporationInPool.Add(CreateFogEvaporation());
        }

        //Populate fog sphere pool with fog spheres
        while (fogSpheresInPool.Count < maxFogSpheresCount * 2)
        {
            fogSpheresInPool.Add(CreateFogSphere());
        }
    }

    //Spawning Methods - Fog Units-------------------------------------------------------------------------------------------------------------------
    
    //Instantiates a fog unit that isn't on the board or in the pool
    private void CreateFogUnit(int x, int z)
    {
        FogUnit f = null;

        if (WorldController.Instance.TileExistsAt(x, z))
        {
            TileData t = WorldController.Instance.GetTileAt(x, z);

            if (t.FogUnit != null)
            {
                Debug.LogError($"FogUnit({x}, {z}) cannot be assigned to TileData({x}, {z}), as {t.FogUnit.name} is already assigned to TileData({x},{z})");
            }
            else
            {
                f = Instantiate<FogUnit>(fogUnitPrefab, transform, true);
                f.Location = t;
                t.FogUnit = f;

                f.name = $"FogUnit ({x}, {z}) (In Pool)";
                f.transform.position = transform.position;
                f.MaxHealth = fogUnitMaxHealth;
                f.Damage = fogDamage;
                f.Fog = this;
            }
        }
        else
        {
            Debug.Log($"No tile exists at ({x},{z}). Cannot create a fog unit there.");
        }

        fogUnits[x, z] = f;
    }

    //Take a fog unit by tile and puts it on the board with minimum health
    public void SpawnFogUnitWithMinHealthOnTile(TileData t)
    {
        SpawnFogUnit(fogUnits[t.X, t.Z], t, fogUnitMinHealth);
    }

    //Takes a fog unit by position, and puts it on the board with the specified health
    private void SpawnFogUnit(int x, int z, float health)
    {
        FogUnit f = fogUnits[x, z];
        TileData t = f.Location;

        SpawnFogUnit(f, t, health);
    }

    //Takes a fog unit by tile and puts it on the board
    private void SpawnFogUnit(FogUnit f, TileData t, float health)
    {
        if (!f.ActiveOnTile && !t.buildingChecks.obstacle)    
        {
            Transform ft = f.gameObject.transform;

            t.FogUnitActive = true;
            f.ActiveOnTile = true;

            ft.SetPositionAndRotation(new Vector3(t.X, 0.13f, t.Z),
                Quaternion.Euler(ft.rotation.eulerAngles.x, Random.Range(0, 360), ft.rotation.eulerAngles.z));

            f.name = $"FogUnit({t.X}, {t.Z})";
            f.gameObject.SetActive(true);
            f.FogRenderer.material = fogUnitVisibleMaterial;
            f.Health = health;
            f.SetStartEmotion(angry);
            f.RenderOpacity();

            fogUnitsInPlay.Insert(Random.Range(0, fogUnitsInPlay.Count), f);
        }
        else if (f.ActiveOnTile)
        {
            Debug.Log($"Error: Cannot spawn Fog.fogUnits[{t.X}, {t.Z}]; it is already in play.");
        }
    }

    //Spawning Methods - Angry Fog Evaporation Effect------------------------------------------------------------------------------------------------

    //Instantiates and returns a new fog evaporation particle effect
    private ParticleSystem CreateFogEvaporation()
    {
        return Instantiate(fogEvaporation, transform, true);
    }

    //Gets a pooled fog evaporation particle effect if there is one, and creates a new one if there isn't.
    private ParticleSystem GetFogEvaporation(Vector3 position)
    {
        ParticleSystem e;

        if (evaporationInPool.Count > 0)
        {
            e = evaporationInPool[0];
            evaporationInPool.Remove(e);           
        }
        else
        {
            e = CreateFogEvaporation();
        }

        evaporationInPlay.Add(e);
        e.transform.position = position;
        return e;
    }

    //Spawning Methods - Fog Spheres-----------------------------------------------------------------------------------------------------------------

    //Instantiates a fog sphere that isn't on the board or in the pool
    private FogSphere CreateFogSphere()
    {
        FogSphere f = Instantiate<FogSphere>(fogSpherePrefab, transform, true);
        f.transform.position = transform.position;
        f.State = FogSphereState.None;
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
        FogSphereWaypoint sp = GetSpawnPoint();

        if (sp != null)
        {
            FogUnit fu = WorldController.Instance.GetTileAt(sp.transform.position).FogUnit;
            FogSphere fs = GetFogSphere();

            foreach (Renderer r in fs.Renderers)
            {
                r.material = fogSphereVisibleMaterial;
            }

            fs.name = "FogSphereInPlay";
            fs.transform.position = fu.transform.position;
            fs.Waypoint = sp.GetNextWaypoint();
            fs.transform.DOLookAt(fs.Waypoint.transform.position, 0);

            fs.SpawningTile = fu.Location;
            fs.Health = fogSphereMinHealth;
            fs.MaxHealth = fogSphereMaxHealth;
            fs.MaxSizeScale = fogSphereMaxSizeScale;
            fs.FogUnitMinHealth = fogUnitMinHealth;
            fs.FogUnitMaxHealth = fogUnitMaxHealth;
            fs.State = FogSphereState.MovingAndGrowing;
            fs.SetStartEmotion(angry);
            fs.RandomiseMovementSpeed();
            fs.UpdateSize();
            fs.RenderOpacity();
            fogSpheresInPlay.Add(fs);
        }
    }

    //Gets a random fog unit at the edge of the board
    private FogSphereWaypoint GetSpawnPoint()
    {
        List<FogSphereWaypoint> validSpawnPoints = new List<FogSphereWaypoint>(fogSphereSpawnPoints);

        while (validSpawnPoints.Count > 0)
        {
            bool valid = true;
            FogSphereWaypoint sp = validSpawnPoints[Random.Range(0, validSpawnPoints.Count - 1)];
            validSpawnPoints.Remove(sp);

            if (!WorldController.Instance.TileExistsAt(sp.transform.position))
            {
                continue;
            }

            TileData t = WorldController.Instance.GetTileAt(sp.transform.position);

            if (t.FogUnit == null || !t.FogUnitActive || t.FogUnit.Health < t.FogUnit.MaxHealth)
            {
                continue;
            }

            foreach (TileData a in t.AdjacentTiles)
            {
                if (a.FogUnit == null || !a.FogUnitActive || a.FogUnit.Health < a.FogUnit.MaxHealth * 0.5)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                return sp;
            }       
        }

        return null;
    }

    //Activating the Fog-----------------------------------------------------------------------------------------------------------------------------

    //Turns on the fog's sensitivity to damage
    public void BeginUpdatingDamage()
    {
        BeginUpdatingDamage(0);
    }
    
    //Turns on the fog's sensitivity to damage
    public void BeginUpdatingDamage(float delay)
    {
        StartCoroutine(UpdateDamageToFogUnits(delay));
    }

    //Wakes up the fog, turning on its filling and expansion, and fog spheres
    public void WakeUpFog(float delay)
    {
        StartCoroutine(UpdateFogUnitFill(delay + 0.1f));
        StartCoroutine(ExpandFog(delay + 0.3f));
        StartCoroutine(UpdateFogSpheres(delay + delay + 1f));
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

    //Handles the damaging of fog units
    IEnumerator UpdateDamageToFogUnits(float delay)
    {
        yield return new WaitForSeconds(delay);
        List<FogUnit> toRender = new List<FogUnit>();
        List<ParticleSystem> evaporationToPool = new List<ParticleSystem>();

        while (fogUnitsInPlay.Count > 0)
        {
            //Check for fog units taking damage
            foreach (FogUnit f in fogUnitsInPlay)
            {
                if (f.TakingDamage)
                {
                    f.UpdateDamageToFogUnit(fogDamageInterval);
                    toRender.Add(f);
                }

                if (f.ReturnToPool)
                {
                    QueueFogUnitForPooling(f);
                    f.ReturnToPool = false;
                }
            }

            //Don't render those that are dead
            if (fogUnitsToReturnToPool.Count > 0)
            {
                foreach (FogUnit f in fogUnitsToReturnToPool)
                {
                    toRender.Remove(f);
                    ReturnFogUnitToPool(f);
                }

                fogUnitsToReturnToPool.Clear();
            }

            //Render those that aren't dead
            foreach (FogUnit f in toRender)
            {
                f.RenderOpacity();

                if (usingFogEvaporation && !f.Evaporating)
                {
                    f.Evaporating = true;
                    ParticleSystem e = GetFogEvaporation(f.transform.position);
                    e.transform.position = f.transform.position;
                    e.Play();
                }
            }

            toRender.Clear();

            if (usingFogEvaporation)
            {
                //Check for evaporations that are done
                foreach (ParticleSystem e in evaporationInPlay)
                {
                    if (!e.isPlaying)
                    {
                        evaporationToPool.Add(e);
                    }
                }

                //Pool those that are
                if (evaporationToPool.Count > 0)
                {
                    foreach (ParticleSystem e in evaporationToPool)
                    {
                        WorldController.Instance.GetTileAt(e.transform.position).FogUnit.Evaporating = false;
                        evaporationInPlay.Remove(e);
                        evaporationInPool.Add(e);
                    }

                    evaporationToPool.Clear();
                }
            }            

            yield return new WaitForSeconds(fogDamageInterval);
        }

        ObjectiveController.Instance.FogDestroyed();
    }

    //Fills the health of fog units
    IEnumerator UpdateFogUnitFill(float delay)
    {
        yield return new WaitForSeconds(delay);
        float framesPerFillUpdate;
        int unitsPerFrame;
        float timeOfLastFillUpdate; //If done faster than the interval, wait the difference in time before resuming.
        float timeOfLastFill;
        int alertCount = 0;

        while (fogUnitsInPlay.Count > 0)
        {
            timeOfLastFillUpdate = Time.time;
            framesPerFillUpdate = fogFillInterval / Time.deltaTime;
            unitsPerFrame = (int)(fogUnitsInPlay.Count / framesPerFillUpdate);
            timeOfLastFill = 0;

            if (!fogFrozen)
            {
                if (fogAccelerates && fogGrowth < 100)
                {
                    fogGrowth += fogFillInterval;
                }

                for (int i = 0; i < framesPerFillUpdate; i++)   //For each frame in the fog fill interval
                {
                    List<FogUnit> toRenderOpacity = new List<FogUnit>();
                    int startIndex = unitsPerFrame * i;
                    int endIndex = Mathf.Min(unitsPerFrame * (i + 1), fogUnitsInPlay.Count);    //Ensures it doesn't overshoot

                    if (i == framesPerFillUpdate - 1 && endIndex != fogUnitsInPlay.Count)   //Ensures it doesn't leave out any fog units
                    {
                        endIndex = fogUnitsInPlay.Count;
                    }

                    for (int j = startIndex; j < endIndex; j++) //For each unit that should be fillable within this frame
                    {
                        FogUnit f = fogUnitsInPlay[j];

                        f.RenderColour();

                        if (!f.NeighboursFull && f.Health >= fogSpillThreshold)
                        {
                            int count = f.Location.AdjacentTiles.Count;
                            int fullCount = 0;

                            foreach (TileData t in f.Location.AdjacentTiles)
                            {
                                if (t.FogUnitActive)
                                {
                                    FogUnit af = t.FogUnit;

                                    if (af.Health < f.Health)
                                    {
                                        af.Health += fogFillInterval * fogGrowth / count;

                                        if (!toRenderOpacity.Contains(af))
                                        {
                                            toRenderOpacity.Add(af);
                                        }
                                    }
                                    else if (af.Health >= fogUnitMaxHealth)
                                    {
                                        fullCount++;
                                    }
                                }
                            }

                            if (count == fullCount)
                            {
                                f.NeighboursFull = true;
                            }
                        }                        
                    }

                    if (toRenderOpacity.Count > 0)
                    {
                        timeOfLastFill = Time.time;
                        alertCount++;

                        foreach (FogUnit f in toRenderOpacity)
                        {
                            f.RenderOpacity();
                        }
                    }                   

                    yield return null;
                }
            }

            float duration = Time.time - timeOfLastFillUpdate;
            float fogFillDelayToNextCycle = Mathf.Max(0, fogFillInterval - duration);
            Debug.Log($"FogUnit fill cycle finished. Alert no.: {alertCount}, Time: {(int)Time.time}, Delay until next cycle: {fogFillDelayToNextCycle} seconds, Time since last fill: {Time.time - timeOfLastFill}");
            yield return new WaitForSeconds(fogFillDelayToNextCycle);       
        }
    }

    //Fog spills over onto adjacent tiles
    IEnumerator ExpandFog(float delay)
    {
        yield return new WaitForSeconds(delay);
        float framesPerExpansionUpdate;
        int unitsPerFrame;
        float timeOfLastExpansionUpdate; //If done faster than the interval, wait the difference in time before resuming.

        while (fogUnitsInPlay.Count > 0)
        {
            timeOfLastExpansionUpdate = Time.time;
            framesPerExpansionUpdate = fogExpansionInterval / Time.deltaTime;
            unitsPerFrame = (int)(fogUnitsInPlay.Count / framesPerExpansionUpdate);

            for (int i = 0; i < framesPerExpansionUpdate; i++)   //For each frame in the fog expansion interval
            {
                List<TileData> newTiles = new List<TileData>();
                int startIndex = unitsPerFrame * i;
                int endIndex = Mathf.Min(unitsPerFrame * (i + 1), fogUnitsInPlay.Count);    //Ensures it doesn't overshoot

                if (i == framesPerExpansionUpdate - 1 && endIndex != fogUnitsInPlay.Count)   //Ensures it doesn't leave out any fog units
                {
                    endIndex = fogUnitsInPlay.Count;
                }

                for (int j = startIndex; j < endIndex; j++) //For each unit that should be checkable within this frame
                {
                    FogUnit f = fogUnitsInPlay[j];

                    if (!f.Spill && f.Health >= fogSpillThreshold)
                    {
                        f.Spill = true;

                        foreach (TileData a in f.Location.AdjacentTiles)
                        {
                            if (!a.buildingChecks.obstacle && !a.FogUnitActive && !newTiles.Contains(a))
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
                        SpawnFogUnit(n.X, n.Z, fogUnitMinHealth);
                    }
                }

                if (fogUnitsInPlay.Count > xCount * zCount)
                {
                    Debug.Log("More fog units than board tiles. There must be some overlapping.");
                }

                yield return null;
            }

            float duration = Time.time - timeOfLastExpansionUpdate;
            yield return new WaitForSeconds(Mathf.Max(0, fogExpansionInterval - duration));
        }
    }

    //Handles the fog spheres
    IEnumerator UpdateFogSpheres(float delay)
    {
        yield return new WaitForSeconds(delay);

        while (fogUnitsInPlay.Count > 0)
        {
            if (!fogFrozen)
            {
                foreach (FogSphere f in fogSpheresInPlay)
                {
                    f.RenderColour();

                    switch (f.State)
                    {
                        case FogSphereState.Damaged:
                            f.UpdateDamageToFogSphere(fogSphereInterval);
                            break;
                        case FogSphereState.MovingAndGrowing:
                            f.Move(fogSphereInterval * 0.5f);
                            f.Grow(fogSphereInterval * fogGrowth * 0.75f);
                            break;
                        case FogSphereState.Spilling:
                            f.Move(fogSphereInterval * 0.5f);
                            f.Spill(fogSphereInterval * fogGrowth);
                            break;
                        case FogSphereState.Attacking:
                            f.Attack(fogSphereInterval * fogGrowth);
                            break;
                    }
                }

                if (fogSpheresToReturnToPool.Count > 0)
                {
                    foreach (FogSphere f in fogSpheresToReturnToPool)
                    {
                        ReturnFogSphereToPool(f);
                    }

                    fogSpheresToReturnToPool.Clear();
                }
            }

            if (fogSpheresInPlay.Count < maxFogSpheresCount && !WorldController.Instance.GameOver)
            {
                SpawnFogSphere();
            }

            yield return new WaitForSeconds(fogSphereInterval);
        }        
    }

    //Re-Pooling Methods-----------------------------------------------------------------------------------------------------------------------------

    //Puts the fog unit in the list of fog units to be put back in the pool
    public void QueueFogUnitForPooling(FogUnit f)
    {
        fogUnitsToReturnToPool.Add(f);
    }

    //Takes the fog unit off the board and puts it back in the pool
    private void ReturnFogUnitToPool(FogUnit f)
    {
        f.ActiveOnTile = false;
        f.Location.FogUnitActive = false;
        f.name = $"{f.name} (In Pool)";

        foreach (TileData t in f.Location.AdjacentTiles)
        {
            if (t.FogUnitActive)
            {
                t.FogUnit.Spill = false;
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
        f.FogRenderer.material = invisibleMaterial;
        f.Spill = false;
        f.transform.position = transform.position;

        fogUnitsInPlay.Remove(f);
    }

    //Puts the fog sphere in the list of fog spheres to be put back in the pool
    public void QueueFogSphereForPooling(FogSphere f)
    {
        fogSpheresToReturnToPool.Add(f);
    }

    //Takes the fog unit off the board and puts it back in the pool
    private void ReturnFogSphereToPool(FogSphere f)
    {
        f.gameObject.name = "FogSphereInPool";
        f.gameObject.SetActive(false);
        f.transform.position = transform.position;
        f.State = FogSphereState.None;
        f.Waypoint = null;

        foreach(Renderer r in f.Renderers)
        {
            r.material = invisibleMaterial;
        }

        foreach (FogUnit u in f.SpiltFog)
        {
            u.FillingFromFogSphere = false;
        }

        f.SpiltFog.Clear();
        fogSpheresInPool.Add(f);
        fogSpheresInPlay.Remove(f);
    }

    //Fog Freezing Methods---------------------------------------------------------------------------------------------------------------------------

    //Pauses the fog growth for a specified amount of duration
    public void FreezeFog(float duration)
    {
        fogFrozen = true;

        foreach (FogUnit f in fogUnitsInPlay)
        {
            f.GetComponent<Renderer>().material.SetFloat("_FPS", 0f);
        }

        StartCoroutine(UnFreezeFog(duration));
    }

    //Handles timing out of the fog freeze
    IEnumerator UnFreezeFog(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (FogUnit f in fogUnitsInPlay)
        {
            f.FogRenderer.material.SetFloat("_FPS", 16f);
        }

        fogFrozen = false;
    }

    //Fog Lightning Methods--------------------------------------------------------------------------------------------------------------------------
    
    private void InitializePool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddLightningToPool();
        }
    }

    private void AddLightningToPool()
    {
        FogLightning fogLightning = Instantiate(fogUnitPrefab.fogLightning).GetComponent<FogLightning>();
        fogLightning.lightning.transform.SetParent(transform);
        fogLightning.lightning.SetActive(false);
        fogLightning.LightningPS = fogLightning.lightning.GetComponent<ParticleSystem>();
        lightningInPool.Add(fogLightning);
    }

    private FogLightning GetLightningFromPool()
    {
        if (lightningInPool.Count > 0)
        {
            FogLightning fogLightning = lightningInPool[0];
            lightningInPool.RemoveAt(0);
            lightningInPlay.Add(fogLightning);
            return fogLightning;
        }
        else
        {
            FogLightning fogLightning = Instantiate(fogUnitPrefab.fogLightning).GetComponent<FogLightning>();
            fogLightning.lightning.transform.SetParent(transform);
            fogLightning.LightningPS = fogLightning.lightning.GetComponent<ParticleSystem>();
            lightningInPlay.Add(fogLightning);
            return fogLightning;
        }
    }

    private void FixedUpdate()
    {
        SelectedLightning();
    }

    private void SelectedLightning()
    {
        FogUnit f = fogUnits[Random.Range(0, 70), Random.Range(0, 70)];

        if (f.ActiveOnTile)
        {
            PlayLightning(f);
        }
    }

    private void PlayLightning(FogUnit fogUnit)
    {
        FogLightning fogLightning = GetLightningFromPool();
        fogLightning.lightning.transform.position = fogUnit.transform.position;
        fogLightning.lightning.SetActive(true);
        fogLightning.LightningPS.Play();
    }

    private void ReturnLightningToPool()
    {
        List<FogLightning> toRemove = new List<FogLightning>();

        foreach (FogLightning fogLightning in lightningInPlay)
        {
            if (fogLightning.LightningPS.isStopped)
            {
                toRemove.Add(fogLightning);
            }
        }

        foreach (FogLightning fogLightning in toRemove)
        {
            fogLightning.lightning.SetActive(false);
            lightningInPlay.Remove(fogLightning);
            lightningInPool.Add(fogLightning);
        }
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
