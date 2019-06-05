﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogUnit : Entity
{
    //Serialized Fields
    [SerializeField] private float damage = 0.1f;
    [SerializeField] private float lerpToMaxInterval;
    [SerializeField] private float rapidLerpMultiplier = 3f;

    [SerializeField] private float startOpacity = 0f;
    [SerializeField] private float endOpacity = 0.90f;

    [SerializeField] [GradientUsageAttribute(true)] private Gradient docileColours;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient angryColours;
    //[SerializeField] [ColorUsageAttribute(true, true)] private Color damagedColour;
    //[ColorUsageAttribute(true, true)] private Color undamagedColour;
    [SerializeField] [GradientUsageAttribute(true)] private Gradient currentColours;
    [SerializeField] private float colourLerpSpeedMultiplier = 1f;

    //Non-Serialized Fields
    private Fog fog;
    private bool angry = false;

    private float colourProgress = 0;
    private float colourProgressTarget = 0;
    private bool lerpForward = true;
    private float healthProgress = 0;
    private float startHealth;
    private float targetHealth;
    private bool takingDamage = false;
    private float damageLerpProgress = 0;

    private bool spill = false;
    private bool neighboursFull = false;

    //Public Properties
    public Fog Fog { get => fog; set => fog = value; }
    public bool NeighboursFull { get => neighboursFull; set => neighboursFull = value; }
    public bool Spill { get => spill; set => spill = value; }
    public bool TakingDamage { get => takingDamage; }
    public bool Angry { get => angry; set => angry = value; }

    //Altered Public Properties
    public override float Health
    {
        get
        {
            return base.Health;
        }

        set
        {
            if (!takingDamage)
            {
                base.Health = value;

                if (base.Health >= maxHealth)
                {
                    base.Health = maxHealth;
                }
                else if (base.Health <= 0)
                {
                    ReturnToFogPool();
                }

                targetHealth = base.Health;
            }
        }
    }

    //Sets the starting values for fog damage health variables
    void Start()
    {
        startHealth = base.Health;
        targetHealth = base.Health;
        currentColours = docileColours;
    }

    //Fog uses this to set the starting emotion of a fog unit upon being dropped onto the board,
    //so that newly spawned fog units don't look docile when the fog is angry.
    public void SetStartEmotion(bool a)
    {
        angry = a;

        if (angry)
        {
            currentColours = angryColours;
        }
        else
        {
            currentColours = docileColours;
        }
    }

    //Fog unit deals damage to the building on its tile
    public void DealDamageToBuilding()
    {
        if (Location.Building != null)
        {
            Location.Building.DealDamageToBuilding(damage * (base.Health / MaxHealth));
        }
    }

    //A defence has dealtt damage to the fog unit
    public void DealDamageToFogUnit(float damage)
    {
        takingDamage = true;

        startHealth = base.Health;
        targetHealth -= damage;
        healthProgress = 0;

        if (targetHealth < 0)
        {
            targetHealth = 0;
        }

        foreach (TileData t in Location.AdjacentTiles)
        {
            if (t.FogUnit != null)
            {
                t.FogUnit.NeighboursFull = false;
            }
        }
    }
 
    //Updates the damage dealt to the fog unit
    public void UpdateDamageToFogUnit(float damageInterval)
    {
        base.Health = Mathf.Lerp(startHealth, targetHealth, healthProgress);

        if (base.Health <= targetHealth)
        {
            base.Health = targetHealth;
            takingDamage = false;
        }
        else
        {
            healthProgress += damageInterval * rapidLerpMultiplier;
        }

        if (base.Health <= 0)
        {
            ReturnToFogPool();
        }
    }

    //Updates the fog unit's shader colour at random between two values
    public void RenderColour()
    {
        //if (takingDamage && damageLerpProgress < 1)
        //{
        //    if (damageLerpProgress == 0)
        //    {
        //        undamagedColour = gameObject.GetComponent<Renderer>().material.color;
        //    }

        //    gameObject.GetComponent<Renderer>().material.SetColor("_Colour", Color.Lerp(undamagedColour, damagedColour, damageLerpProgress));

        //    damageLerpProgress += Time.deltaTime * colourLerpSpeedMultiplier;

        //    if (damageLerpProgress > 1)
        //    {
        //        damageLerpProgress = 1;
        //    }
        //}
        //else if (!takingDamage && damageLerpProgress > 0)
        //{
        //    gameObject.GetComponent<Renderer>().material.SetColor("_Colour", Color.Lerp(undamagedColour, damagedColour, damageLerpProgress));

        //    damageLerpProgress -= Time.deltaTime * colourLerpSpeedMultiplier;

        //    if (damageLerpProgress < 0)
        //    {
        //        damageLerpProgress = 0;
        //    }
        //}
        //else
        //{
        gameObject.GetComponent<Renderer>().material.SetColor("_Colour", currentColours.Evaluate(Mathf.Lerp(0, 1, colourProgress)));

        if ((!angry && currentColours == angryColours) || (angry && currentColours == docileColours))
        {
            colourProgress -= Time.deltaTime * colourLerpSpeedMultiplier;

            if (((!angry && currentColours == angryColours) || (angry && currentColours == docileColours)) && colourProgress < 0)
            {
                colourProgress = 0;
                colourProgressTarget = 0;

                if (angry)
                {
                    currentColours = angryColours;
                }
                else
                {
                    currentColours = docileColours;
                }
            }
        }
        else
        {
            if (colourProgress == colourProgressTarget)
            {
                colourProgressTarget = UnityEngine.Random.Range(0f, 1f);

                if (colourProgressTarget > colourProgress)
                {
                    lerpForward = true;
                }
                else
                {
                    lerpForward = false;
                }
            }

            if (lerpForward)
            {
                colourProgress += Time.deltaTime * colourLerpSpeedMultiplier;

                if (colourProgress > colourProgressTarget)
                {
                    colourProgress = colourProgressTarget;
                }
            }
            else
            {
                colourProgress -= Time.deltaTime * colourLerpSpeedMultiplier;

                if (colourProgress < colourProgressTarget)
                {
                    colourProgress = colourProgressTarget;
                }
            }
        }
        //} 
    }

    //Updates the fog unit's shader opacity according to its health
    public void RenderOpacity()
    {
        gameObject.GetComponent<Renderer>().material.SetFloat("_Alpha", Mathf.Lerp(startOpacity, endOpacity, base.Health / MaxHealth));
    }

    //Tells Fog to put the fog unit back in the pool
    public void ReturnToFogPool()
    {
        if (fog)
        {
            fog.QueueFogUnitForPooling(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
