using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    bool isFired = false;
    Vector3 posStart;
    Vector3 posEnd;
    int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isFired)
        {
            Move();
            counter++;
            if (counter > 1000) ProjectilePool.Instance.ReturnToPool(this);
        }
    }

    public void Fire(Vector3 start, Vector3 end)
    {
        posStart = start;
        posEnd = end;
        isFired = true;
    }

    void Move()
    {
        float animation = Time.deltaTime;
        animation = animation % 5f;

        transform.position = MathParabola.Parabola(posStart, posEnd, 5, animation / 5f);
    }
}
