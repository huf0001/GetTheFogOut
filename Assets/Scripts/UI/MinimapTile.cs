using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MinimapTile : MonoBehaviour
{
    public TileData Tile { get; set; }

    [SerializeField] private Color poweredTile = new Color(0, 0.8f, 0);
    [SerializeField] private Color unpoweredTile = new Color(0.5f, 0, 0, 0.7f);
    [SerializeField] private Color buildingTile = new Color(0, 0, 0.7f);
    [SerializeField] private Color buildingDamageTile = new Color(0.9f, 0, 0);
    [SerializeField] private Color shipTile = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color collectableTile = new Color(1, 1, 1);
    [SerializeField] private Color collectableTileAlt;
    [SerializeField] private Color mineralTile = new Color32(228, 148, 0, 255);
    [SerializeField] private Color obstacleTile;
    private MaterialPropertyBlock material;
    private Renderer rend;
    private Color curColour;
    private float curFoggyness;

    private void Start()
    {
        //material = GetComponent<Renderer>().material;
        rend = GetComponent<Renderer>();
        material = new MaterialPropertyBlock();
        if (Tile.buildingChecks.obstacle)
        {
            material.SetColor("_BaseColor", obstacleTile);
            rend.SetPropertyBlock(material);
        }
        else
        {
            InvokeRepeating(nameof(CheckColour), 0.1f, 0.5f);
        }
    }

    private void CheckColour()
    {
        if (Tile.Building)
        {
            if (Tile.Building.TakingDamage && curColour != buildingDamageTile)
            {
                material.SetColor("_BaseColor", buildingDamageTile);
                curColour = buildingDamageTile;
            }
            else if (Tile.Building.BuildingType == BuildingType.Hub && curColour != shipTile)
            {
                material.SetColor("_BaseColor", shipTile);
                curColour = shipTile;
            }
            else if (curColour != buildingTile)
            {
                material.SetColor("_BaseColor", buildingTile);
                curColour = buildingTile;
            }
        }
        else if (Tile.Resource)
        {
            if (curColour != mineralTile)
            {
                material.SetColor("_BaseColor", mineralTile);
                curColour = mineralTile;
            }
        }
        else if (!Tile.PowerSource)
        {
            if (curColour != unpoweredTile)
            {
                material.SetColor("_BaseColor", unpoweredTile);
                curColour = unpoweredTile;
            }
        }
        else if (Tile.Collectible)
        {
            if (curColour != collectableTile)
            {
                material.SetColor("_BaseColor", collectableTile);
                curColour = collectableTile;
            }
            else
            {
                material.SetColor("_BaseColor", collectableTileAlt);
                curColour = collectableTileAlt;
            }
        }
        else if (Tile.PowerSource)
        {
            if (curColour != poweredTile)
            {
                material.SetColor("_BaseColor", poweredTile);
                curColour = poweredTile;
            }
        }

        if (Tile.FogUnitActive)
        {
            float healthPer = Tile.FogUnit.Health / Tile.FogUnit.MaxHealth;
            if (curFoggyness != healthPer)
            {
                material.SetFloat("_Foggyness", healthPer);
                curFoggyness = healthPer;
            }
        }
        else if (curFoggyness != 0)
        {
            material.SetFloat("_Foggyness", 0);
            curFoggyness = 0;
        }

        rend.SetPropertyBlock(material);
    }
}
