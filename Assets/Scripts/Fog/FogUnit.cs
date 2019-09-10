using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Profiling;

public class FogUnit : Entity
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Opacity")]
    [SerializeField] private float startOpacity;
    [SerializeField] private float endOpacity;

    [Header("Colour")]
    [SerializeField] private float colourLerpSpeedMultiplier = 0.2f;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient docileColours;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient angryColours;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient currentColours;

    [Header("Damage to Fog Unit")]
    [SerializeField] private float damageLerpMultiplier = 3.0f;

    [Header("Damage to Buildings")]
    [SerializeField] private float damage;

    [Header("VFX")]
    //TODO: plug in animation get it running
    [SerializeField] private GameObject angryFogEvaporation;

    [Header("For Testing")]
    [SerializeField] private bool returnToPool;

    [Header("Prefabs")]
    [SerializeField] public GameObject fogLightning;

    //Non-Serialized Fields
    private Fog fog;
    private bool activeOnTile = false;
    private bool angry = false;

    private float colourProgress = 0;
    private float colourProgressTarget = 0;
    private bool lerpForward = true;
    private float healthProgress = 0;
    private float startHealth;
    private float targetHealth;
    private bool takingDamage = false;
    //private float damageLerpProgress = 0;

    private Renderer fogRenderer;
    private GameObject Lightning;
    private ParticleSystem LightningPS;
    private int colour;
    private int alpha;
    private MaterialPropertyBlock mat;

    private bool spill = false;
    private bool neighboursFull = false;

    private bool fillingFromFogSphere = false;
    
    //Public Properties------------------------------------------------------------------------------------------------------------------------------

    //Basic Public Properties
    public Fog Fog { get => fog; set => fog = value; }
    public bool ActiveOnTile { get => activeOnTile; set => activeOnTile = value; }
    public bool Angry { get => angry; set => angry = value; }
    public float Damage { get => damage; set => damage = value; }
    public bool FillingFromFogSphere {  get => fillingFromFogSphere; set => fillingFromFogSphere = value; }
    public Renderer FogRenderer { get => fogRenderer; set => fogRenderer = value; }
    public bool NeighboursFull { get => neighboursFull; set => neighboursFull = value; }
    public bool ReturnToPool { get => returnToPool; set => returnToPool = value; }
    public bool Spill { get => spill; set => spill = value; }
    public bool TakingDamage { get => takingDamage; }

    //Altered Public Properties
    public override float Health
    {
        get
        {
            return base.Health;
        }

        set
        {
            if (!takingDamage)
            {
                base.Health = value;

                if (base.Health >= maxHealth)
                {
                    base.Health = maxHealth;
                }
                else if (base.Health <= 0)
                {
                    ReturnToFogPool();
                }

                targetHealth = base.Health;
            }
        }
    }
    
    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    //Awake
    private void Awake()
    {
        fogRenderer = GetComponent<Renderer>();
        mat = new MaterialPropertyBlock();
        fogRenderer.GetPropertyBlock(mat);
        alpha = Shader.PropertyToID("_Alpha");
        colour = Shader.PropertyToID("_Colour");
    }

    //Sets the starting values for fog damage health variables
    private void Start()
    {
        startHealth = base.Health;
        targetHealth = base.Health;
        currentColours = docileColours;
    }

    //Fog uses this to set the starting emotion of a fog unit upon being dropped onto the board,
    //so that newly spawned fog units don't look docile when the fog is angry
    public void SetStartEmotion(bool a)
    {
        angry = a;
        currentColours = angry ? angryColours : docileColours;
    }

    //Recurring Methods - Updating Damage------------------------------------------------------------------------------------------------------------

    //Fog unit deals damage to the building on its tile
    public void DealDamageToBuilding()
    {
        if (Location.Building != null)
        {
            Location.Building.DealDamageToBuilding(damage * (base.Health / MaxHealth));
        }
    }

    //Updates the damage dealt to the fog unit
    public void UpdateDamageToFogUnit(float damageInterval)
    {
        base.Health = Mathf.Lerp(startHealth, targetHealth, healthProgress);

        if (base.Health <= targetHealth)
        {
            base.Health = targetHealth;
            takingDamage = false;
        }
        else
        {
            healthProgress += damageInterval * damageLerpMultiplier;
        }

        if (base.Health <= 0)
        {
            ReturnToFogPool();
        }
    }

    //Recurring Methods - Appearance-----------------------------------------------------------------------------------------------------------------

    //Updates the fog unit's shader colour at random between two values
    public void RenderColour()
    {
        mat.SetColor(colour, currentColours.Evaluate(Mathf.Lerp(0, 1, colourProgress)));
        //Profiler.BeginSample("FogUnit.RenderColour Mathf.Lerp(colourProgress)");
        //float lerp = Mathf.Lerp(0, 1, colourProgress);
        //Profiler.EndSample();

        //Profiler.BeginSample("FogUnit.RenderColour currentColours.Evaluate(lerp)");
        //Color evaluated = currentColours.Evaluate(lerp);
        //Profiler.EndSample();

        //Profiler.BeginSample("FogUnit.RenderColour fogRenderer.material.SetColor(colour, evaluated)");
        //fogRenderer.material.SetColor(colour, evaluated);
        //Profiler.EndSample();

        if (!angry && currentColours == angryColours || angry && currentColours == docileColours)
        {
            colourProgress -= Time.deltaTime * colourLerpSpeedMultiplier;

            if (((!angry && currentColours == angryColours) || (angry && currentColours == docileColours)) &&
                colourProgress < 0)
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

        fogRenderer.SetPropertyBlock(mat);
    }

    //Updates the fog unit's shader opacity according to its health
    public void RenderOpacity()
    {
        mat.SetFloat(alpha, Mathf.Lerp(startOpacity, endOpacity, base.Health / MaxHealth));
        fogRenderer.SetPropertyBlock(mat);
    }

    //Triggered Methods------------------------------------------------------------------------------------------------------------------------------

    //A defence has dealt damage to the fog unit
    public void DealDamageToFogUnit(float damage)
    {
        //Run angry fog evaporation effect here
        if (!fillingFromFogSphere)
        {
            takingDamage = true;
            startHealth = base.Health;
            targetHealth -= damage;
            healthProgress = 0;

            if (targetHealth < 0)
            {
                targetHealth = 0;
            }

            foreach (TileData t in Location.AdjacentTiles)
            {
                if (t.FogUnitActive)
                {
                    t.FogUnit.NeighboursFull = false;
                }
            }
        }
    }

    //Tells Fog to put the fog unit back in the pool
    private void ReturnToFogPool()
    {
        if (fog)
        {
            fog.QueueFogUnitForPooling(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //Fog Lightning Methods--------------------------------------------------------------------------------------------------------------------------

    //Removed because of new lightning pooling -Cyrus
//    private void spawnLightning()
//    {
//            if (!Lightning)
//            {
//                Lightning = Instantiate(fogLightning, transform, true);
//                LightningPS = Lightning.GetComponent<ParticleSystem>();
//            }
//    }

    //Removed because of new lightning pooling -Cyrus
//    public void playLightning()
//    {
//        if (LightningPS)
//        {
//            if (LightningPS.isStopped)
//            {
//                LightningPS.Play();
//            }
//        }
//    }

}
