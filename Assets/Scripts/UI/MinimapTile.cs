using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MinimapTile : MonoBehaviour
{
    public TileData Tile { get; set; }

    private Color poweredTile = new Color(0, 0, 1);
    private Color unpoweredTile = new Color(0.7f, 0.3f, 0.3f);
    private Material mat;
    private Color curColour;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        InvokeRepeating(nameof(CheckColour), 0.1f, 0.5f);
    }

    private void CheckColour()
    {
        if (Tile.PowerSource && curColour != poweredTile)
        {
            mat.SetColor("_BaseColor", poweredTile);
            curColour = poweredTile;
        }
        else if (!Tile.PowerSource && curColour != unpoweredTile)
        {
            mat.SetColor("_BaseColor", unpoweredTile);
            curColour = unpoweredTile;
        }
    }
}
