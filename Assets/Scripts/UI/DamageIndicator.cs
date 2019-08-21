﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] private float leftEdgeBuffer = 300;
    [SerializeField] private float rightEdgeBuffer = 300;
    [SerializeField] private float topEdgeBuffer = 300;
    [SerializeField] private float bottomEdgeBuffer = 300;

    private RectTransform rectTransform;
    private Rect screen;
    private CanvasGroup icon;
    private TextMeshProUGUI exclamationMark;
    private bool on = true;
    private Transform camTarget;
    [HideInInspector][SerializeField] new Camera camera;

    public Building Building { get; set; }
    public bool On
    {
        get => on;
        set
        {
            on = value;
            if (!value)
            {
                icon.alpha = 0;
                icon.blocksRaycasts = false;
                icon.interactable = false;
            }
        }
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        screen = new Rect(0, 0, Screen.width, Screen.height);
        icon = GetComponent<CanvasGroup>();
        exclamationMark = GetComponentInChildren<TextMeshProUGUI>();
        camera = Camera.main;
    }

    private void Update()
    {
        if (On)
        {
            Vector3 lookAtPos = camera.WorldToScreenPoint(Building.transform.position);

            if (!screen.Contains(lookAtPos))
            {
                rectTransform.position = lookAtPos;
                Vector3 newPos = rectTransform.anchoredPosition;
                newPos.x = Mathf.Clamp(newPos.x, leftEdgeBuffer, 1280 - rightEdgeBuffer);
                newPos.y = Mathf.Clamp(newPos.y, bottomEdgeBuffer, 720 - topEdgeBuffer);
                rectTransform.anchoredPosition = newPos;

                Vector3 dir = -(lookAtPos - rectTransform.position).normalized;
                float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.AngleAxis(rotZ + 180, Vector3.forward);
                exclamationMark.rectTransform.rotation = Quaternion.identity;

                if (icon.alpha == 0)
                {
                    icon.alpha = 1;
                    icon.blocksRaycasts = true;
                    icon.interactable = true;
                }
            }
            else if (icon.alpha == 1)
            {
                icon.alpha = 0;
                icon.blocksRaycasts = false;
                icon.interactable = false;
            }
        }
    }

    public void MoveToBuilding()
    {
        if (!camTarget) camTarget = GameObject.Find("CameraTarget").transform;
        camTarget.DOMove(new Vector3(Building.Location.X, camTarget.position.y, Building.Location.Z), 0.3f);
    }
}
