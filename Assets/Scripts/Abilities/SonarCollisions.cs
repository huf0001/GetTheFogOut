using System;
using System.Collections;
using UnityEngine;

namespace Abilities
{
    public class SonarCollisions : MonoBehaviour
    {
        // Ping the collectable
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.name + " pinged.");
            Collectable collectable = other.GetComponent<Collectable>();
            collectable.sonarPing.Play(true);
            collectable.isTriggered = true;
            collectable.pingTime = 3f;
        }
    }
}