﻿using System.Collections;
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
    [SerializeField] public Difficulty selectDifficulty = Difficulty.normal;
    [SerializeField] private bool lerpStartingFog = true;
    [SerializeField] private FogUnit fogUnitPrefab;
    [SerializeField] private StartConfiguration configuration;
    [SerializeField] private FogExpansionDirection expansionDirection;
    [SerializeField] private bool fogAccelerates = true;
    [SerializeField] private float fogGrowth = 5;
    [SerializeField] private float fogMaxHealth = 100f;
    [SerializeField] private float fogSpillThreshold = 50f;
    private float fogDamage;
    [SerializeField] private bool damageOn = false;
    [SerializeField] private float surroundingHubRange;

    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material invisibleMaterial;

    [SerializeField] private float fogLerpInInterval = 0.02f;
    [SerializeField] private float fogLerpInIncrement = 1f;
    [SerializeField] private float fogFillInterval = 0.2f;
    [SerializeField] private float fogDamageInterval = 0.02f;
    [SerializeField] private float fogExpansionInterval = 0.5f;

    [SerializeField] private bool angry = false;

    //Private Fields
    private WorldController wc;
    private int xMax;
    private int zMax;

    //Private Container Fields
    private List<TileData> fogCoveredTiles = new List<TileData>();              //i.e. tiles currently covered by fog
    private List<FogUnit> fogUnitsInPlay = new List<FogUnit>();                 //i.e. currently active fog units on the board
    private List<FogUnit> fogUnitsToReturnToPool = new List<FogUnit>();         //i.e. currently waiting to be re-pooled
    private List<FogUnit> fogUnitsInPool = new List<FogUnit>();                 //i.e. currently inactive fog units waiting for spawning

    //Public Properties
    public static Fog Instance { get; protected set; }
    public bool DamageOn { get => damageOn; set => damageOn = value; }
    public FogExpansionDirection ExpansionDirection { get => expansionDirection; }
    public float FogGrowth { get => fogGrowth; set => fogGrowth = value; }
    public bool FogAccelerates { get => fogAccelerates; set => fogAccelerates = value; }
    public float FogMaxHealth { get => fogMaxHealth; set => fogMaxHealth = value; }

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------
    public void SetDifficulty()
    {
        fogDamage = fogUnitPrefab.damage;
        switch (selectDifficulty)
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

    //Fog's awake method sets the static instance of Fog
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should not be more than one Fog");
        }
        Instance = this;
    }

    //Create the max no. of fog units the game should need
    //Pooling methods adapted / borrowed from: https://catlikecoding.com/unity/tutorials/object-pools/
    public void PopulateFogPool()
    {
        SetDifficulty();
        wc = WorldController.Instance;
        xMax = wc.Width;
        zMax = wc.Length;

        if (fogUnitsInPool.Count == 0)
        {
            for (int i = 0; i < xMax * zMax; i++)
            {
                fogUnitsInPool.Add(CreateFogUnit());
            }
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
        switch (startConfiguration)
        {
            case StartConfiguration.OneSide:
                //Spawns fog on one side of the board.
                for (int i = 0; i < xMax; i++)
                {
                    SpawnFogUnitWithMinHealth(i, zMax - 1);
                }

                break;
            case StartConfiguration.Corners:
                //Corner spaces
                SpawnFogUnitWithMinHealth(0, 0);
                SpawnFogUnitWithMinHealth(0, zMax - 1);
                SpawnFogUnitWithMinHealth(xMax - 1, 0);
                SpawnFogUnitWithMinHealth(xMax - 1, zMax - 1);
                break;

            case StartConfiguration.FourCompassPoints:
                //Four compass points
                SpawnFogUnitWithMinHealth(Mathf.RoundToInt(xMax / 2), 0);
                SpawnFogUnitWithMinHealth(Mathf.RoundToInt(xMax / 2), zMax - 1);
                SpawnFogUnitWithMinHealth(0, Mathf.RoundToInt(zMax / 2));
                SpawnFogUnitWithMinHealth(xMax - 1, Mathf.RoundToInt(zMax / 2));
                break;
            case StartConfiguration.EightCompassPoints:
                //Corner spaces
                SpawnFogUnitWithMinHealth(0, 0);
                SpawnFogUnitWithMinHealth(0, zMax - 1);
                SpawnFogUnitWithMinHealth(xMax - 1, 0);
                SpawnFogUnitWithMinHealth(xMax - 1, zMax - 1);

                //Four compass points
                SpawnFogUnitWithMinHealth(Mathf.RoundToInt(xMax / 2), 0);
                SpawnFogUnitWithMinHealth(Mathf.RoundToInt(xMax / 2), zMax - 1);
                SpawnFogUnitWithMinHealth(0, Mathf.RoundToInt(zMax / 2));
                SpawnFogUnitWithMinHealth(xMax - 1, Mathf.RoundToInt(zMax / 2));

                break;
            case StartConfiguration.FourSides:
                //Each side
                for (int i = 1; i < xMax - 1; i++)
                {
                    SpawnFogUnitWithMinHealth(i, 0);
                    SpawnFogUnitWithMinHealth(i, zMax - 1);
                }

                for (int i = 1; i < zMax - 1; i++)
                {
                    SpawnFogUnitWithMinHealth(0, i);
                    SpawnFogUnitWithMinHealth(xMax - 1, i);
                }

                //Corner spaces
                SpawnFogUnitWithMinHealth(0, 0);
                SpawnFogUnitWithMinHealth(0, zMax - 1);
                SpawnFogUnitWithMinHealth(xMax - 1, 0);
                SpawnFogUnitWithMinHealth(xMax - 1, zMax - 1);

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
                            SpawnFogUnitWithMinHealth(i, j);
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
                        SpawnFogUnitWithMinHealth(i, j);
                    }
                }
                break;
        }

        if (lerpStartingFog)
        {
            InvokeRepeating(nameof(LerpStartingFogToMaxHealth), fogLerpInInterval, fogLerpInInterval);
        }
    }

    //Smoothly lerps the fog in when it spawns instead of the fog being like "BAM!! I'm heeeeEEEEERRRREEEEEE!!!"
    private void LerpStartingFogToMaxHealth()
    {
        bool finished = false;

        foreach (FogUnit f in fogUnitsInPlay)
        {
            f.Health += fogLerpInIncrement;

            if (f.Health >= fogMaxHealth)
            {
                f.Health = fogMaxHealth;
                finished = true;
            }

            f.RenderOpacity();
        }

        if (finished)
        {
            CancelInvoke(nameof(LerpStartingFogToMaxHealth));
        }
    }

    //Take a fog unit and puts it on the board with maximum health
    private void SpawnFogUnitWithMinHealth(int x, int z)
    {
        SpawnFogUnit(x, z, 0.0001f);
    }

    //Takes a fog unit and puts it on the board
    private void SpawnFogUnit(int x, int z, float health)
    {
        GameObject fGO = GetFogUnit().gameObject;
        FogUnit f = fGO.GetComponent<FogUnit>();
        Vector2 pos = new Vector2(x, z);
        TileData t = wc.GetTileAt(pos);
        Transform ft = fGO.transform;
        
        ft.position = new Vector3(x, 0.13f, z);
        ft.SetPositionAndRotation(new Vector3(x, 0.13f, z), Quaternion.Euler(ft.rotation.eulerAngles.x, Random.Range(0, 360), ft.rotation.eulerAngles.z));
        fGO.name = "FogUnit(" + x + "," + z + ")";

        f.gameObject.GetComponent<Renderer>().material = visibleMaterial;
        f.Location = t;
        f.Health = health;
        f.Spill = false;
        f.SetStartEmotion(angry);
        t.FogUnit = f;

        fogUnitsInPlay.Add(f);
        fogCoveredTiles.Add(t);

        f.RenderOpacity();
    }

    //Instantiates a fog unit that isn't on the board or in the pool
    private FogUnit CreateFogUnit()
    {
        FogUnit f = Instantiate<FogUnit>(fogUnitPrefab, transform, true);
        f.transform.position = transform.position;
        f.MaxHealth = fogMaxHealth;
        f.damage = fogDamage;
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
                SpawnFogUnitWithMinHealth(n.X, n.Z);        //SpawnFogUnit adds the tile spawned on to the list fogTiles
            }
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

        foreach (TileData t in f.Location.AdjacentTiles)
        {
            if (t.FogUnit != null)
            {
                t.FogUnit.Spill = false;
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

    //Other Methods----------------------------------------------------------------------------------------------------------------------------------

    public void ToggleAnger()
    {
        angry = !angry;

        foreach (FogUnit f in fogUnitsInPlay)
        {
            f.Angry = angry;
        }
    }
}
