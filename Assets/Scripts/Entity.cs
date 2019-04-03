using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private float health = 0.1f;
    protected Tile location;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (health == 0)
        {
            Destroy(this.gameObject);
        }
    }

    public virtual float Health
    {
        get
        {
            return health;
        }

        set
        {
            health = value;
        }
    }

    public Tile Location
    {
        get
        {
            return location;
        }

        set
        {
            location = value;
        }
    }
}
