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
                    
                    Renderer[] renderers = t.Building.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.material.SetFloat("_Overclock", 1f);
                    }
                    
                    switch (t.Building.BuildingType)
                    {
                        case BuildingType.Harvester:
                            t.Building.GetComponentInChildren<ParticleSystem>().startSpeed = 8f;
                            t.Building.GetComponentInChildren<ParticleSystem>().startLifetime = 1.25f;

                            break;
                        case BuildingType.FogRepeller:
                            t.Building.GetComponentInChildren<ParticleSystem>().emission.
                                SetBurst(0 , new ParticleSystem.Burst(0f, 2, 10, 0.5f));
                            break;
                    }
                    
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
            Renderer[] renderers = b.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.SetFloat("_Overclock", 0f);
            }
            
            switch (b.BuildingType)
            {
                case BuildingType.Harvester:
                    b.GetComponentInChildren<ParticleSystem>().startSpeed = 2f;
                    b.GetComponentInChildren<ParticleSystem>().startLifetime = 5f;

                    break;
                case BuildingType.FogRepeller:
                    b.GetComponentInChildren<ParticleSystem>().emission.
                        SetBurst(0 , new ParticleSystem.Burst(0f, 2, 1, 0.5f));
                    break;
            }
        }
    }
}