﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvester : Building
{
    [SerializeField] private int harvestAmt = +5;
    [SerializeField] private Canvas noMineralCanvas;
    [SerializeField] private Light offLight;
    [SerializeField] public ParticleSystem hvtProgress;

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
        getMineral();
        if (!location.Resource)
        {
            noMineralCanvas.transform.LookAt(cam.transform);
        }
    }

    public void TurnOnMineralIndicator()
    {
        noMineralCanvas.gameObject.SetActive(true);
        
        offLight.GetComponent<Light>().enabled = true; //turn the red light on 
        hvtProgress.Stop(); // stop the particle
    }
    
    public override void PowerUp()
    {
        //    //if (location.Resource.ResourceType == Resource.Mineral)
        //    //{ 
        base.PowerUp();
        if (hvtProgress.isStopped)
        {
            hvtProgress.Play();
        }
    }

    public void getMineral()
    {
        /*
 100%: 21FF00  r33,g255,b0
75%: FFFF00 r255,g255,b0
50%: FF9A00 r255,g154,b0
25%: FF0000 r255,g0,b0
 */
        ResourceNode[] resources = FindObjectsOfType<ResourceNode>();
        ParticleSystem.MainModule main = hvtProgress.main;
        TileData hvtTile = WorldController.Instance.GetTileAt(this.transform.position);
        if (hvtTile.Resource)
        {
            Color changeColor = main.startColor.color;
            float amountLeft = hvtTile.Resource.Health / hvtTile.Resource.MaxHealth * 100;
            if (amountLeft != 0)
            {
                if (100 > amountLeft && amountLeft > 75)
                {
                    changeColor = new Color(0.129f, 1f, 0f);
                }
                else if (75 > amountLeft && amountLeft > 50)
                {
                    changeColor = new Color(1f, 1f, 0f);
                }
                else if (50 > amountLeft && amountLeft > 25)
                {
                    changeColor = new Color(1f, 0.325f, 0f);
                }
                else if (25 > amountLeft && amountLeft > 0)
                {
                    changeColor = new Color(1f, 0f, 0f);
                }
                main.startColor = changeColor;
            }
        }
    }
    public override void PowerDown()
    {
        if (hvtProgress.isPlaying)
        {
            hvtProgress.Stop();
        }
            base.PowerDown();
    }
}
