using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FogSphereState
{
    None,
    Filling,
    Damaged,
    Full,
    Throwing
}

public class FogSphere : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    //[SerializeField] private float damage = 0.1f;
    //[SerializeField] private float lerpToMaxInterval;
    //[SerializeField] private float damage = 0.1f;

    [Header("Fog Sphere State")]
    [SerializeField] private FogSphereState state;
    
    [Header("Health")]
    [SerializeField] private float health = 1f;
    [SerializeField] private float maxHealth = 1f;
    [SerializeField] private float damageLerpMultiplier = 3f;

    [Header("Height")]
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;

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

    private float colourProgress = 0;
    private float colourProgressTarget = 0;
    private bool lerpForward = true;
    private float healthProgress = 0;
    private float startHealth;
    private float targetHealth;
    private float damageLerpProgress = 0;

    private Renderer fogRenderer;
    private int colour;
    private int alpha;

    //private bool spill = false;
    //private bool neighboursFull = false;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Properties
    public Fog Fog { get => fog; set => fog = value; }
    //public bool NeighboursFull { get => neighboursFull; set => neighboursFull = value; }
    //public bool Spill { get => spill; set => spill = value; }
    public bool Angry { get => angry; set => angry = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float MinHeight { get => minHeight; }
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
                    //ReturnToFogPool();
                }
                else if (health <= 0)
                {
                    ReturnToFogPool();
                }

                targetHealth = health;
            }
        }
    }

    //protected bool GotNoHealth()
    //{
    //    return health <= 0;
    //}

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

    //Recurring Methods - Movement and Spill---------------------------------------------------------------------------------------------------------

    //Lerps height according to health/maxHealth
    public void UpdateHeight()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(minHeight, maxHeight, Mathf.Min(health / maxHealth, 1));
        transform.position = pos;

        if (transform.position.y == maxHeight)
        {
            state = FogSphereState.Full;
        }
    }

    //Recurring Methods - Health and Appearance------------------------------------------------------------------------------------------------------
    
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

    //Fire() and CalculateVelocity() borrowed from Projectile.cs and then adapted to the fog sphere's needs

    // Fires the projectile from the origin to the target, with the given damage
    public void Fire()
    {
        Rigidbody r = gameObject.GetComponent<Rigidbody>();
        Vector3 vel = CalculateVelocity(CalculateTarget(), transform.position, 1f);
        r.velocity = vel;
        r.useGravity = true;
        transform.rotation = Quaternion.LookRotation(vel);
        state = FogSphereState.Throwing;
    }

    //Calculates the target of the fog sphere
    private Vector3 CalculateTarget()
    {
        return GameObject.Find("Hub").transform.position;
    }

    // Calculates the velocity for an arching projectile
    // Taken from: https://www.youtube.com/watch?v=03GHtGyEHas
    private Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        // Define the distance x and y first
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = minHeight;

        // Create a float to represent our distance
        float Sy = distance.y;
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }
}
