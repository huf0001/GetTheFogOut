using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private float radius = 100;
    [SerializeField] private float offset = 0;
    [SerializeField] private float restrictVal = 1;

    private List<float> angles = new List<float>();

    public float Radius
    {
        get => radius;
        set
        {
            if (radius == value) return;
            radius = value;
            AdjustPositions();
        }
    }

    private void Start()
    {
        offset = offset * (Mathf.PI / 180);
        for (int i = 0; i < transform.childCount; i++)
        {
            angles.Add(i * (Mathf.PI * 2) / transform.childCount / restrictVal);
            transform.GetChild(i).localPosition = new Vector3(Mathf.Cos(offset + angles[i]) * radius, Mathf.Sin(offset + angles[i]) * radius, 0) - new Vector3(9.7f, 0);
        }
    }

    private void AdjustPositions()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = new Vector3(Mathf.Cos(offset + angles[i]) * radius, Mathf.Sin(offset + angles[i]) * radius, 0) - new Vector3(9.7f, 0);
        }
    }
}
