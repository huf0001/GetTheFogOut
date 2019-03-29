﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerSource : Building
{
    [SerializeField] protected float powerRange;

    [SerializeField] protected List<Building> suppliedBuildings = new List<Building>();
    
    protected override void Awake()
    {
        base.Awake();
        ActivateTiles();
    }

    protected override void OnDestroy()
    {
        DeactivateTiles();
        base.OnDestroy();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ActivateTiles()
    {
        Collider[] tilesToActivate = Physics.OverlapSphere(transform.position, powerRange);

        foreach (Collider c in tilesToActivate)
        {
            if (c.gameObject.GetComponent<Tile>() != null)
            {
                c.gameObject.GetComponent<Tile>().PowerUp(this as PowerSource);
            }
        }
    }

    private void DeactivateTiles()
    {
        Collider[] tilesToDeactivate = Physics.OverlapSphere(transform.position, powerRange);

        foreach (Collider c in tilesToDeactivate)
        {
            if (c.gameObject.GetComponent<Tile>() != null)
            {
                c.gameObject.GetComponent<Tile>().PowerDown(this as PowerSource);
            }
        }
    }

    public float PowerRange
    {
        get
        {
            return powerRange;
        }
    }

    public void PlugIn(Building newBuilding)
    {
        suppliedBuildings.Add(newBuilding);
    }

    public void Unplug(Building unplug)
    {
        if (suppliedBuildings.Contains(unplug))
        {
            suppliedBuildings.Remove(unplug);
        }
    }

    public abstract bool SupplyingPower();
}
