using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ArcDefence : Defence
{
    public GameObject mortarBarrelGO;
        
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
            InvokeRepeating("Fire", rateOfFire / OverclockValue, rateOfFire / OverclockValue);
        }
    }

    public override void PowerDown()
    {
        base.PowerDown();
        CancelInvoke("Fire");
    }

    protected override void ResetFire()
    {
        if (IsInvoking("Fire"))
        {
            CancelInvoke("Fire");
            InvokeRepeating("Fire", rateOfFire / OverclockValue, rateOfFire / OverclockValue);
        }
    }
    
    void Fire()
    {
        if (TutorialController.Instance.DefencesOn)
        {
            TileData target = GetTarget();

            if (target != null)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.Append(mortarBarrelGO.transform.DOLookAt(target.Position, 0.5f, AxisConstraint.Y))
                    .OnComplete(
                        delegate
                        {
                            // Get a projectile from the pool and send it on its way
                            Projectile p = ProjectilePool.Instance.GetFromPool();

                            Vector3 origin = transform.position;
                            origin.y += 0.8f;
                            Vector3 targetPos = new Vector3(target.X, 0, target.Z);
                            p.Fire(origin, targetPos, directDamage, aoeDamage);
                        });
            }
        }
    }

    TileData GetTarget()
    {
        // Get the tile with the highest fog concentration.
        List<TileData> fogTiles = new List<TileData>();
        List<TileData> tiles = location.CollectTilesInRange((int)visibilityRange);

        foreach (TileData tile in tiles)
        {
            if (tile.FogUnitActive && !tile.FogUnit.TakingDamage)
            {
                fogTiles.Add(tile);
            }
        }

        fogTiles.Sort((t1, t2) => t1.FogUnit.Health.CompareTo(t2.FogUnit.Health));
        fogTiles.Reverse();

        if (fogTiles.Count > 0)
        {
            TileData target = fogTiles[0];
            return target;
        }
        else
        {
            return null;
        }
    }

    public override bool TargetInRange()
    {
        List<TileData> tiles = location.CollectTilesInRange((int)visibilityRange);

        foreach (TileData tile in tiles)
        {
            if (tile.FogUnitActive)
            {
                return true;
            }
        }

        return false;
    }
}
