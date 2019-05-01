using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] private int xAndYRange;
    [SerializeField] private Texture2D FogTexture;

    //Non-Serialized Fields
    private static Color[] _colors;
    private static int lowerXAndYRange;
    private static int higherXAndYRange;

    void Start ()
    {
        _colors = FogTexture.GetPixels();

        higherXAndYRange = xAndYRange;
        lowerXAndYRange = xAndYRange - 1;

        for (int i = 0; i < higherXAndYRange * higherXAndYRange; i++)
        {
            _colors[i] = Color.white;
        }
    }

    public static void UpdatePosition(Vector3 position, int circleSize)
    {
        Vector2 hole = new Vector2Int((int)position.x, (int)position.z);
        hole.x = (int) Mathf.Clamp(hole.x, 0f, lowerXAndYRange);
        hole.y = (int) Mathf.Clamp(hole.y, 0f, lowerXAndYRange);

        for (int x = -circleSize; x < circleSize; x++)
        {
            for (int y = -circleSize; y < circleSize; y++)
            {
                if(Mathf.Sqrt(x*x+y*y) <= circleSize)
                {
                    int xPos = (int)Mathf.Clamp(hole.x + x, 0f, lowerXAndYRange);
                    int yPos = (int)Mathf.Clamp(hole.y + y, 0f, lowerXAndYRange);
                    _colors[xPos + yPos * higherXAndYRange] = Color.black;
                }
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < higherXAndYRange * higherXAndYRange; i++)
        {
            Color c = _colors[i];
            c.r = c.r * (1f - 0.2f * Time.deltaTime);
            _colors[i] = c;
        }

        FogTexture.SetPixels(_colors);
        FogTexture.Apply();
    }
}
