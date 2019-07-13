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

    [Header("General Settings")]
    [SerializeField] private bool angry;
    [SerializeField] private bool damageOn;

    [SerializeField] private StartConfiguration configuration;
    [SerializeField] private Difficulty difficulty;
    [SerializeField] private FogExpansionDirection expansionDirection;

    [SerializeField] private float surroundingHubRange;

    [Header("Fog Expansion")]
    [SerializeField] private bool fogAccelerates;
    [SerializeField] private bool fogUnitsGrow;
    [SerializeField] private bool fogSpheresGrow;
    [SerializeField] private float maxFogSpheresCount;
    [SerializeField] private float fogGrowth;
    [SerializeField] private float fogSpillThreshold;

    [Header("Health")]
    [SerializeField] private float fogUnitMinHealth;
    [SerializeField] private float fogUnitMaxHealth;
    [SerializeField] private float fogSphereMinHealth;
    [SerializeField] private float fogSphereMaxHealth;

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
    private Vector3 hubPosition;

    //Private Collection Fields
    private List<TileData>  fogCoveredTiles = new List<TileData>();                 //i.e. tiles currently covered by fog

    private List<FogUnit>   fogUnitsInPlay = new List<FogUnit>();                   //i.e. currently active fog units on the board
    private List<FogUnit>   borderFogUnitsInPlay = new List<FogUnit>();             //i.e. currently active fog units around the edge of the board
    private List<FogUnit>   fogUnitsToReturnToPool = new List<FogUnit>();           //i.e. currently waiting to be re-pooled
    private List<FogUnit>   fogUnitsInPool = new List<FogUnit>();                   //i.e. currently inactive fog units waiting for spawning

    private List<FogSphere> fogSpheresInPlay = new List<FogSphere>();               //i.e. currently active fog spheres on the board
    private List<FogSphere> fogSpheresToReturnToPool = new List<FogSphere>();       //i.e. currently waiting to be re-pooled
    private List<FogSphere> fogSpheresInPool = new List<FogSphere>();               //i.e. currently inactive fog spheres waiting for spawning

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public static Fog            Instance { get; protected set; }
    public bool                  DamageOn { get => damageOn; set => damageOn = value; }
    public FogExpansionDirection ExpansionDirection { get => expansionDirection; }
    public float                 FogGrowth { get => fogGrowth; set => fogGrowth = value; }
    public Difficulty            Difficulty { get => difficulty; set => difficulty = value; }
    public int                   XMax { get => xMax; }
    public int                   ZMax { get => zMax; }

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
            Difficulty = (Difficulty)GlobalVars.Difficulty;
        }

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
        hubPosition = GameObject.Find("Hub").transform.position;

        //Populate fog unit pool with fog units
        if (fogUnitsInPool.Count == 0)
        {
            for (int i = 0; i < xCount * zCount; i++)
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
    }

    //Spawning Methods - Fog Units-------------------------------------------------------------------------------------------------------------------

    //Instantiates a fog unit that isn't on the board or in the pool
    private FogUnit CreateFogUnit()
    {
        FogUnit f = Instantiate<FogUnit>(fogUnitPrefab, transform, true);
        f.transform.position = transform.position;
        f.MaxHealth = fogUnitMaxHealth;
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
        SpawnFogUnit(t.X, t.Z, fogUnitMinHealth);
    }

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

        if (t.X == 0 || t.Z == 0 || t.X == xMax || t.Z == zMax)
        {
            borderFogUnitsInPlay.Add(f);
        }

        f.RenderOpacity();
    }

    //Spawning Methods - Fog Spheres-----------------------------------------------------------------------------------------------------------------

    //Instantiates a fog sphere that isn't on the board or in the pool
    private FogSphere CreateFogSphere()
    {
        FogSphere f = Instantiate<FogSphere>(fogSpherePrefab, transform, true);
        f.transform.position = transform.position;
        f.MaxHealth = fogSphereMaxHealth;
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
        FogUnit u = GetBorderFogUnit();

        if (u != null)
        {
            GameObject o = GetFogSphere().gameObject;
            FogSphere s = o.GetComponent<FogSphere>();
            o.name = "FogSphereInPlay";
            o.transform.position = u.transform.position;
            s.SpawningTile = u.Location;
            s.FogRenderer.material = visibleMaterial;
            s.Health = fogSphereMinHealth;
            s.FogUnitMinHealth = fogUnitMinHealth;
            s.FogUnitMaxHealth = fogUnitMaxHealth;
            s.State = FogSphereState.MovingAndGrowing;
            s.SetStartEmotion(angry);
            s.RandomiseMovementSpeed();
            fogSpheresInPlay.Add(s);
            s.UpdateHeight();
            s.RenderOpacity();
        }
    }

    //Gets a random fog unit at the edge of the board
    private FogUnit GetBorderFogUnit()
    {        
        FogUnit f = null;
        List<FogUnit> validBorderFogUnits = new List<FogUnit>(borderFogUnitsInPlay);

        while (validBorderFogUnits.Count > 0)
        {
            bool valid = true;
            f = validBorderFogUnits[Random.Range(0, validBorderFogUnits.Count - 1)];

            foreach (TileData t in f.Location.AdjacentTiles)
            {
                if (t.FogUnit == null)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                return f;
            }
            else
            {
                validBorderFogUnits.Remove(f);
                f = null;
            }            
        }

        return f;
    }

    //Activating the Fog-----------------------------------------------------------------------------------------------------------------------------

    //Invokes the WakeUpFog method according to the parameter passed to it
    public void InvokeWakeUpFog(int delay)
    {
        Invoke(nameof(WakeUpFog), delay);
    }

    //Invokes the "update" methods of Fog according to the intervals set in the inspector
    public void WakeUpFog()
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

            if (f.ReturnToPool)
            {
                QueueFogUnitForPooling(f);
                f.ReturnToPool = false;
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

                if (!f.NeighboursFull && f.Health >= fogSpillThreshold)
                {
                    int count = f.Location.AdjacentTiles.Count;
                    int fullCount = 0;

                    foreach (TileData t in f.Location.AdjacentTiles)
                    {
                        FogUnit af = t.FogUnit;

                        if (af != null)
                        {
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

            foreach (FogUnit f in toRenderOpacity)
            {
                f.RenderOpacity();
            }
        }
    }

    //Checks if the fog is able to spill over into adjacent tiles
    private void CheckExpandFog()
    {
        if (fogUnitsInPool.Count > 0)
        {
            ExpandFog();
        }
        else if (fogUnitsInPlay.Count < xCount * zCount)
        {
            Debug.Log("Ran out of fog units. If the board isn't full, there must be some overlapping.");
        }
        else if (fogUnitsInPlay.Count > xCount * zCount)
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
            if (!f.Spill && f.Health >= fogSpillThreshold)
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
                SpawnFogUnit(n.X, n.Z, fogUnitMinHealth);
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
                        break;
                    case FogSphereState.MovingAndGrowing:
                        f.Move(fogSphereInterval * 0.5f);
                        f.Grow(fogSphereInterval * fogGrowth * 0.75f);
                        break;
                    case FogSphereState.Spilling:
                        f.Spill(fogSphereInterval * fogGrowth * 2f);
                        break;
                    case FogSphereState.Attacking:
                        f.Attack(fogSphereInterval * fogGrowth);
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

        if (fogSpheresInPlay.Count < maxFogSpheresCount && !WorldController.Instance.GameOver)
        {
            SpawnFogSphere();
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
        f.gameObject.name = "FogUnitInPool";

        if (f.Location != null)
        {
            if (f.Location.X == 0 || f.Location.Z == 0 || f.Location.X == xMax || f.Location.Z == zMax)
            {
                borderFogUnitsInPlay.Add(f);
            }

            f.Location.FogUnit = null;

            foreach (TileData t in f.Location.AdjacentTiles)
            {
                if (t.FogUnit != null)
                {
                    t.FogUnit.Spill = false;
                }
            }
        }
        else if (borderFogUnitsInPlay.Contains(f))
        {
            borderFogUnitsInPlay.Remove(f);
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
        f.Spill = true;
        f.transform.position = transform.position;

        fogUnitsInPool.Add(f);
        fogUnitsInPlay.Remove(f);
        fogCoveredTiles.Remove(f.Location);
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
