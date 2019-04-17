using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterFan : Defence
{

    [SerializeField] private int directDamage = 50;
    [SerializeField] private int aoeDamage = 25;
    [SerializeField] private float rateOfFire = 0.25f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void Place()
    {
        base.Place();
        InvokeRepeating("Fire", 0.25f, rateOfFire);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CancelInvoke();
    }

    void Fire()
    {
        Tile target = GetTarget();

        if (target != null)
        {
            // Get a projectile from the pool and send it on its way
            Projectile p = ProjectilePool.Instance.GetFromPool();

            Vector3 origin = transform.position;
            origin.y += 0.4f;
            p.Fire(origin, target.transform.position, directDamage, aoeDamage);

        }
    }

    Tile GetTarget()
    {
        // Get the tile with the highest fog concentration.
        Collider[] tiles = Physics.OverlapSphere(transform.position, visibilityRange, LayerMask.GetMask("Tiles"));
        List<Tile> fogTiles = new List<Tile>();

        foreach (Collider tile in tiles)
        {
            if (tile.gameObject.GetComponent<Tile>().FogUnit != null)
            {
                fogTiles.Add(tile.gameObject.GetComponent<Tile>());
            }
        }

        fogTiles.Sort((t1, t2) => t1.FogUnit.Health.CompareTo(t2.FogUnit.Health));
        fogTiles.Reverse();

        if (fogTiles.Count > 0)
        {
            Tile target = fogTiles[0];
            return target;
        } else
        {
            return null;
        }
    }
}
