using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterFan : Defence
{

    [SerializeField] private int directDamage = 50;
    [SerializeField] private int aoeDamage = 25;
    [SerializeField] private float rateOfFire = 0.25f;
    [SerializeField] private bool placedInEditor = false;

    public AudioSource audioSpawn;

    // Start is called before the first frame update
    protected override void Start()
    {
        audioSpawn = GetComponent<AudioSource>();
        base.Start();
        if (placedInEditor)
        {
            InvokeRepeating("Fire", 0.25f, rateOfFire);
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
            InvokeRepeating("Fire", 0.25f, rateOfFire);
        }
    }

    public override void PowerDown()
    {
        base.PowerDown();
        CancelInvoke();
    }

    public override void Place()
    {
        base.Place();
        audioSpawn.Play();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    void Fire()
    {
        TileData target = GetTarget();

        if (target != null)
        {
            // Get a projectile from the pool and send it on its way
            Projectile p = ProjectilePool.Instance.GetFromPool();

            Vector3 origin = transform.position;
            origin.y += 0.4f;
            Vector3 targetPos = new Vector3(target.X, 0, target.Z);
            p.Fire(origin, targetPos, directDamage, aoeDamage);

        }
    }

    TileData GetTarget()
    {
        // Get the tile with the highest fog concentration.
        List<TileData> fogTiles = new List<TileData>();
        List<TileData> tiles = new List<TileData>();
        location.CollectTilesInRange(tiles, (int)visibilityRange);

        foreach (TileData tile in tiles)
        {
            if (tile.FogUnit != null)
            {
                fogTiles.Add(tile);
            }
            tile.Visited = false;
        }

        fogTiles.Sort((t1, t2) => t1.FogUnit.Health.CompareTo(t2.FogUnit.Health));
        fogTiles.Reverse();

        if (fogTiles.Count > 0)
        {
            TileData target = fogTiles[0];
            return target;
        } else
        {
            return null;
        }
    }
}
