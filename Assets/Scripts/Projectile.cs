using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    int damage, aoeDamage, innerRange, outerRange;
    float timeToReturn;
    bool isLanded;
    private Rigidbody rigidbody;

    public GameObject hitEffect;

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

    public void Fire(Vector3 origin, Vector3 target, int dmg, int aoeDmg, int inRange, int outRange)
    // Fires the projectile from the origin to the target, with the given damage
    {
        damage = dmg;
        aoeDamage = aoeDmg;
        innerRange = inRange;
        outerRange = outRange;

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

        // If the collision is a tile and it has fog, damage the fog
        if (tileHit != null)
        {
            if (tileHit.FogUnitActive)
            {
                if (innerRange > 0)
                {
                    List<TileData> innerTiles = tileHit.CollectTilesInRange(innerRange);

                    foreach (TileData tile in innerTiles)
                    {
                        if (tile.FogUnitActive)
                        {
                            tile.FogUnit.DealDamageToFogUnit(damage);
                        }
                    }
                }
                else
                {
                    tileHit.FogUnit.DealDamageToFogUnit(damage);
                }
                
                Destroy((Instantiate(hitEffect, transform.position, transform.rotation)), 1f);

                List<TileData> outerTiles = tileHit.CollectTilesInRange(outerRange);
                foreach (TileData tile in outerTiles)
                {
                    if (tile.FogUnitActive)
                    {
                        tile.FogUnit.DealDamageToFogUnit(aoeDamage);
                    }
                }
            }
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
