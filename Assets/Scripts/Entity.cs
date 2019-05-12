﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : Locatable
{
    [SerializeField] private float health = 1f;

    public virtual float Health { get => health; set => health = value; }
    public float MaxHealth { get; protected set; }

    // Start is called before the first frame update
    //void Start()
    //{
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    //if (health == 0)
    //    //{
    //    //    Debug.Log("This entity has been destroyed. Called from Entity.Update()");
    //    //    Destroy(this.gameObject);
    //    //}
    //}

    protected bool GotNoHealth()
    {
        if (health <= 0)
        {
            return true;
        }

        return false;
    }
}
