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
    easy,
    normal,
    hard
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
    [SerializeField] private Difficulty difficulty = Difficulty.normal;
    [SerializeField] private FogExpansionDirection expansionDirection;

    [Header("Fog Expansion")]
    [SerializeField] private bool fogAccelerates = true;
    [SerializeField] private float fogGrowth = 5;
    [SerializeField] private float fogSpillThreshold = 50f;
    [SerializeField] private float fogMaxHealth = 100f;

    [Header("Update Intervals")]
    [SerializeField] private float fogFillInterval = 0.2f;
    [SerializeField] private float fogDamageInterval = 0.02f;
    [SerializeField] private float fogExpansionInterval = 0.5f;

    [Header("Other")]
    [SerializeField] private bool angry = false;
    [SerializeField] private bool damageOn = false;
    [SerializeField] private float surroundingHubRange;

    //Private Fields
    private int xMax;
    private int zMax;
    private float minHealth = 0.0001f;
    private float fogDamage;

    //Private Container Fields
    private List<TileData>  fogCoveredTiles = new List<TileData>();                  //i.e. tiles currently covered by fog
    private List<FogUnit>   fogUnitsInPlay = new List<FogUnit>();                     //i.e. currently active fog units on the board
    private List<FogUnit>   fogUnitsToReturnToPool = new List<FogUnit>();             //i.e. currently waiting to be re-pooled
    private List<FogUnit>   fogUnitsInPool = new List<FogUnit>();                     //i.e. currently inactive fog units waiting for spawning
    private List<FogSphere> fogSpheresInPlay = new List<FogSphere>();               //i.e. currently active fog spheres on the board
    private List<FogSphere> fogSpheresToReturnToPool = new List<FogSphere>();       //i.e. currently waiting to be re-pooled
    private List<FogSphere> fogSpheresInPool = new List<FogSphere>();               //i.e. currently inactive fog spheres waiting for spawning

    //Public Properties
    public static Fog            Instance { get; protected set; }
    public bool                  DamageOn { get => damageOn; set => damageOn = value; }
    //public bool                  FogAccelerates { get => fogAccelerates; set => fogAccelerates = value; }
    public FogExpansionDirection ExpansionDirection { get => expansionDirection; }
    public float                 FogGrowth { get => fogGrowth; set => fogGrowth = value; }
    //public float                 FogMaxHealth { get => fogMaxHealth; set => fogMaxHealth = value; }
    public Difficulty            Difficulty { get => difficulty; set => difficulty = value; }

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
            case Difficulty.easy:
                fogDamage = fogDamage / 1.40f;
                break;
            case Difficulty.normal:
                break;
            case Difficulty.hard:
                fogDamage = fogDamage * 2f;
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
                    SpawnFogUnit(i, zMax - 1, fogMaxHealth);
                }

                break;
            case StartConfiguration.Corners:
                //Corner spaces
                SpawnFogUnit(0, 0, fogMaxHealth);
                SpawnFogUnit(0, zMax - 1, fogMaxHealth);
                SpawnFogUnit(xMax - 1, 0, fogMaxHealth);
                SpawnFogUnit(xMax - 1, zMax - 1, fogMaxHealth);
                break;

            case StartConfiguration.FourCompassPoints:
                //Four compass points
                SpawnFogUnit(Mathf.RoundToInt(xMax / 2), 0, fogMaxHealth);
                SpawnFogUnit(Mathf.RoundToInt(xMax / 2), zMax - 1, fogMaxHealth);
                SpawnFogUnit(0, Mathf.RoundToInt(zMax / 2), fogMaxHealth);
                SpawnFogUnit(xMax - 1, Mathf.RoundToInt(zMax / 2), fogMaxHealth);
                break;
            case StartConfiguration.EightCompassPoints:
                //Corner spaces
                SpawnFogUnit(0, 0, fogMaxHealth);
                SpawnFogUnit(0, zMax - 1, fogMaxHealth);
                SpawnFogUnit(xMax - 1, 0, fogMaxHealth);
                SpawnFogUnit(xMax - 1, zMax - 1, fogMaxHealth);

                //Four compass points
                SpawnFogUnit(Mathf.RoundToInt(xMax / 2), 0, fogMaxHealth);
                SpawnFogUnit(Mathf.RoundToInt(xMax / 2), zMax - 1, fogMaxHealth);
                SpawnFogUnit(0, Mathf.RoundToInt(zMax / 2), fogMaxHealth);
                SpawnFogUnit(xMax - 1, Mathf.RoundToInt(zMax / 2), fogMaxHealth);

                break;
            case StartConfiguration.FourSides:
                //Each side
                for (int i = 1; i < xMax - 1; i++)
                {
                    SpawnFogUnit(i, 0, fogMaxHealth);
                    SpawnFogUnit(i, zMax - 1, fogMaxHealth);
                }

                for (int i = 1; i < zMax - 1; i++)
                {
                    SpawnFogUnit(0, i, fogMaxHealth);
                    SpawnFogUnit(xMax - 1, i, fogMaxHealth);
                }

                //Corner spaces
                SpawnFogUnit(0, 0, fogMaxHealth);
                SpawnFogUnit(0, zMax - 1, fogMaxHealth);
                SpawnFogUnit(xMax - 1, 0, fogMaxHealth);
                SpawnFogUnit(xMax - 1, zMax - 1, fogMaxHealth);

                break;
            case StartConfiguration.SurroundingHub:
                Vector3 hubPosition = GameObject.Find("Hub").transform.position;

                //Every space on the board
                for (int i = 0; i < xMax; i++)
                {
                    for (int j = 0; j < zMax; j++)
                    {
                        //Except those within a specified range of the hub
                        if (Vector3.Distance(hubPosition, new Vector3(i, hubPosition.y, j)) > surroundingHubRange)
                        {
                            SpawnFogUnit(i, j, fogMaxHealth);
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
                        SpawnFogUnit(i, j, fogMaxHealth);
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
        f.MaxHealth = fogMaxHealth;
        f.Damage = fogDamage;
        f.Fog = this;
        return f;
    }

    //Retrieves a fog unit from the pool, or asks for a new one if the pool is empty
    private FogUnit GetFogUnit()
    {
        FogUnit f;
        int lastAvailableIndex = fogUnitsInPool.Count - 1;

        if (lastAvailableIndex >= 0)
        {
            f = fogUnitsInPool[lastAvailableIndex];
            fogUnitsInPool.RemoveAt(lastAvailableIndex);
            f.gameObject.SetActive(true);
        }
        else
        {
            f = CreateFogUnit();
        }

        return f;
    }

    //Take a fog unit and puts it on the board with maximum health
    //private void SpawnFogUnitWithMinHealth(int x, int z)
    //{
    //    SpawnFogUnit(x, z, minHealth);
    //}

    //Take a fog unit and puts it on the board with maximum health
    //private void SpawnFogUnitWithMaxHealth(int x, int z)
    //{
    //    SpawnFogUnit(x, z, fogMaxHealth);
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

        fGO.GetComponent<Renderer>().material = visibleMaterial;
        f.Location = t;
        f.Health = health;
        f.Spill = false;
        f.SetStartEmotion(angry);
        t.FogUnit = f;

        fogUnitsInPlay.Add(f);
        fogCoveredTiles.Add(t);

        f.RenderOpacity();
    }

    //Instantiates a fog sphere that isn't on the board or in the pool
    private FogSphere CreateFogSphere()
    {
        FogSphere f = Instantiate<FogSphere>(fogSpherePrefab, transform, true);
        f.transform.position = transform.position;
        f.MaxHealth = fogMaxHealth;
        f.Fog = this;
        return f;
    }

    //Retrieves a fog sphere from the pool, or asks for a new one if the pool is empty
    private FogSphere GetFogSphere()
    {
        FogSphere f;
        int lastAvailableIndex = fogUnitsInPool.Count - 1;

        if (lastAvailableIndex >= 0)
        {
            f = fogSpheresInPool[lastAvailableIndex];
            fogUnitsInPool.RemoveAt(lastAvailableIndex);
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
        //fGO.name = "FogSphere(" + x + "," + z + ")";
        fGO.transform.position = GetFogSpherePosition();
        fGO.GetComponent<Renderer>().material = visibleMaterial;
        f.Health = minHealth;
        f.SetStartEmotion(angry);
        fogSpheresInPlay.Add(f);
        f.RenderOpacity();
    }

    //Calculates the position for the fog sphere
    private Vector3 GetFogSpherePosition()
    {
        return Vector3.zero;
    }

    //Invokes the ActivateFog method according to the parameter passed to it.
    public void InvokeActivateFog(int delay)
    {
        Invoke(nameof(ActivateFog), delay);
    }

    //Invokes the "update" methods of Fog according to the intervals set in the inspector
    public void ActivateFog()
    {
        InvokeRepeating(nameof(UpdateFogFill), 0.1f, fogFillInterval);
        InvokeRepeating(nameof(CheckExpandFog), 0.3f, fogExpansionInterval);
        InvokeRepeating(nameof(UpdateDamageToFogUnits), 0.5f, fogDamageInterval);
        Invoke(nameof(UpdateFogSpheres), 1f);
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

        foreach (FogUnit f in fogUnitsToReturnToPool)
        {
            toRender.Remove(f);
            ReturnFogUnitToPool(f);
        }

        fogUnitsToReturnToPool = new List<FogUnit>();

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
    private void UpdateFogFill()
    {
        List<FogUnit> toRenderOpacity = new List<FogUnit>();

        if (fogAccelerates && fogGrowth < 100)
        {
            fogGrowth += fogFillInterval;
        }

        FogUnit af;

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
                af = t.FogUnit;

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
                else if (af.Health >= fogMaxHealth)
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
        if (fogSpheresInPlay.Count == 0)
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
            foreach (TileData t in f.Location.AdjacentTiles)
            {
                if (t.FogUnit != null)
                {
                    t.FogUnit.Spill = false;
                }
            }
        }

        f.gameObject.SetActive(false);
        f.Location.FogUnit = null;
        f.gameObject.GetComponent<Renderer>().material = invisibleMaterial;
        f.Spill = true;
        f.gameObject.transform.position = transform.position;

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
        f.gameObject.GetComponent<Renderer>().material = invisibleMaterial;
        //f.Spill = true;
        f.gameObject.transform.position = transform.position;

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
    }
}
