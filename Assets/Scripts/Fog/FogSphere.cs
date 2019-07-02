//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FogSphere : MonoBehaviour
//{
//    //Serialized Fields
//    // [SerializeField] private float damage = 0.1f;
//    [SerializeField] private float lerpToMaxInterval;
//    [SerializeField] private float rapidLerpMultiplier = 3f;
//    [SerializeField] private float damage = 0.1f;
//    [SerializeField] private float startOpacity = 0f;
//    [SerializeField] private float endOpacity = 0.90f;

//    [SerializeField] [GradientUsageAttribute(true)] private Gradient docileColours;
//    [SerializeField] [GradientUsageAttribute(true)] private Gradient angryColours;
//    [SerializeField] [GradientUsageAttribute(true)] private Gradient currentColours;
//    [SerializeField] private float colourLerpSpeedMultiplier = 1f;

//    [SerializeField] private GameObject angryFogEvaporation;

//    [SerializeField] private float health = 1f;
//    [SerializeField] private float maxHealth = 1f;

//    //Non-Serialized Fields
//    private Fog fog;
//    private bool angry = false;

//    private float colourProgress = 0;
//    private float colourProgressTarget = 0;
//    private bool lerpForward = true;
//    private float healthProgress = 0;
//    private float startHealth;
//    private float targetHealth;
//    private bool takingDamage = false;
//    private float damageLerpProgress = 0;

//    private Renderer fogRenderer;
//    private int colour;
//    private int alpha;

//    //private bool spill = false;
//    //private bool neighboursFull = false;

//    //Public Properties
//    public Fog Fog { get => fog; set => fog = value; }
//    //public bool NeighboursFull { get => neighboursFull; set => neighboursFull = value; }
//    //public bool Spill { get => spill; set => spill = value; }
//    public bool Angry { get => angry; set => angry = value; }

//    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
//    public bool TakingDamage { get => takingDamage; }

//    //Altered Public Properties
//    public float Health
//    {
//        get
//        {
//            return health;
//        }

//        set
//        {
//            if (!takingDamage)
//            {
//                health = value;

//                if (health >= maxHealth)
//                {
//                    health = maxHealth;
//                }
//                else if (health <= 0)
//                {
//                    ReturnToFogPool();
//                }

//                targetHealth = health;
//            }
//        }
//    }
//    protected bool GotNoHealth()
//    {
//        if (health <= 0)
//        {
//            return true;
//        }

//        return false;
//    }

//    //Awake
//    private void Awake()
//    {
//        fogRenderer = gameObject.GetComponent<Renderer>();
//        colour = Shader.PropertyToID("_Colour");
//        alpha = Shader.PropertyToID("_Alpha");
//    }

//    //Sets the starting values for fog damage health variables
//    private void Start()
//    {
//        startHealth = health;
//        targetHealth = health;
//        currentColours = docileColours;
//    }

//    //Fog uses this to set the starting emotion of a fog unit upon being dropped onto the board,
//    //so that newly spawned fog units don't look docile when the fog is angry.
//    public void SetStartEmotion(bool a)
//    {
//        angry = a;

//        currentColours = angry ? angryColours : docileColours;
//    }

//    ////Verifies that (x, z) is the fog unit's position
//    //public bool PositionIs(int x, int z)
//    //{
//    //    return transform.position.x == x && transform.position.z == z;
//    //}

//    //A defence has dealt damage to the fog unit
//    public void DealDamageToFogSphere(float damage)
//    {
//        //Run angry fog evaporation effect here

//        takingDamage = true;

//        startHealth = health;
//        targetHealth -= damage;
//        healthProgress = 0;

//        if (targetHealth < 0)
//        {
//            targetHealth = 0;
//        }
//    }

//    //Updates the damage dealt to the fog unit
//    public void UpdateDamageToFogSphere(float damageInterval)
//    {
//        health = Mathf.Lerp(startHealth, targetHealth, healthProgress);

//        if (health <= targetHealth)
//        {
//            health = targetHealth;
//            takingDamage = false;
//        }
//        else
//        {
//            healthProgress += damageInterval * rapidLerpMultiplier;
//        }

//        if (health <= 0)
//        {
//            ReturnToFogPool();
//        }
//    }

//    //Updates the fog unit's shader colour at random between two values
//    public void RenderColour()
//    {
//        fogRenderer.material.SetColor(colour, currentColours.Evaluate(Mathf.Lerp(0, 1, colourProgress)));

//        if (!angry && currentColours == angryColours || angry && currentColours == docileColours)
//        {
//            colourProgress -= Time.deltaTime * colourLerpSpeedMultiplier;

//            if (((!angry && currentColours == angryColours) || (angry && currentColours == docileColours)) && colourProgress < 0)
//            {
//                colourProgress = 0;
//                colourProgressTarget = 0;

//                currentColours = angry ? angryColours : docileColours;
//            }
//        }
//        else
//        {
//            if (colourProgress == colourProgressTarget)
//            {
//                colourProgressTarget = Random.Range(0f, 1f);

//                lerpForward = colourProgressTarget > colourProgress;
//            }

//            if (lerpForward)
//            {
//                colourProgress += Time.deltaTime * colourLerpSpeedMultiplier;

//                if (colourProgress > colourProgressTarget)
//                {
//                    colourProgress = colourProgressTarget;
//                }
//            }
//            else
//            {
//                colourProgress -= Time.deltaTime * colourLerpSpeedMultiplier;

//                if (colourProgress < colourProgressTarget)
//                {
//                    colourProgress = colourProgressTarget;
//                }
//            }
//        }
//    }

//    //Updates the fog unit's shader opacity according to its health
//    public void RenderOpacity()
//    {
//        fogRenderer.material.SetFloat(alpha, Mathf.Lerp(startOpacity, endOpacity, health / MaxHealth));
//    }

//    //Tells Fog to put the fog unit back in the pool
//    private void ReturnToFogPool()
//    {
//        if (fog)
//        {
//            fog.QueueFogSphereForPooling(this);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }
//}
