﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private float health = 1f;
    [SerializeField] protected TileData location;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (health == 0)
        //{
        //    Debug.Log("This entity has been destroyed. Called from Entity.Update()");
        //    Destroy(this.gameObject);
        //}
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

    public TileData Location
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
