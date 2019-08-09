using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFogTile : MonoBehaviour
{
    private Material mat;
    private Color curColour;

    public TileData Tile { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetColor("_BaseColor", new Color(0, 0, 0, 0));
        curColour = new Color(0, 0, 0, 0);
        InvokeRepeating(nameof(CheckFogUnit), 0.1f, 0.5f);
    }

    private void CheckFogUnit()
    {
        FogUnit fogUnit = Tile.FogUnit;
        if (fogUnit)
        {
            float fogHealth = fogUnit.Health * 0.5f;
            if (curColour.a != fogHealth)
            {
                Color newColour = new Color(0.5f, 0.5f, 0.5f, fogHealth / fogUnit.MaxHealth);
                mat.SetColor("_BaseColor", newColour);
                curColour = newColour;
            }
        }
        else if (curColour.a != 0)
        {
            mat.SetColor("_BaseColor", new Color(0.5f, 0.5f, 0.5f, 0));
            curColour.a = 0;
        }
    }
}
