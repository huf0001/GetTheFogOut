using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class RepelFan : Defence
{

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (placedInEditor)
        {
            InvokeRepeating(nameof(FirePulse), 1f, rateOfFire);
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

        if (TutorialController.Instance.DefencesOn)
        {
            GetComponentInChildren<ParticleSystem>().Play();
        }
        else
        {
            GetComponentInChildren<ParticleSystem>().Stop();
        }

        if (!IsInvoking(nameof(FirePulse)))
        {
            InvokeRepeating(nameof(FirePulse), rateOfFire / OverclockValue, rateOfFire / OverclockValue);
        }
    }

    public override void PowerDown()
    {
        base.PowerDown();
        GetComponentInChildren<ParticleSystem>().Stop();
        CancelInvoke(nameof(FirePulse));
    }

    protected override void ResetFire()
    {
        if (IsInvoking(nameof(FirePulse)))
        {
            CancelInvoke(nameof(FirePulse));
            InvokeRepeating(nameof(FirePulse), rateOfFire / OverclockValue, rateOfFire / OverclockValue);
        }
    }
    
    void FirePulse()
    {
        if (TutorialController.Instance.DefencesOn)
        {
            List<TileData> directTarget = location.AllAdjacentTiles;
            List<TileData> notDirectTarget = GetTarget(2);

            if ((directTarget != null) || notDirectTarget != null)
            {
                foreach (TileData dir in directTarget)
                {
                    if ((dir.FogUnitActive && !dir.FogUnit.TakingDamage))
                    {
                        dir.FogUnit.DealDamageToFogUnit(directDamage);
                    }
                }

                foreach (TileData ndir in notDirectTarget)
                {
                    if (ndir.FogUnitActive && !ndir.FogUnit.TakingDamage)
                    {
                        ndir.FogUnit.DealDamageToFogUnit(aoeDamage);
                    }
                }

            }
        }
    }

    List<TileData> GetTarget(int range)
    {
        List<TileData> tiles = location.CollectTilesInRange(range);

        return tiles;
    }

    public override bool TargetInRange()
    {
        List<TileData> tiles = GetTarget(2);

        foreach (TileData t in tiles)
        {
            if (t.FogUnitActive)
            {
                return true;
            }
        }

        return false;
    }
}
