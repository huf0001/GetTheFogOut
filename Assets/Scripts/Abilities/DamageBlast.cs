using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu (menuName = "Abilities/DamageBlast")]
    public class DamageBlast : Ability
    {
        public int fallOffRange;
        
        public override void Initialize(GameObject obj)
        {
            throw new System.NotImplementedException();
        }

        public override void TriggerAbility(TileData tile)
        {
            throw new System.NotImplementedException();
        }
    }
}