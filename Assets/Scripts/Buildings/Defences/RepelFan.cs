using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class RepelFan : Defence
{
    [SerializeField] private int directDamage = 50;
    [SerializeField] private int aoeDamage = 25;
    [SerializeField] private float rateOfFire = 1f;
    [SerializeField] private bool placedInEditor = false;
   // private VisualEffect myeffect;

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (placedInEditor)
        {
            InvokeRepeating("FirePulse", 1f, rateOfFire);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void PowerUp()
    {
        base.PowerUp();
        this.gameObject.GetComponentInChildren<ParticleSystem>().Play();
        if (!IsInvoking("FirePulse"))
        {
            InvokeRepeating("FirePulse", 1f, rateOfFire);
        }
    }

    public override void PowerDown()
    {
        base.PowerDown();
        this.gameObject.GetComponentInChildren<ParticleSystem>().Pause();
        CancelInvoke("FirePulse");
    }

    void FirePulse()
    {
        List<TileData> directTarget = location.AllAdjacentTiles;
        List<TileData> notDirectTarget = GetTarget(2);

        if ((directTarget != null) || notDirectTarget != null)
        {
            foreach (TileData dir in directTarget)
            {
                if ((dir.FogUnit != null && !dir.FogUnit.TakingDamage))
                {
                    dir.FogUnit.DealDamageToFogUnit(directDamage);
                }
            }

            foreach (TileData ndir in notDirectTarget)
            {
                if (ndir.FogUnit != null && !ndir.FogUnit.TakingDamage)
                {
                    ndir.FogUnit.DealDamageToFogUnit(aoeDamage);
                }
            }

        }

    }

    List<TileData> GetTarget(int range)
    {
        List<TileData> tiles = location.CollectTilesInRange(location.X, location.Z, range);

        return tiles;
    }
}
