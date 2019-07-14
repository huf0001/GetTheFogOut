using System;
using UnityEngine;

namespace Abilities
{
    public class SonarCollisions : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            // TODO: Ping the collectible
            Debug.Log(other.gameObject.name + " pinged.");
        }
    }
}