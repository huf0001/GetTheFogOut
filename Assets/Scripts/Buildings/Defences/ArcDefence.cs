﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class ArcDefence : Defence
{
    public GameObject mortarBarrelGO;
    public int innerRange, outerRange;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (placedInEditor)
        {
            InvokeRepeating(nameof(Fire), 0.25f, rateOfFire);
        }
    }

    public override void Place()
    {
        base.Place();
        if (WorldController.Instance.mortarUpgradeLevel) 
            Upgrade(WorldController.Instance.mortarUpgradeLevel);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void PowerUp()
    {
        base.PowerUp();

        if (!IsInvoking(nameof(Fire)))
        {
            InvokeRepeating(nameof(Fire), rateOfFire / OverclockValue, rateOfFire / OverclockValue);
        }
    }

    public override void PowerDown()
    {
        base.PowerDown();
        CancelInvoke(nameof(Fire));
    }

    protected override void ResetFire()
    {
        if (IsInvoking(nameof(Fire)))
        {
            CancelInvoke(nameof(Fire));
            InvokeRepeating(nameof(Fire), rateOfFire / OverclockValue, rateOfFire / OverclockValue);
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
                            p.Fire(origin, targetPos, directDamage, aoeDamage, innerRange, outerRange);
                        });
            }
        }
    }

    TileData GetTarget()
    {
        // Get the closest fog tile.
        List<TileData> fogTiles = new List<TileData>();
        List<TileData> tiles = location.CollectTilesInRange((int)visibilityRange);

        foreach (TileData tile in tiles)
        {
            if (tile.FogUnitActive && !tile.FogUnit.TakingDamage)
            {
                fogTiles.Add(tile);
            }
        }
        
        if (fogTiles.Count > 1)
        {
            fogTiles = fogTiles.OrderBy(x => Vector3.Distance(this.transform.position, x.Position)).ToList();
            return fogTiles[0];
        } 
        else if (fogTiles.Count == 1)
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

    public void Upgrade(Upgrade upgrade)
    {
        switch (upgrade.pathNum)
        {
            case 1:
                switch (upgrade.upgradeNum)
                {
                    case 1:
                        outerRange = 2;
                        break;
                    case 2:
                        innerRange = 1;
                        outerRange = 3;
                        break;
                }
                break;
            case 2:
                switch (upgrade.upgradeNum)
                {
                    case 1:
                        upkeep = -1.6f;
                        break;
                    case 2:
                        upkeep = -1f;
                        break;
                }
                break;
        }
    }
}
