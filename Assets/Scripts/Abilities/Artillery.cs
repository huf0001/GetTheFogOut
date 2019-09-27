using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Abilities
{
    [CreateAssetMenu (menuName = "Abilities/DamageBlast")]
    public class Artillery : Ability
    {
        public GameObject effectPrefab;
        public int fullDamageRange;
        public int fullDamage;
        public int fallOffDamage;

        public override void TriggerAbility(TileData tile)
        {
            AbilityController.Instance.StartCoroutine(KillFog(tile));
            GameObject effectGO = Instantiate(
                effectPrefab, tile.Position, Quaternion.Euler(0, 0, 0), WorldController.Instance.transform);
            AbilityController.Instance.StartCoroutine(KillEffect(effectGO));
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-ArtilleryStrike", tile.Position);
        }

        IEnumerator KillEffect(GameObject g)
        {
            float time = 10f;
            float normalizedTime = 0f;
            
            while (normalizedTime <= 1f)
            {
                normalizedTime += Time.deltaTime / time;
                yield return null;
            }
            
            Destroy(g);
        }

        IEnumerator KillFog(TileData tile)
        {
            float time = 2f;
            float normalizedTime = 0f;
            
            while (normalizedTime <= 1f)
            {
                normalizedTime += Time.deltaTime / time;
                yield return null;
            }
            
            // Gets the required tiles to damage
            List<TileData> fullDamageTiles = tile.CollectTilesInRange(fullDamageRange);
            List<TileData> allDamageTiles = tile.CollectTilesInRange(targetRadius);
            List<TileData> lowDamageTiles = allDamageTiles.Except(fullDamageTiles).ToList();

            // Deals full damage to short range tiles
            foreach (var t in fullDamageTiles)
            {
                if (t.FogUnitActive)
                {
                    t.FogUnit.DealDamageToFogUnit(fullDamage);
                }
            }
            
            // Deals less damage to far range tiles
            foreach (var t in lowDamageTiles)
            {
                if (t.FogUnitActive)
                {
                    t.FogUnit.DealDamageToFogUnit(fallOffDamage);
                }
            }
        }
    }
}