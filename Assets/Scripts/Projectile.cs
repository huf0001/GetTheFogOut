using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    int damage;
    int aoeDamage;
    float timeToReturn;
    bool isLanded;
    private Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isLanded)
        {
            ReturnMeToPool();
        }
    }

    public void Fire(Vector3 origin, Vector3 target, int dmg, int aoeDmg)
    // Fires the projectile from the origin to the target, with the given damage
    {
        damage = dmg;
        aoeDamage = aoeDmg;

        Vector3 Vo = CalculateVelocity(target, origin, 1f);
        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(Vo);
        gameObject.GetComponent<Rigidbody>().velocity = Vo;
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    // Calculates the velocity for an arching projectile
    // Taken from: https://www.youtube.com/watch?v=03GHtGyEHas
    {
        // Define the distance x and y first
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;

        // Create a float to represent our distance
        float Sy = distance.y;
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint hit = collision.GetContact(0);
        TileData tileHit = WorldController.Instance.GetTileAt(new Vector2(hit.point.x, hit.point.z));
        //Tile tileHit = collision.gameObject.GetComponent<Tile>();

        // If the collision is a tile and it has fog, damage the fog
        if (tileHit != null)
        {
            if (tileHit.FogUnit != null)
            {
                //Debug.Log("Dealing damage to " + tileHit.FogUnit);
                tileHit.FogUnit.DealDamageToFogUnit(damage);

                foreach (TileData tile in tileHit.AllAdjacentTiles)
                {
                    if (tile.FogUnit != null)
                    {
                        //Debug.Log("Dealing damage to " + tile.FogUnit);
                        tile.FogUnit.DealDamageToFogUnit(aoeDamage);
                    }
                }
            }
            //else if (tileHit.FogBatch.Batched)
            //{
            //    //Debug.Log("Dealing damage to " + tileHit.FogUnit);
            //    tileHit.FogBatch.DealDamage(damage, transform.position);
            //}
        }

        // Return the projectile to the pool
        timeToReturn = 0.7f;
        isLanded = true;
        rigidbody.isKinematic = true;
    }

    void ReturnMeToPool()
    {
        timeToReturn -= Time.deltaTime;
        if (timeToReturn <= 0)
        {
            ProjectilePool.Instance.ReturnToPool(this);
            isLanded = false;
            rigidbody.isKinematic = false;
        }
    }
}
