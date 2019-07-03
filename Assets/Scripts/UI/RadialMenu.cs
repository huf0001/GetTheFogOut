using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private float radius = 100;
    [SerializeField] private float offset = 0;
    [SerializeField] private float restrictVal = 1;

    private void Start()
    {
        offset = offset * (Mathf.PI / 180);
        for (int i = 0; i < transform.childCount; i++)
        {
            float angle = i * (Mathf.PI * 2) / transform.childCount / restrictVal;
            transform.GetChild(i).localPosition = new Vector3(Mathf.Cos(offset + angle) * radius, Mathf.Sin(offset + angle) * radius, 0) - new Vector3(9.7f, 0);
        }
    }
}
