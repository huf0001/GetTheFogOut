using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FogSphereWaypoint : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] private List<FogSphereWaypoint> nextWaypoints;
    [SerializeField] private bool spawnPoint;
    [SerializeField] private bool editing;

    //Public Properties
    public bool SpawnPoint { get => spawnPoint; }

    // Update is called once per frame
    void Update()
    {
        if (editing)
        {
            //Snap to grid
            Vector3 pos = transform.position;
            pos.x = Mathf.Round(pos.x);
            pos.z = Mathf.Round(pos.z);
            transform.position = pos;

            //Rename by position and type
            if (spawnPoint)
            {
                name = $"FogSphereSpawnPoint({transform.position.x},{transform.position.z})";
            }
            else
            {
                name = $"FogSphereWaypoint({transform.position.x},{transform.position.z})";
            }

            //Point towards next waypoint
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
