﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvester : Building
{
    [SerializeField] private int harvestAmt = +5;
    [SerializeField] private Canvas noMineralCanvas;

    public int HarvestAmt { get => harvestAmt; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!location.Resource)
        {
            noMineralCanvas.transform.LookAt(Camera.main.transform);
        }
    }

    public void TurnOnMineralIndicator()
    {
        noMineralCanvas.gameObject.SetActive(true);
    }

    //public override void PowerUp()
    //{
    //    //if (location.Resource.ResourceType == Resource.Mineral)
    //    //{ 
    //    base.PowerUp();

    //    //if (!WorldController.Instance.Hub.Harvesters.Contains(this))
    //    //{
    //    //    WorldController.Instance.Hub.Harvesters.Add(this);
    //    //}
    //}

    //public override void PowerDown()
    //{
    //    base.PowerDown();

    //    //if (WorldController.Instance.Hub.Harvesters.Contains(this))
    //    //{
    //    //    WorldController.Instance.Hub.Harvesters.Remove(this);
    //    //}
    //}
}
