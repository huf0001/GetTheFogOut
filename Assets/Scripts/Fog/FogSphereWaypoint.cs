using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FogSphereWaypoint : MonoBehaviour
{
    [SerializeField] private List<FogSphereWaypoint> nextWaypoints;
    [SerializeField] private bool renderLines;
    [SerializeField] private bool spawnPoint;
    [SerializeField] private bool hub;

    public bool SpawnPoint { get => spawnPoint; }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x);
        pos.z = Mathf.Round(pos.z);
        transform.position = pos;

        if (!hub)
        {
            if (spawnPoint)
            {
                name = $"FogSphereSpawnPoint({pos.x},{pos.z})";
            }
            else
            {
                name = $"FogSphereWaypoint({pos.x},{pos.z})";
            }
        }        

        if (renderLines)
        {
            foreach (FogSphereWaypoint w in nextWaypoints)
            {
                if (w != null)
                {
                    Debug.DrawLine(transform.position, Vector3.MoveTowards(transform.position, w.transform.position, 2f), Color.cyan);
                }
            }
        }        
    }

    public FogSphereWaypoint GetNextWaypoint()
    {
        if (nextWaypoints.Count == 0)
        {
            return null;
        }
        else if (nextWaypoints.Count == 1)
        {
            return nextWaypoints[0];
        }
        else
        {
            return nextWaypoints[UnityEngine.Random.Range(0, nextWaypoints.Count)];
        }
    }
}
