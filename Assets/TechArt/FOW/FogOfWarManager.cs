using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    public Texture2D FogTexture;
    public GameObject testobj;
    private Color[] _colors;
    public int circleSize = 4;

    void Start ()
    {
        _colors = FogTexture.GetPixels();
        for (int i = 0; i < 64 * 64; i++)
        {
            _colors[i] = Color.black;
        }
    }

    public void UpdatePosition(Vector3 position)
    {
        Vector2 hole = new Vector2Int((int)position.x + 32, (int)position.z + 32);
        hole.x = (int) Mathf.Clamp(hole.x, 0f, 63f);
        hole.y = (int) Mathf.Clamp(hole.y, 0f, 63f);
        for (int x = -circleSize; x < circleSize; x++)
        {
            for (int y = -circleSize; y < circleSize; y++)
            {
                if(Mathf.Sqrt(x*x+y*y) <= circleSize)
                {
                    int xPos = (int)Mathf.Clamp(hole.x + x, 0f, 63f);
                    int yPos = (int)Mathf.Clamp(hole.y + y, 0f, 63f);
                    _colors[xPos + yPos * 64] = Color.white;
                }
            }
        }
    }

    void Update()
    {
        UpdatePosition(testobj.transform.position);
        for (int i = 0; i < 64 * 64; i++)
        {
            Color c = _colors[i];
            c.r = c.r * (1f - 0.2f * Time.deltaTime);
            _colors[i] = c;
        }

        FogTexture.SetPixels(_colors);
        FogTexture.Apply();
    }
}
