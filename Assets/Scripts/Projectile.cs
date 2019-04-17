using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    int damage;
    int aoeDamage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fire(Vector3 origin, Vector3 target, int dmg, int aoeDmg)
    {
        damage = dmg;
        aoeDamage = aoeDmg;

        Vector3 Vo = CalculateVelocity(target, origin, 1f);
        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(Vo);
        gameObject.GetComponent<Rigidbody>().velocity = Vo;
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        // define the distance x and y first
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;

        // create a float to represent our distance
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
        Tile tileHit = collision.gameObject.GetComponent<Tile>();

        if (tileHit != null)
        {
            if (tileHit.FogUnit != null)
            {
                tileHit.FogUnit.Health -= damage;
                foreach (Tile tile in tileHit.AllAdjacentTiles)
                {
                    if (tile.FogUnit != null)
                    {
                        tile.FogUnit.Health -= aoeDamage;
                    }
                }  
            }
        }

        ProjectilePool.Instance.ReturnToPool(this);
    }
}
