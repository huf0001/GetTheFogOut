using DG.Tweening;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu (menuName = "Abilities/Sonar")]
    public class Sonar : Ability
    {
        private SphereCollider collider;
        private GameObject colliderGo;
        private bool isActive;

        public float sonarSpeed;
        public GameObject colliderPrefab;

        public override void TriggerAbility(TileData tile)
        {
            if (!collider)
            {
                colliderGo = Instantiate(colliderPrefab);
                collider = colliderGo.GetComponent<SphereCollider>();
            }
            
            if (!isActive)
            {
                // Set starting values
                isActive = true;
                colliderGo.transform.position = tile.Position;
                collider.radius = 0;
                
                // Tween the collider
                Sequence sequence = DOTween.Sequence();
                sequence.Append(DOTween.To(() => collider.radius, x => collider.radius = x, targetRadius, sonarSpeed))
                    .OnComplete(() => isActive = false);
                // TODO: visual effect
            }
        }
    }
}