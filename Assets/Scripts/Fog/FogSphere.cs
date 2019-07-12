using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Cinemachine;
using UnityEngine;

public enum FogSphereState
{
    None,
    Damaged,
    MovingAndGrowing,
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

    [Header("Spilling")]
    [SerializeField] private int maxSpiltFogCount;

    [Header("Size")]
    [SerializeField] private float minSizeScale;
    [SerializeField] private float maxSizeScale;

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
    private Vector3 hubPosition;

    //[Header("Exposed During Testing")]
    /*[SerializeField]*/ private float movementSpeed;

    private List<List<FogUnit>> spiltFog = new List<List<FogUnit>>() {new List<FogUnit>()};
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
    public TileData SpawningTile { get => spawningTile; set => spawningTile = value; }
    public List<List<FogUnit>> SpiltFog { get => spiltFog; set => spiltFog = value; }
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
        fogRenderer = gameObject.GetComponentInChildren<Renderer>();
        colour = Shader.PropertyToID("_Colour");
        alpha = Shader.PropertyToID("_Alpha");
        hubPosition = GameObject.Find("Hub").transform.position;
        hubPosition.y = maxHeight;
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

    //Recurring Methods - MovingAndGrowing and Growing---------------------------------------------------------------------------------------------------------

    //Updates how much health the fog unit has
    public void Grow(float increment)
    {
        if (health < maxHealth)
        {
            Health += increment;
            UpdateHeight();
            RenderOpacity();
        }
    }

    //Change so it moves in a vertical quarter circle from the base of the circle to one side.
    public void Move(float interval)
    {
        hubPosition.y = transform.position.y;       //Ensures rate of movement accounts only for orthogonal movement; vertical movement is handled by UpdateHeight()
        transform.position = Vector3.MoveTowards(transform.position, hubPosition, movementSpeed * interval);

        if (transform.position == hubPosition)
        {
            state = FogSphereState.Spilling;

            if (WorldController.Instance.TileExistsAt(transform.position))
            {
                spiltFog = new List<List<FogUnit>>() {new List<FogUnit>()};
                TileData t = WorldController.Instance.GetTileAt(transform.position);

                Fog.Instance.SpawnFogUnitWithMinHealth(t);
                spiltFog[0].Add(t.FogUnit);
                t.FogUnit.FillingFromFogSphere = true;

                foreach (TileData a in t.AdjacentTiles)
                {
                    if (a.FogUnit == null)
                    {
                        Fog.Instance.SpawnFogUnitWithMinHealth(a);
                    }

                    spiltFog[0].Add(a.FogUnit);
                    a.FogUnit.FillingFromFogSphere = true;
                }
            }
        }
        else if (CheckTilesToFill())    //have this method check if not full tiles are under the fog sphere, and another method get those tiles.
        {
            GetTilesToFill();
            state = FogSphereState.Spilling;
        }
    }

    private bool CheckTilesToFill()
    {
        WorldController wc = WorldController.Instance;
        float radius = fogRenderer.bounds.extents.magnitude * 0.5f;

        for (int i = Mathf.RoundToInt(Mathf.Max(0, transform.position.x - radius)); i < Mathf.Min(fog.XMax, transform.position.x + radius); i++)
        {
            for (int j = Mathf.RoundToInt(Mathf.Max(0, transform.position.z - radius)); j < Mathf.Min(fog.ZMax, transform.position.z + radius); j++)
            {
                if (wc.TileExistsAt(i, j))
                {
                    TileData t = wc.GetTileAt(i, j);

                    if ((t.FogUnit == null || t.FogUnit.Health < t.FogUnit.MaxHealth * 0.5f) && Vector3.Distance(transform.position, new Vector3(i, transform.position.y, j)) < radius)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void GetTilesToFill()
    {
        bool EmptyTilesDoesNotContain(List<List<TileData>> list, TileData tile)
        {
            foreach (List<TileData> l in list)
            {
                if (l.Contains(tile))
                {
                    return false;
                }
            }

            return true;
        }

        WorldController wc = WorldController.Instance;

        if (wc.TileExistsAt(transform.position))
        {
            TileData c = wc.GetTileAt(transform.position);
            List<List<TileData>> emptyTiles = new List<List<TileData>> {new List<TileData>()};

            //Find all empty tiles in range
            //List<TileData> tilesInRadius = new List<TileData>(); // t.CollectTilesInRange(t.X, t.Z, Mathf.RoundToInt(transform.localScale.x));

            float radius = fogRenderer.bounds.extents.magnitude * 0.5f;
            int count = 0;

            for (int i = Mathf.RoundToInt(Mathf.Max(0, transform.position.x - radius)); i < Mathf.Min(fog.XMax, transform.position.x + radius); i++)
            {
                for (int j = Mathf.RoundToInt(Mathf.Max(0, transform.position.z - radius)); j < Mathf.Min(fog.ZMax, transform.position.z + radius); j++)
                {
                    if (wc.TileExistsAt(i, j))
                    {
                        TileData t = wc.GetTileAt(i, j);

                        if ((t.FogUnit == null || t.FogUnit.Health < t.FogUnit.MaxHealth) && Vector3.Distance(transform.position, new Vector3(i, transform.position.y, j)) < radius)
                        {
                            if (t.FogUnit == null)
                            {
                                fog.SpawnFogUnitWithMinHealth(t);
                            }

                            emptyTiles[0].Add(t);
                            spiltFog[0].Add(t.FogUnit);
                            //t.FogUnit.Distance = 0;
                            count++;
                        }
                    }
                }
            }

            ////Find all empty tiles in range
            //foreach (TileData a in tilesInRadius)
            //{
            //    if (a.FogUnit == null || a.FogUnit.Health < fogUnitMaxHealth)
            //    {
            //        emptyTiles.Add(a);
            //    }
            //}
            
            //If empty tiles in range 
            if (count > 0)
            {
                //If haven't maxed out on empty tiles already
                if (count < maxSpiltFogCount)
                {
                    int distance = 1;
                    List<TileData> edgeTiles = new List<TileData>(emptyTiles[0]);
                    bool finished;

                    do
                    {
                        emptyTiles.Add(new List<TileData>());
                        spiltFog.Add(new List<FogUnit>());

                        //For each edge tile, look for new empty tiles beyond the current edge tiles
                        foreach (TileData e in edgeTiles)
                        {
                            foreach (TileData a in e.AdjacentTiles)
                            {
                                if ((a.FogUnit == null || a.FogUnit.Health < fogUnitMaxHealth) && EmptyTilesDoesNotContain(emptyTiles, a))
                                {
                                    if (a.FogUnit == null)
                                    {
                                        fog.SpawnFogUnitWithMinHealth(a);
                                    }

                                    emptyTiles[0].Add(a);
                                    spiltFog[0].Add(a.FogUnit);
                                    count++;
                                }
                            }
                        }

                        //If found new tiles
                        if (emptyTiles[distance].Count > 0)
                        {
                            edgeTiles = emptyTiles[distance];
                            finished = count >= maxSpiltFogCount;
                            distance++;
                        }
                        else    //No more not-full stuff to spill into
                        {
                            finished = true;
                        }
                    } while (!finished);
                }

                //TODO: order by distance from fog sphere so that it flows from closest to farthest when spilling out, rather than filling everything uniformly.
                //foreach (TileData e in emptyTiles)
                //{
                //    fog.SpawnFogUnitWithMinHealth(e);
                //    spiltFog.Add(e.FogUnit);
                //    e.FogUnit.Distance = Vector3.Distance(transform.position, e.FogUnit.transform.position);
                //}

                //spiltFog.Sort(new FogUnitDistanceComparison());

                //foreach (FogUnit f in spiltFog)
                //{
                //    Debug.Log($"{f.name}.Distance: {f.Distance}");
                //}
            }
        }

        if (spiltFog.Count == 0)
        {
            Debug.Log("FogSphere.GetTilesToFill was called, but didn't find any tiles even though FogSphere.CheckTilesToFill found some.");
        }
    }

    //Recurring Methods - Spilling-------------------------------------------------------------------------------------------------------------------
    
    //TODO: have fog filling flow from closest fog unit to farthest fog unit when spilling out, rather than filling everything uniformly.
    //Fog sphere spills into fog tiles that it finds.
    public void Spill(float increment)
    {
        Health -= increment;    //TODO: scale by the no of fog units currently being filled / max spilt fog capacity
        UpdateHeight();
        RenderColour();
        RenderOpacity();

        //bool overThreshold = true;        //TODO: once the threshold is triggered, instead of using the List<List<FogUnit>>, have the contents of the list check for
                                            //further adjacent tiles that aren't in the list or full, and add them to a single List<FogUnit>.
                                            //List<FogUnit> full = new List<FogUnit>();

        foreach (List<FogUnit> l in spiltFog)//Remove this loop once List<List<X>> works for uniform filling
        {
            foreach (FogUnit f in l)//spiltFog[0])
            {
                f.Health += increment;
                f.RenderColour();
                f.RenderOpacity();

                //if (f.Health < f.MaxHealth * 0.5f)
                //{
                //    overThreshold = false;
                //}
                //else if (f.Health >= f.MaxHealth)
                //{
                //    full.Add(f);
                //}
            }
        }

        //if (overThreshold && spiltFog.Count > 1)
        //{
        //    spiltFog[0].AddRange(spiltFog[1]);
        //    spiltFog.Remove(spiltFog[1]);
        //}
        
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
            UpdateHeight();
            RenderOpacity();
        }
    }

    //Triggered/Utility Methods - Appearance---------------------------------------------------------------------------------------------------------
    
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
        fogRenderer.material.SetFloat(alpha, Mathf.Lerp(startOpacity, endOpacity, health / maxHealth));
    }

    //Lerps height according to health/maxHealth
    public void UpdateHeight()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(minHeight, maxHeight, Mathf.Min(health / maxHealth, 1));
        transform.position = pos;
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

    public bool SpiltFogContains(FogUnit f)
    {
        foreach (List<FogUnit> l in spiltFog)
        {
            if (l.Contains(f))
            {
                return true;
            }
        }

        return false;
    }

    public void SpiltFogRemove(FogUnit f)
    {
        foreach (List<FogUnit> l in spiltFog)
        {
            if (l.Contains(f))
            {
                l.Remove(f);
            }
        }
    }
}
