﻿using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
    Chill,
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
    [SerializeField] private Material fogUnitVisibleMaterial;
    [SerializeField] private Material fogSphereVisibleMaterial;
    [SerializeField] private Material invisibleMaterial;
    [SerializeField] private Material fogEffect;

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

    [Header("Fog Strength Over Time")]
    [SerializeField] private float fogGrowthEasy;
    [SerializeField] private float fogGrowthMedium;
    [SerializeField] private float fogGrowthHard;
    [SerializeField] private float fogSphereEasyMaxHealth;
    [SerializeField] private float fogSphereMediumMaxHealth;
    [SerializeField] private float fogSphereHardMaxHealth;

    [Header("Fog Size Over Time")]
    [SerializeField] private float fogSphereEasyMaxSizeScale;
    [SerializeField] private float fogSphereMediumMaxSizeScale;
    [SerializeField] private float fogSphereHardMaxSizeScale;

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

    private bool fogActive = true;
    private float fogDamage;
    private float fogSphereMaxHealth;
    private float fogSphereMaxSizeScale;
    private int intensity;

    private Vector3 hubPosition;

    private float pauseTime;

    //Private Collection Fields
    private FogUnit[,]      fogUnits;                                               //i.e. all fog units being managed by the fog; effectively the pool of all fog units, pooled or in play

    private List<FogUnit>   fogUnitsInPlay = new List<FogUnit>();                   //i.e. currently active fog units on the board
    private List<FogUnit>   borderFogUnitsInPlay = new List<FogUnit>();             //i.e. currently active fog units around the edge of the board
    private List<FogUnit>   fogUnitsToReturnToPool = new List<FogUnit>();           //i.e. currently waiting to be re-pooled

    private List<FogSphere> fogSpheresInPlay = new List<FogSphere>();               //i.e. currently active fog spheres on the board
    private List<FogSphere> fogSpheresToReturnToPool = new List<FogSphere>();       //i.e. currently waiting to be re-pooled
    private List<FogSphere> fogSpheresInPool = new List<FogSphere>();               //i.e. currently inactive fog spheres waiting for spawning

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public static Fog            Instance { get; protected set; }
    public bool                  DamageOn { get => damageOn; set => damageOn = value; }
    public FogExpansionDirection ExpansionDirection { get => expansionDirection; }
    public List<FogUnit>         FogUnitsInPlay { get => fogUnitsInPlay; }
    public Difficulty            Difficulty { get => difficulty; set => difficulty = value; }
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
            if (intensity != value && value >= 1 && value <= 3)
            {
                intensity = value;

                switch (intensity)
                {
                    case 1:
                        fogGrowth = fogGrowthEasy;
                        fogSphereMaxHealth = fogSphereEasyMaxHealth;
                        fogSphereMaxSizeScale = fogSphereEasyMaxSizeScale;

                        if (angry)
                        {
                            ToggleAnger();
                        }

                        break;
                    case 2:
                        fogGrowth = fogGrowthMedium;
                        fogSphereMaxHealth = fogSphereMediumMaxHealth;
                        fogSphereMaxSizeScale = fogSphereMediumMaxSizeScale;

                        if (angry)
                        {
                            ToggleAnger();
                        }

                        break;
                    case 3:
                        fogGrowth = fogGrowthHard;
                        fogSphereMaxHealth = fogSphereHardMaxHealth;
                        fogSphereMaxSizeScale = fogSphereHardMaxSizeScale;

                        if (!angry)
                        {
                            ToggleAnger();
                        }

                        break;
                }
            }
            else if (value < 1)
            {
                Debug.Log("Fog.Instance.Intensity has to be >= 1 (and <= 3). Try again, you cabbage!");
            }
            else if (value > 3)
            {
                Debug.Log("Fog.Instance.Intensity has to be <= 3 (and >= 1). Try again, you cabbage!");
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
            Difficulty = (Difficulty)GlobalVars.Difficulty;
        }

        SetDifficulty();
        Intensity = 1;  //Sets the intensity-derived values to the default of their "Easy" values.
    }

    //Sets the difficulty according to what the player selects
    public void SetDifficulty()
    {
        fogDamage = fogUnitPrefab.Damage;

        switch (difficulty)
        {
            case Difficulty.Chill:
                fogDamage /= 2f;
                fogGrowthEasy /= 2;
                fogGrowthMedium /= 2;
                fogGrowthHard /= 2;
                break;
            case Difficulty.Easy:
                fogDamage /= 1.40f;
                fogGrowthEasy /= 2;
                fogGrowthMedium /= 2;
                fogGrowthHard /= 2;
                break;
            case Difficulty.Normal:
                break;
            case Difficulty.Hard:
                fogDamage *= 2f;
                fogGrowthEasy *= 2;
                fogGrowthMedium *= 2;
                fogGrowthHard *= 2;
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

        //Populate fog sphere pool with fog spheres
        if (fogSpheresInPool.Count == 0)
        {
            for (int i = 0; i < maxFogSpheresCount * 2; i++)
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
        if (!fogUnitsInPlay.Contains(f))
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

            fogUnitsInPlay.Add(f);

            f.playLightning();

            //fogCoveredTiles.Add(t);

            if (t.X == 0 || t.Z == 0 || t.X == xMax || t.Z == zMax)
            {
                borderFogUnitsInPlay.Add(f);
            }
        }
        else
        {
            Debug.Log($"Error: Cannot spawn Fog.fogUnits[{t.X}, {t.Z}]; it is already in play.");
        }
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
        FogUnit u = GetBorderFogUnit();

        if (u != null)
        {
            FogSphere s = GetFogSphere();
            GameObject o = s.gameObject;
            Vector3 pos = u.transform.position;
            pos.y = s.Height;
            o.transform.position = pos;
            o.name = "FogSphereInPlay";

            foreach (Renderer r in s.Renderers)
            {
                r.material = fogSphereVisibleMaterial;
            }

            s.SpawningTile = u.Location;
            s.Health = fogSphereMinHealth;
            s.MaxHealth = fogSphereMaxHealth;
            s.MaxSizeScale = fogSphereMaxSizeScale;
            s.FogUnitMinHealth = fogUnitMinHealth;
            s.FogUnitMaxHealth = fogUnitMaxHealth;
            s.State = FogSphereState.MovingAndGrowing;
            s.SetStartEmotion(angry);
            s.RandomiseMovementSpeed();
            s.UpdateSize();
            s.RenderOpacity();
            fogSpheresInPlay.Add(s);
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
                if (!t.FogUnitActive)
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

    //Turns on the fog's sensitivity to damage
    public void BeginUpdatingDamage()
    {
        BeginUpdatingDamage(0);
    }
    
    //Turns on the fog's sensitivity to damage
    public void BeginUpdatingDamage(float delay)
    {
        InvokeRepeating(nameof(UpdateDamageToFogUnits), delay, fogDamageInterval);
    }

    //Wakes up the fog, turning on its filling and expansion, and fog spheres
    public void WakeUpFog()
    {
        WakeUpFog(0);
    }

    //Wakes up the fog, turning on its filling and expansion, and fog spheres
    public void WakeUpFog(float delay)
    {
        InvokeRepeating(nameof(UpdateFogUnitFill), delay + 0.1f, fogFillInterval);
        InvokeRepeating(nameof(ExpandFog), delay + 0.3f, fogExpansionInterval);
        InvokeRepeating(nameof(UpdateFogSpheres), delay + 1f, fogSphereInterval);
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

            fogUnitsToReturnToPool.Clear();
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

        if (fogUnitsGrow)
        {
            if (fogAccelerates && fogGrowth < 100)
            {
                fogGrowth += fogFillInterval;
            }

            foreach (FogUnit f in fogUnitsInPlay)
            {
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

            foreach (FogUnit f in toRenderOpacity)
            {
                f.RenderOpacity();
            }
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
                    if (!a.FogUnitActive && !newTiles.Contains(a))
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
                        f.Move(fogSphereInterval * 0.5f);
                        f.Spill(fogSphereInterval * fogGrowth * 2f);
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
    }

    //Re-Pooling Methods-----------------------------------------------------------------------------------------------------------------------------

    //Puts the fog unit in the list of fog units to be put back in the pool
    public void QueueFogUnitForPooling(FogUnit f)
    {
        fogUnitsToReturnToPool.Add(f);
    }

    //TODO: double check everything in here is doing what it should be
    //Takes the fog unit off the board and puts it back in the pool
    private void ReturnFogUnitToPool(FogUnit f)
    {
        f.ActiveOnTile = false;
        f.Location.FogUnitActive = false;
        f.name = $"{f.name} (In Pool)";

        if (borderFogUnitsInPlay.Contains(f))
        {
            borderFogUnitsInPlay.Remove(f);
        }

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
        fogUnitsGrow = false;
        fogSpheresGrow = false;

        foreach (FogUnit f in fogUnitsInPlay)
        {
            f.GetComponent<Renderer>().material.SetFloat("_FPS", 0f);
        }

        Invoke(nameof(UnFreezeFog), duration);
    }

    //Handles timing out of the fog freeze
    private void UnFreezeFog()
    {
        fogUnitsGrow = true;
        fogSpheresGrow = true;

        foreach (FogUnit f in fogUnitsInPlay)
        {
            f.FogRenderer.material.SetFloat("_FPS", 16f);
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
    
    private void selectedLightning(int amount)
    {
        if (amount > 0)
        {
            FogUnit f = fogUnits[Random.Range(0, 50), Random.Range(0, 50)];
            if (fogUnitsInPlay.Contains(f))
            {
                f.playLightning();
                amount--;
            }
        }
        
    }

    private void Update()
    {
        selectedLightning(1000);
    }

    //fog lighting prefab VFX Spawn randomly ???


}
