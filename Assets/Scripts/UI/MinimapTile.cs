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
    [SerializeField] private Color shipTile = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color collectableTile = new Color(1, 1, 1);
    [SerializeField] private Color mineralTile = new Color32(228, 148, 0, 255);
    private Material mat;
    private Color curColour;
    private float curFoggyness;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        InvokeRepeating(nameof(CheckColour), 0.1f, 0.5f);
    }

    private void CheckColour()
    {
        if (Tile.Building)
        {
            if (Tile.Building.GetComponent<Hub>() && curColour != shipTile)
            {
                mat.SetColor("_BaseColor", shipTile);
                curColour = shipTile;
            }
            else if (curColour != buildingTile)
            {
                mat.SetColor("_BaseColor", buildingTile);
                curColour = buildingTile;
            }
        }
        else if (!Tile.PowerSource)
        {
            if (curColour != unpoweredTile)
            {
                mat.SetColor("_BaseColor", unpoweredTile);
                curColour = unpoweredTile;
            }
        }
        else if (Tile.Resource)
        {
            if (curColour != mineralTile)
            {
                mat.SetColor("_BaseColor", mineralTile);
                curColour = mineralTile;
            }
        }
        else if (Tile.Collectible)
        {
            if (curColour != collectableTile)
            {
                mat.SetColor("_BaseColor", collectableTile);
                curColour = collectableTile;
            }
        }
        else if (Tile.PowerSource)
        {
            if (curColour != poweredTile)
            {
                mat.SetColor("_BaseColor", poweredTile);
                curColour = poweredTile;
            }
        }

        if (Tile.FogUnit)
        {
            float healthPer = Tile.FogUnit.Health / Tile.FogUnit.MaxHealth;
            if (curFoggyness != healthPer)
            {
                mat.SetFloat("_Foggyness", healthPer);
                curFoggyness = healthPer;
            }
        }
        else if (curFoggyness != 0)
        {
            mat.SetFloat("_Foggyness", 0);
            curFoggyness = 0;
        }
    }
}
