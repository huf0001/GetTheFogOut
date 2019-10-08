using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class RepelFan : Defence
{
    public int repelRange;
    public GameObject shockwave;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (placedInEditor)
        {
            InvokeRepeating(nameof(FirePulse), 1f, rateOfFire);
        }
    }

    public override void Place()
    {
        base.Place();
        if (WorldController.Instance.pulseDefUpgradeLevel) 
            Upgrade(WorldController.Instance.pulseDefUpgradeLevel);
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
            List<TileData> notDirectTarget = GetTarget(repelRange);

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

    public void Upgrade(Upgrade upgrade)
    {
        switch (upgrade.pathNum)
        {
            case 1:
                switch (upgrade.upgradeNum)
                {
                    case 1:
                        repelRange = 3;
                        shockwave.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
                        rangeIndicator.transform.localScale = new Vector3(1f,0.3636363f, 1f);
                        break;
                    case 2:
                        repelRange = 4;
                        shockwave.transform.localScale = new Vector3(1.55f, 1f, 1.55f);
                        rangeIndicator.transform.localScale = new Vector3(1.3f,0.3636363f, 1.3f);
                        break;
                }
                break;
            case 2:
                switch (upgrade.upgradeNum)
                {
                    case 1:
                        upkeep = -0.8f;
                        break;
                    case 2:
                        upkeep = -0.5f;
                        break;
                }
                break;
        }
    }
}
