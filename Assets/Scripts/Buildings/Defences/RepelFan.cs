using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepelFan : Defence
{
    [SerializeField] private int directDamage = 50;
    [SerializeField] private int aoeDamage = 25;
    [SerializeField] private float rateOfFire = 1f;
    [SerializeField] private bool placedInEditor = false;

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
            InvokeRepeating("Fire", 1f, rateOfFire);
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

        if (!IsInvoking("Fire"))
        {
            InvokeRepeating("Fire", 1f, rateOfFire);
        }
    }

    public override void PowerDown()
    {
        base.PowerDown();
        CancelInvoke();
    }

    void Fire()
    {
        List<TileData> directtarget = location.AllAdjacentTiles;
        List<TileData> Notdirecttarget = getTarget(2);

        if ((directtarget != null) || Notdirecttarget != null)
        {
            foreach (TileData dir in directtarget)
            {
                if ((dir.FogUnit != null && !dir.FogUnit.TakingDamage))
                {
                    dir.FogUnit.DealDamageToFogUnit(directDamage);
                }
                //else if (dir.FogBatch.Batched)
                //{
                //    dir.FogBatch.DealDamage(directDamage, new Vector3(dir.X, 0, dir.Z));
                //}
            }

            foreach (TileData ndir in Notdirecttarget)
            {
                if (ndir.FogUnit != null && !ndir.FogUnit.TakingDamage)
                {
                    ndir.FogUnit.DealDamageToFogUnit(aoeDamage);
                }
                //else if (ndir.FogBatch.Batched)
                //{
                //    ndir.FogBatch.DealDamage(directDamage, new Vector3(ndir.X, 0, ndir.Z));
                //}
            }

        }

    }

    List<TileData> getTarget(int range)
    {
        List<TileData> tiles = location.CollectTilesInRange(location.X, location.Z, range);

        return tiles;
    }
}
