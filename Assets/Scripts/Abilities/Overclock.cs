using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu (menuName = "Abilities/Overclock")]
    public class Overclock : Ability
    {
        private float timer;
        
        public override void TriggerAbility(TileData tile)
        {
            List<TileData> tiles = tile.CollectTilesInRange(targetRadius);
            foreach (TileData t in tiles)
            {
                if (t.Building)
                {
                    t.Building.IsOverclockOn = true;
                    timer = duration;
                    AbilityController.Instance.StartCoroutine(TurnOffOverclock(t.Building));
                }
            }
        }

        private IEnumerator TurnOffOverclock(Building b)
        {
            b.overclockTimer = duration;
            
            while (b.overclockTimer > 0)
            {
                b.overclockTimer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            b.IsOverclockOn = false;
        }
    }
}