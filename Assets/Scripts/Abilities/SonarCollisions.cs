using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Abilities
{
    public class SonarCollisions : MonoBehaviour
    {
        public GameObject sonarPingBackPrefab;
        
        // Ping the collectable
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.name + " pinged.");
            Collectable collectable = other.GetComponent<Collectable>();
            collectable.sonarPing.Play(true);
            collectable.isTriggered = true;
            collectable.pingTime = 5f;
            
            GameObject sonarEffect = Instantiate(sonarPingBackPrefab, collectable.transform.position, Quaternion.Euler(Vector3.zero));
            sonarEffect.GetComponent<Animator>().enabled = true;
            AbilityController.Instance.StartCoroutine(OnAnimationComplete(sonarEffect.GetComponent<Animator>()));
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-SonarPingback", GetComponent<Transform>().position);
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