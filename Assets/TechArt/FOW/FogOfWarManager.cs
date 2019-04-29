using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    public Texture2D FogTexture;
    public GameObject testobj;
    private Color[] _colors;
    public int circleSizes = 4;
    void Start ()
    {
        _colors = FogTexture.GetPixels();
        for (int i = 0; i < 32 * 32; i++)
        {
            _colors[i] = Color.black;
        }
    }

    public void UpdatePosition(Vector3 position, int circleSize)
    {
        Vector2 hole = new Vector2Int((int)position.x, (int)position.z);
        hole.x = (int) Mathf.Clamp(hole.x, 0f, 31f);
        hole.y = (int) Mathf.Clamp(hole.y, 0f, 31f);
        for (int x = -circleSize; x < circleSize; x++)
        {
            for (int y = -circleSize; y < circleSize; y++)
            {
                if(Mathf.Sqrt(x*x+y*y) <= circleSize)
                {
                    int xPos = (int)Mathf.Clamp(hole.x + x, 0f, 31f);
                    int yPos = (int)Mathf.Clamp(hole.y + y, 0f, 31f);
                    _colors[xPos + yPos * 32] = Color.white;
                }
            }
        }
    }

    void Update()
    {
        UpdatePosition(testobj.transform.position, circleSizes);
        for (int i = 0; i < 32 * 32; i++)
        {
            Color c = _colors[i];
            c.r = c.r * (1f - 0.2f * Time.deltaTime);
            _colors[i] = c;
        }

        FogTexture.SetPixels(_colors);
        FogTexture.Apply();
    }
}
