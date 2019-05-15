﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cinemachine;
public class BuildingSelector : MonoBehaviour
{
    //[SerializeField] TextMeshProUGUI buildingDesc;
    [SerializeField] bool visible = false;
    private RectTransform parent;

    public bool Visible { get => visible; set => visible = value; }

    void Start()
    {
        parent = GetComponentInParent<RectTransform>();
    }

    void Update()
    {
        if (visible)
        {
            parent.LookAt(Camera.main.transform);
        }
    }

    public void ToggleVisibility()
    {
        if (gameObject.activeSelf)
        {
            btnTutorial[] buttons = GetComponentsInChildren<btnTutorial>();

            foreach (btnTutorial b in buttons)
            {
                if (b.Lerping)
                {
                    b.DeactivateLerping();
                }
            }
        }

        gameObject.SetActive(!gameObject.activeSelf);
        visible = !visible;

        if (visible)
        {
            freezeCam(0f, 0f);
        }
        else
            freezeCam(0.4f, 0.4f);

        //buildingDesc.gameObject.SetActive(!buildingDesc.gameObject.activeSelf);
    }

    public void freezeCam(float freq, float Amp)
    {
        CinemachineVirtualCamera CMVcam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
        CMVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = freq;
        CMVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = Amp;
    }
}
