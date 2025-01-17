﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;

public class BuildingSelector : MonoBehaviour
{
    [SerializeField] private bool visible = false;
    [SerializeField] private float radius = 65;

    private CanvasGroup selectParent;
    private RadialMenu radialMenu;
    private Camera cam;
    private RectTransform rectTransform;

    public bool Visible { get => visible; set => visible = value; }
    public TileData CurrentTile { get; set; }

    private void Start()
    {
        radialMenu = GetComponentInChildren<RadialMenu>();
        selectParent = GetComponent<CanvasGroup>();
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (visible)
        {
            rectTransform.position = cam.WorldToScreenPoint(new Vector3(CurrentTile.X, 0, CurrentTile.Z));// + new Vector3(-Screen.width / 100, Screen.height / 25);
            Vector2 rect = rectTransform.anchoredPosition;
            rect += new Vector2(11, 5);
            rectTransform.anchoredPosition = rect;
        }
    }

    public void ToggleVisibility()
    {
        if (visible) CloseMenu();
        else OpenMenu();
    }

    private void OpenMenu()
    {
        DOTween.Kill("BuildMenu");
        visible = true;
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-UI_Select", GetComponent<Transform>().position);
        selectParent.alpha = 1;
        DOTween.To(() => radialMenu.Radius, x => radialMenu.Radius = x, radius, 0.3f).SetId("BuildMenu").SetEase(Ease.OutBack).OnComplete(
            delegate
            {
                selectParent.interactable = true;
                selectParent.blocksRaycasts = true;
            });
    }

    private void CloseMenu()
    {
        DOTween.Kill("BuildMenu");
        visible = false;
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-UI_Back", GetComponent<Transform>().position);
        selectParent.interactable = false;
        selectParent.blocksRaycasts = false;
        DOTween.To(() => radialMenu.Radius, x => radialMenu.Radius = x, 0, 0.3f).SetId("BuildMenu").SetEase(Ease.InBack).OnComplete(
            delegate
            {
                selectParent.alpha = 0;
            });
    }

    public void QuickCloseMenu()
    {
        DOTween.Kill("BuildMenu");
        visible = false;
        selectParent.interactable = false;
        selectParent.blocksRaycasts = false;
        radialMenu.Radius = 0;
        selectParent.alpha = 0;
    }

    public void freezeCam()
    {
        CinemachineVirtualCamera CMVcam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
        float FreqGain = CMVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain;
        float AmpGain = CMVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain;
        float tparam = 0f;
        float speed = 1f;

        if (tparam < 1)
        {
            tparam += Time.deltaTime * speed;
        }
        if (visible)
        {
            CMVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = Mathf.Lerp(FreqGain, 0f, tparam);
            CMVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = Mathf.Lerp(AmpGain, 0f, tparam);
        }
        else
        {
            CMVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = Mathf.Lerp(FreqGain, 0.4f, tparam);
            CMVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = Mathf.Lerp(AmpGain, 0.4f, tparam);
        }
    }
}
