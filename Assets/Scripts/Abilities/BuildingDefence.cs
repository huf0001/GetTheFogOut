using System.Collections.Generic;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu (menuName = "Abilities/BuildingDefence")]
    public class BuildingDefence : Ability
    {
        public float shieldAmount;

        public override void TriggerAbility(TileData tile)
        {
            Vector3 pos = new Vector3(tile.X, 0, tile.Z);
            Collider[] hitColliders = Physics.OverlapSphere(pos, targetRadius, LayerMask.GetMask("Buildings"));
            foreach (Collider collider in hitColliders)
            {
                Building building = collider.GetComponent<Building>();
                if (building)
                {
                    building.Shield = shieldAmount;
                    building.ShieldTime = duration;
                    building.isShieldOn = true;
                }
            }
        }
    }
}