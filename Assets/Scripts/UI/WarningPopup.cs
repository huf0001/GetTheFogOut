using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningPopup : MonoBehaviour
{
    Transform camTarget;

    public Building Building { get; set; }

    void Start()
    {
        if (Building != null)
        {
            GetComponent<Button>().onClick.AddListener(MoveToBuilding);
        }
    }

    void MoveToBuilding()
    {
        camTarget = GameObject.Find("CameraTarget").transform;
        camTarget.position = new Vector3(Building.Location.X, camTarget.position.y, Building.Location.Z);
    }
}
