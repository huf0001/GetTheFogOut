using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu (menuName = "Abilities/FreezeFog")]
    public class FreezeFog : Ability
    {
        public override void TriggerAbility(TileData tile)
        {
            Fog.Instance.PauseFogGrowth(duration);
        }
    }
}