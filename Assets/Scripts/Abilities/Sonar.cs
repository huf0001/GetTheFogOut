using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu (menuName = "Abilities/Sonar")]
    public class Sonar : Ability
    {
        private SphereCollider collider;
        private GameObject colliderGo;
        private bool isActive = false;

        public float sonarSpeed;
        public int sonarRange;
        public GameObject colliderPrefab;
        public GameObject sonarEffectPrefab;

        public override void TriggerAbility(TileData tile)
        {
            if (!collider)
            {
                colliderGo = Instantiate(colliderPrefab);
                collider = colliderGo.GetComponent<SphereCollider>();
                isActive = false;
            }
            
            if (!isActive)
            {
                // Set starting values
                isActive = true;
                colliderGo.SetActive(true);
                colliderGo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                colliderGo.transform.position = tile.Position;

                // Tween the collider
                Sequence sequence = DOTween.Sequence();
                sequence.Append(colliderGo.transform.DOScale((sonarRange / 2) + 6, sonarSpeed))
                    .OnComplete(delegate
                    {
                        colliderGo.SetActive(false);
                        isActive = false;
                    });

                GameObject sonarEffect = Instantiate(sonarEffectPrefab, tile.Position, Quaternion.Euler(Vector3.zero));
                sonarEffect.GetComponent<Animator>().enabled = true;
                AbilityController.Instance.StartCoroutine(OnAnimationComplete(sonarEffect.GetComponent<Animator>()));
            }
        }

        IEnumerator OnAnimationComplete(Animator animator)
        {
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                yield return null;
            }
            
            Destroy(animator.gameObject);
        }
    }
}