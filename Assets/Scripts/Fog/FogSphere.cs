using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Schema;
using Cinemachine;
using UnityEngine;

public enum FogSphereState
{
    None,
    Damaged,
    MovingAndGrowing,
    Spilling,
    Attacking
}

public class FogSphere : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Fog Sphere State")]
    [SerializeField] private FogSphereState state;
    
    [Header("Health")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private float damageLerpMultiplier;

    [Header("Movement")]
    //[SerializeField] private float minHeight;
    [SerializeField] private float height;
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;
    [SerializeField] private float minSpeedWhileSpilling;
    [SerializeField] private float acceleration;

    [Header("Spilling")]
    [SerializeField] private int maxSpiltFogCount;

    [Header("Damage")]
    [SerializeField] private float damage;

    [Header("Renderers")]
    [SerializeField] private List<Renderer> renderers;

    [Header("Opacity")]
    [SerializeField] private float startOpacity;
    [SerializeField] private float endOpacity;

    [Header("Colour")]
    [SerializeField] private float colourLerpSpeedMultiplier;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient docileColours;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient angryColours;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient currentColours;

    [Header("Size")]
    [SerializeField] private float minSizeScale;
    [SerializeField] private float maxSizeScale;

    //Non-Serialized Fields
    private Fog fog;
    private bool angry = false;

    private int colour;
    private int alpha;

    private float colourProgress = 0;
    private float colourProgressTarget = 0;
    private bool lerpForward = true;

    private float healthProgress = 0;
    private float startHealth;
    private float targetHealth;
    private float damageLerpProgress = 0;

    private TileData spawningTile;
    private Vector3 startPosition;
    private Vector3 hubPosition;

    private float normalMovementSpeed;

    [Header("Testing")]
    [SerializeField] private float currentMovementSpeed;

    private List<FogUnit> spiltFog = new List<FogUnit>();
    private float fogUnitMinHealth;
    private float fogUnitMaxHealth;
    private bool canSpillFurther = true;
    private Hub hub;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public Fog Fog { get => fog; set => fog = value; }
    public bool Angry { get => angry; set => angry = value; }
    public float FogUnitMaxHealth {  get => fogUnitMaxHealth; set => fogUnitMaxHealth = value; }
    public float FogUnitMinHealth {  get => fogUnitMinHealth; set => fogUnitMinHealth = value; }
    public float Height { get => height; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float MaxSizeScale { get => maxSizeScale; set => maxSizeScale = value; }
    public float MinSizeScale { get => minSizeScale; set => minSizeScale = value; }
    public List<Renderer> Renderers {  get => renderers; }
    public TileData SpawningTile { get => spawningTile; set => spawningTile = value; }
    public List<FogUnit> SpiltFog { get => spiltFog; set => spiltFog = value; }
    public FogSphereState State { get => state; set => state = value; }

    //Altered Public Properties
    public float Health
    {
        get
        {
            return health;
        }

        set
        {
            if (state != FogSphereState.Damaged)
            {
                health = value;

                if (health >= maxHealth)
                {
                    health = maxHealth;
                }
                else if (health <= 0)
                {
                    ReturnToFogPool();
                }

                targetHealth = health;
            }
        }
    }

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    //Awake
    private void Awake()
    {
        colour = Shader.PropertyToID("_Colour");
        alpha = Shader.PropertyToID("_Alpha");
        hub = GameObject.Find("Hub").GetComponent<Hub>();
        hubPosition = hub.transform.position;
        hubPosition.y = height;
    }

    //Sets the starting values for fog damage health variables
    private void Start()
    {
        startHealth = health;
        targetHealth = health;
        currentColours = docileColours;
    }

    //Fog uses this to set the starting emotion of a fog unit upon being dropped onto the board,
    //so that newly spawned fog units don't look docile when the fog is angry
    public void SetStartEmotion(bool a)
    {
        angry = a;
        currentColours = angry ? angryColours : docileColours;
    }

    public void RandomiseMovementSpeed()
    {
        normalMovementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);
        currentMovementSpeed = normalMovementSpeed;
    }

    //Recurring Methods - MovingAndGrowing and Growing---------------------------------------------------------------------------------------------------------

    //Updates how much health the fog unit has
    public void Grow(float increment)
    {
        if (health < maxHealth)
        {
            Health += increment;
            UpdateSize();
            RenderOpacity();
        }
    }

    //Moves the fog sphere towards the hub
    public void Move(float interval)
    {
        if (state == FogSphereState.Spilling)
        {
            if (currentMovementSpeed > minSpeedWhileSpilling)
            {
                currentMovementSpeed = Mathf.Max(minSpeedWhileSpilling, currentMovementSpeed - acceleration);
            }
        }
        else if (state == FogSphereState.MovingAndGrowing)
        {
            if (currentMovementSpeed < normalMovementSpeed)
            {
                currentMovementSpeed = Mathf.Min(normalMovementSpeed, currentMovementSpeed + acceleration);
            }
        }

        hubPosition.y = transform.position.y;       //Ensures rate of movement accounts only for orthogonal movement; vertical movement is handled by UpdateSize()
        transform.position = Vector3.MoveTowards(transform.position, hubPosition, currentMovementSpeed * interval);
        
        if (transform.position == hubPosition)
        {
            state = FogSphereState.Attacking;
        }
        else if (CheckFogToFill())    
        {
            GetFogToFill();
            state = FogSphereState.Spilling;
            canSpillFurther = true;
        }
    }

    //Checks that there are tiles within range that the fog sphere can spill into
    private bool CheckFogToFill()
    {
        WorldController wc = WorldController.Instance;
        float radius = renderers[0].bounds.extents.magnitude * 0.5f;

        int iMin = Mathf.RoundToInt(Mathf.Max(0, transform.position.x - radius));
        int jMin = Mathf.RoundToInt(Mathf.Max(0, transform.position.z - radius));
        float iMax = Mathf.Min(fog.XMax, transform.position.x + radius);
        float jMax = Mathf.Min(fog.ZMax, transform.position.z + radius);

        if (iMin > iMax || jMin > jMax) //Tiny radius --> effective search area of 0 if using loops.
        {
            if (wc.TileExistsAt(transform.position))
            {
                TileData t = wc.GetTileAt(transform.position);

                if (!t.FogUnitActive || t.FogUnit.Health < t.FogUnit.MaxHealth * 0.5f)
                {
                    return true;
                }
            }
        }
        else
        { 
            for (int i = iMin; i < iMax; i++)
            {
                for (int j = jMin; j < jMax; j++)
                {
                    if (wc.TileExistsAt(i, j))
                    {
                        TileData t = wc.GetTileAt(i, j);

                        if ((!t.FogUnitActive || t.FogUnit.Health < t.FogUnit.MaxHealth * 0.5f) && Vector3.Distance(transform.position, new Vector3(i, transform.position.y, j)) < radius)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    //Gets the tiles within range that the fog sphere can spill into
    private void GetFogToFill()
    {
        WorldController wc = WorldController.Instance;

        if (wc.TileExistsAt(transform.position))
        {
            TileData c = wc.GetTileAt(transform.position);

            float radius = renderers[0].bounds.extents.magnitude * 0.5f;

            int iMin = Mathf.RoundToInt(Mathf.Max(0, transform.position.x - radius));
            int jMin = Mathf.RoundToInt(Mathf.Max(0, transform.position.z - radius));
            float iMax = Mathf.Min(fog.XMax, transform.position.x + radius);
            float jMax = Mathf.Min(fog.ZMax, transform.position.z + radius);

            if (iMin > iMax || jMin > jMax) //Tiny radius --> effective search area of 0 if using loops.
            {
                if (wc.TileExistsAt(transform.position))
                {
                    TileData t = wc.GetTileAt(transform.position);

                    if (!t.FogUnitActive || t.FogUnit.Health < t.FogUnit.MaxHealth)
                    {
                        if (!t.FogUnitActive)
                        {
                            fog.SpawnFogUnitWithMinHealthOnTile(t);
                        }

                        spiltFog.Add(t.FogUnit);
                    }
                }
            }
            else
            {
                for (int i = iMin; i < iMax; i++)
                {
                    for (int j = jMin; j < jMax; j++)
                    {
                        if (wc.TileExistsAt(i, j))
                        {
                            TileData t = wc.GetTileAt(i, j);

                            if ((!t.FogUnitActive || t.FogUnit.Health < t.FogUnit.MaxHealth) && Vector3.Distance(transform.position, new Vector3(i, transform.position.y, j)) < radius)
                            {
                                if (!t.FogUnitActive)
                                {
                                    fog.SpawnFogUnitWithMinHealthOnTile(t);
                                }

                                spiltFog.Add(t.FogUnit);
                            }
                        }
                    }
                }
            }
        }

        if (spiltFog.Count == 0)
        {
            Debug.Log("FogSphere.GetFogToFill was called, but didn't find any tiles even though FogSphere.CheckFogToFill found some.");
        }
    }

    //Recurring Methods - Spilling-------------------------------------------------------------------------------------------------------------------
    
    //Fog sphere spills into fog tiles that it finds.
    public void Spill(float increment)
    {
        bool readyToSpillFurther = true;
        List<FogUnit> full = new List<FogUnit>();

        Health -= increment;
        UpdateSize();
        RenderColour();
        RenderOpacity();

        foreach (FogUnit f in spiltFog)
        {
            f.Health += increment * (maxSpiltFogCount / spiltFog.Count);
            f.RenderColour();
            f.RenderOpacity();

            if (f.Health < f.MaxHealth * 0.5f)
            {
                readyToSpillFurther = false;
            }
            else if (f.Health >= f.MaxHealth)
            {
                full.Add(f);
            }
        }

        if (canSpillFurther && readyToSpillFurther)
        {
            canSpillFurther = GetMoreFogToFill();
        }

        foreach (FogUnit f in full)
        {
            spiltFog.Remove(f);
        }

        if (spiltFog.Count == 0)
        {
            state = FogSphereState.MovingAndGrowing;
        }
    }

    //Fog has reached a spilling threshold, spills over into even more tiles
    private bool GetMoreFogToFill()
    {
        List<FogUnit> newFog = new List<FogUnit>();

        foreach(FogUnit f in spiltFog)
        {
            foreach (TileData t in f.Location.AdjacentTiles)
            {
                if (!t.FogUnitActive || (t.FogUnit.Health < t.FogUnit.MaxHealth && !newFog.Contains(t.FogUnit) && !spiltFog.Contains(t.FogUnit)))
                {
                    if (!t.FogUnitActive)
                    {
                        fog.SpawnFogUnitWithMinHealthOnTile(t);
                    }
                    
                    newFog.Add(t.FogUnit);
                }
            }
        }

        spiltFog.AddRange(newFog);
        return newFog.Count > 0;
    }

    //Recurring Methods - Attacking------------------------------------------------------------------------------------------------------------------

    //Attacks the hub
    public void Attack(float increment)
    {
        Health -= increment * 5;

        if (fog.DamageOn && !WorldController.Instance.GameOver)
        {
            hub.DealDamageToBuilding(damage * increment);
        }

        if (health <= 0)
        {
            ReturnToFogPool();
        }
        else
        {
            UpdateSize();
            RenderColour();
            RenderOpacity();
        }
    }

    //Recurring Methods - Taking Damage--------------------------------------------------------------------------------------------------------------

    //Updates the damage dealt to the fog unit
    public void UpdateDamageToFogSphere(float damageInterval)
    {
        health = Mathf.Lerp(startHealth, targetHealth, healthProgress);

        if (health <= targetHealth)
        {
            health = targetHealth;
            state = FogSphereState.MovingAndGrowing;
        }
        else
        {
            healthProgress += damageInterval * damageLerpMultiplier;
        }

        if (health <= 0)
        {
            ReturnToFogPool();
        }
        else
        {
            UpdateSize();
            RenderColour();
            RenderOpacity();
        }
    }

    //Triggered/Utility Methods - Appearance---------------------------------------------------------------------------------------------------------
    
    //Updates the fog unit's shader colour at random between two values
    public void RenderColour()
    {
        foreach (Renderer r in renderers)
        {
            r.material.SetColor(colour, currentColours.Evaluate(Mathf.Lerp(0, 1, colourProgress)));
        }

        if (!angry && currentColours == angryColours || angry && currentColours == docileColours)
        {
            colourProgress -= Time.deltaTime * colourLerpSpeedMultiplier;

            if (((!angry && currentColours == angryColours) || (angry && currentColours == docileColours)) && colourProgress < 0)
            {
                colourProgress = 0;
                colourProgressTarget = 0;

                currentColours = angry ? angryColours : docileColours;
            }
        }
        else
        {
            if (colourProgress == colourProgressTarget)
            {
                colourProgressTarget = Random.Range(0f, 1f);

                lerpForward = colourProgressTarget > colourProgress;
            }

            if (lerpForward)
            {
                colourProgress += Time.deltaTime * colourLerpSpeedMultiplier;

                if (colourProgress > colourProgressTarget)
                {
                    colourProgress = colourProgressTarget;
                }
            }
            else
            {
                colourProgress -= Time.deltaTime * colourLerpSpeedMultiplier;

                if (colourProgress < colourProgressTarget)
                {
                    colourProgress = colourProgressTarget;
                }
            }
        }
    }

    //Updates the fog unit's shader opacity according to its health
    public void RenderOpacity()
    {
        foreach (Renderer r in renderers)
        {
            r.material.SetFloat(alpha, Mathf.Lerp(startOpacity, endOpacity, health / maxHealth));
        }
    }

    //Lerps size according to health/maxHealth; also maintains height.
    public void UpdateSize()
    {
        float scale = Mathf.Lerp(minSizeScale, maxSizeScale, Mathf.Min(health / maxHealth, 1));
        transform.localScale = new Vector3(scale, scale, scale);
    }

    //Triggered/Utility Methods - Other--------------------------------------------------------------------------------------------------------------

    //A defence has dealt damage to the fog unit
    public void DealDamageToFogSphere(float damage)
    {
        state = FogSphereState.Damaged;

        startHealth = health;
        targetHealth -= damage;
        healthProgress = 0;

        if (targetHealth < 0)
        {
            targetHealth = 0;
        }
    }

    //Tells Fog to put the fog unit back in the pool
    private void ReturnToFogPool()
    {
        if (fog)
        {
            fog.QueueFogSphereForPooling(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
