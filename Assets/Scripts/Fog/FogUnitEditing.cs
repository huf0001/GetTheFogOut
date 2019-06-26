using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FogUnitEditing : MonoBehaviour
{
    [SerializeField] private bool reposition = false;
    [SerializeField] private bool randomiseRotation = false;
    [SerializeField] private bool rename = false;
    [SerializeField] private bool snapToGrid = false;

    [SerializeField] private int xChange = 0;
    [SerializeField] private int yChange = 0;
    [SerializeField] private int zChange = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        if (enabled)
        {
            enabled = false;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (reposition)
        {
            reposition = false;
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x + xChange, pos.y + yChange, pos.z + zChange);
            
            xChange = 0;
            yChange = 0;
            zChange = 0;
        }

        if (randomiseRotation)
        {
            randomiseRotation = false;
            Quaternion rot = transform.rotation;
            transform.SetPositionAndRotation(transform.position, Quaternion.Euler(rot.eulerAngles.x, Random.Range(0, 360), rot.eulerAngles.z));
        }

        if (rename)
        {
            rename = false;
            name = $"FogUnit ({transform.position.x}, {transform.position.z})";
        }

        if (snapToGrid)
        {
            snapToGrid = false;
            Vector3 pos = transform.position;
            transform.position = new Vector3(Mathf.RoundToInt(pos.x), pos.y, Mathf.RoundToInt(pos.z));
        }
    }
}
