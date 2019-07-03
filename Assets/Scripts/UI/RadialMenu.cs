using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private float radius = 100;
    [SerializeField] private float offset = 0;

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            float angle = i * (Mathf.PI * 2) / transform.childCount;
            transform.GetChild(i).localPosition = new Vector3(Mathf.Cos(offset + angle) * radius, Mathf.Sin(offset + angle) * radius, 0);
        }
    }
}
