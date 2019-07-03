using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FogSphereState
{
    None,
    Filling,
    Damaged,
    Full,
    Throwing,
    Attacking,
    Spilling
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

    //[Header("Movement")]
    //[SerializeField] private float heightIncrement;

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

    private Vector3 startPosition;
    private Vector3 throwTarget;
    private Vector3 attackTarget;
    private Vector3 reflectStartPosition;
    private Vector3 reflectThrowTarget;
    private Vector3 reflectAttackTarget;
    private float moveProgress = 0;
    //private float heightProgress = 0;

    //private bool spill = false;
    //private bool neighboursFull = false;

    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Properties
    public Fog Fog { get => fog; set => fog = value; }
    //public bool NeighboursFull { get => neighboursFull; set => neighboursFull = value; }
    //public bool Spill { get => spill; set => spill = value; }
    public bool Angry { get => angry; set => angry = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
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
        //Positive y targets
        startPosition = transform.position;
        Vector3 targetBuilding = CalculateTarget();

        throwTarget = targetBuilding;
        throwTarget.y = maxHeight * 7;
        attackTarget = targetBuilding;
        attackTarget.y = 0;

        //Negative y targets
        reflectStartPosition = startPosition;
        reflectStartPosition.y *= -1;
        reflectThrowTarget = throwTarget;
        reflectThrowTarget.y *= -1;
        reflectAttackTarget = attackTarget;
        reflectAttackTarget.y *= -1;

        state = FogSphereState.Throwing;
    }

    //Calculates the target of the fog sphere
    private Vector3 CalculateTarget()
    {
        return GameObject.Find("Hub").transform.position;
    }

    //Change so it moves in a vertical quater circle from the base of the circle to one side.
    public void Move(float increment)
    {
        moveProgress += increment;
        Vector3 pos = Vector3.Lerp(startPosition, throwTarget, moveProgress);
        pos.y = transform.position.y * 1.05f;
        transform.position = pos;

        if (moveProgress >= 1)
        {
            moveProgress = 0;
            state = FogSphereState.Attacking;
        }
    }

    //Move up a bit, then drop down (parabolic)? Or keep as straight drop down?
    public void Attack(float increment)
    {
        moveProgress += increment;
        transform.position = Vector3.Lerp(throwTarget, attackTarget, moveProgress);

        if (moveProgress >= 1)
        {
            moveProgress = 0;
            state = FogSphereState.Spilling;
        }
    }

    public void Spill()
    {
        ReturnToFogPool();
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
}
