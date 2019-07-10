using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum FogSphereState
{
    None,
    Filling,
    Damaged,
    Full,
    Throwing,
    //Attacking,
    Spilling
}

public class FogSphere : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Fog Sphere State")]
    [SerializeField] private FogSphereState state;

    [Header("Health")]
    [SerializeField] private float health = 1f;
    [SerializeField] private float maxHealth = 1f;
    [SerializeField] private float damageLerpMultiplier = 3f;

    [Header("Movement")]
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;

    [Header("Opacity")]
    [SerializeField] private float startOpacity = 0f;
    [SerializeField] private float endOpacity = 0.90f;

    [Header("Colour")]
    [SerializeField] private float colourLerpSpeedMultiplier = 1f;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient docileColours;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient angryColours;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient currentColours;

    //Non-Serialized Fields
    private Fog fog;
    private bool angry = false;

    private Renderer fogRenderer;
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
    private Vector3 hubPos;
    //private Vector3 attackTarget;
    private Vector3 spillTarget;
    private float moveProgress = 0;

    [Header("Exposed During Testing")]
    [SerializeField] private float movementSpeed;

    private List<FogUnit> spiltFog = new List<FogUnit>();
    private float fogUnitMinHealth;
    private float fogUnitMaxHealth;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Properties
    public Fog Fog { get => fog; set => fog = value; }
    public bool Angry { get => angry; set => angry = value; }
    public Renderer FogRenderer {  get => fogRenderer; }
    public float FogUnitMaxHealth {  get => fogUnitMaxHealth; set => fogUnitMaxHealth = value; }
    public float FogUnitMinHealth {  get => fogUnitMinHealth; set => fogUnitMinHealth = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public List<FogUnit> SpiltFog { get => spiltFog; set => spiltFog = value; }
    public FogSphereState State { get => state; set => state = value; }
    public TileData SpawningTile { get => spawningTile; set => spawningTile = value; }

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
                    state = FogSphereState.Full;
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
        fogRenderer = gameObject.GetComponentInChildren<Renderer>();
        colour = Shader.PropertyToID("_Colour");
        alpha = Shader.PropertyToID("_Alpha");
    }

    //Sets the starting values for fog damage health variables
    private void Start()
    {
        startHealth = health;
        targetHealth = health;
        currentColours = docileColours;
    }

    //Fog uses this to set the starting emotion of a fog unit upon being dropped onto the board,
    //so that newly spawned fog units don't look docile when the fog is angry.
    public void SetStartEmotion(bool a)
    {
        angry = a;
        currentColours = angry ? angryColours : docileColours;
    }

    public void RandomiseMovementSpeed()
    {
        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);
    }

    //Recurring Methods - Movement and Spill---------------------------------------------------------------------------------------------------------
    
    //Lerps height according to health/maxHealth
    public void UpdateHeight()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(minHeight, maxHeight, Mathf.Min(health / maxHealth, 1));
        transform.position = pos;
    }

    // Sets the fog sphere moving towards the target position
    public void Throw()
    {
        startPosition = transform.position;
        hubPos = GameObject.Find("Hub").transform.position;
        spillTarget = hubPos;
        spillTarget.y = minHeight;
        state = FogSphereState.Throwing;
    }

    //Change so it moves in a vertical quarter circle from the base of the circle to one side.
    public void Move(float interval)
    {
        transform.position = Vector3.MoveTowards(transform.position, hubPos, movementSpeed * interval);

        if (transform.position == hubPos)
        {
            moveProgress = 0;
            state = FogSphereState.Spilling;
            hubPos = transform.position;  //Accounts for overshoot

            if (WorldController.Instance.TileExistsAt(transform.position))
            {
                spiltFog = new List<FogUnit>();
                TileData t = WorldController.Instance.GetTileAt(transform.position);

                Fog.Instance.SpawnFogUnitWithMinHealth(t);
                spiltFog.Add(t.FogUnit);
                t.FogUnit.FillingFromFogSphere = true;

                foreach (TileData a in t.AdjacentTiles)
                {
                    if (a.FogUnit == null)
                    {
                        Fog.Instance.SpawnFogUnitWithMinHealth(a);
                    }

                    spiltFog.Add(a.FogUnit);
                    a.FogUnit.FillingFromFogSphere = true;
                }
            }
        }
    }

    //Fog sphere spills into fog tiles that it finds.
    public void Spill(float increment)
    {
        moveProgress += increment;
        transform.position = Vector3.Lerp(hubPos, spillTarget, moveProgress);

        foreach (FogUnit f in spiltFog)
        {
            f.Health = Mathf.Max(f.Health, Mathf.Lerp(fogUnitMinHealth, fogUnitMaxHealth, moveProgress));
            f.RenderColour();
            f.RenderOpacity();
        }

        if (moveProgress >= 1)
        {
            ReturnToFogPool();
        }
    }

    //Recurring Methods - Health and Appearance------------------------------------------------------------------------------------------------------

    //Updates how much health the fog unit has
    public void UpdateFill(float increment)
    {
        Health += increment * GetFillMultiplier();
        UpdateHeight();
        RenderOpacity();
    }

    //Returns +1 or -1 depending on how much fog is available to fuel the fog sphere.
    private int GetFillMultiplier()
    {
        //Shrivels if the spawning tile's fog unit hasn't spilt; one fog unit isn't enough.
        if (spawningTile.FogUnit == null || !spawningTile.FogUnit.Spill)
        {
            return -1;
        }

        //Shrivels if any of the adjacent tile's FogUnits aren't present or strong enough.
        foreach (TileData t in spawningTile.AdjacentTiles)
        {
            if (t.FogUnit == null || t.FogUnit.Health < t.FogUnit.MaxHealth * 0.33)
            {
                return -1;
            }
        }

        return 1;
    }

    //Updates the damage dealt to the fog unit
    public void UpdateDamageToFogSphere(float damageInterval)
    {
        health = Mathf.Lerp(startHealth, targetHealth, healthProgress);

        if (health <= targetHealth)
        {
            health = targetHealth;
            state = FogSphereState.Filling;
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
            UpdateHeight();
            RenderOpacity();
        }
    }

    //Updates the fog unit's shader colour at random between two values
    public void RenderColour()
    {
        fogRenderer.material.SetColor(colour, currentColours.Evaluate(Mathf.Lerp(0, 1, colourProgress)));

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
        fogRenderer.material.SetFloat(alpha, Mathf.Lerp(startOpacity, endOpacity, health / MaxHealth));
    }

    //Triggered/Utility Methods----------------------------------------------------------------------------------------------------------------------

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
