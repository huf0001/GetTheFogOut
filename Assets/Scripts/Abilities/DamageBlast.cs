using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Abilities
{
    [CreateAssetMenu (menuName = "Abilities/DamageBlast")]
    public class DamageBlast : Ability
    {
        public int fullDamageRange;
        public int fullDamage;
        public int fallOffDamage;

        public override void TriggerAbility(TileData tile)
        {
            // Gets the required tiles to damage
            List<TileData> fullDamageTiles = tile.CollectTilesInRange(tile.X, tile.Z, fullDamageRange);
            List<TileData> allDamageTiles = tile.CollectTilesInRange(tile.X, tile.Z, targetRadius);
            List<TileData> lowDamageTiles = allDamageTiles.Except(fullDamageTiles).ToList();

            // Deals full damage to short range tiles
            foreach (var t in fullDamageTiles)
            {
                if (t.FogUnit)
                {
                    t.FogUnit.DealDamageToFogUnit(fullDamage);
                }
            }
            
            // Deals less damage to far range tiles
            foreach (var t in lowDamageTiles)
            {
                if (t.FogUnit)
                {
                    t.FogUnit.DealDamageToFogUnit(fallOffDamage);
                }
            }
            
            // TODO: Audio effect
            // TODO: Visual effect
        }
    }
}