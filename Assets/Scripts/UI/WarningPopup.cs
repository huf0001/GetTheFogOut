using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningPopup : MonoBehaviour
{
    private Transform camTarget;
    private RectTransform rectTransform;
    private int index;
    private bool indexChanged = false;
    private float initialYPos;

    public Building Building { get; set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialYPos = rectTransform.anchoredPosition.y;
    }

    void Start()
    {
        if (Building != null)
        {
            GetComponent<Button>().onClick.AddListener(MoveToBuilding);
        }
        InvokeRepeating("UpdateYPos", 0.2f, 0.2f);
    }

    void MoveToBuilding()
    {
        camTarget = GameObject.Find("CameraTarget").transform;
        camTarget.position = new Vector3(Building.Location.X, camTarget.position.y, Building.Location.Z);
    }

    void UpdateYPos()
    {
        if (indexChanged)
        {
            indexChanged = false;
            rectTransform.DOAnchorPosY(initialYPos - ((index + 1) * (Screen.height / 40)), 0.1f);
        }
    }

    public int Index
    {
        set
        {
            index = value;
            indexChanged = true;
        }
    }
}
