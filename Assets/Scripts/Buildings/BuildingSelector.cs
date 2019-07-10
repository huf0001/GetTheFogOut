using System.Collections;
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

    public bool Visible { get => visible; set => visible = value; }
    public TileData CurrentTile { get; set; }

    private void Start()
    {
        radialMenu = GetComponentInChildren<RadialMenu>();
        selectParent = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (visible)
        {
            transform.position = Camera.main.WorldToScreenPoint(new Vector3(CurrentTile.X, 0, CurrentTile.Z)) + new Vector3(-Screen.width / 100, Screen.height / 25);
        }
    }

    public void ToggleVisibility()
    {
        if (visible) CloseMenu();
        else OpenMenu();

        //gameObject.SetActive(!gameObject.activeSelf);
        //visible = !visible;

        //    if (visible)
        //   {
        //      freezeCam(0f, 0f);
        //  }
        // else
        //     freezeCam(0.4f, 0.4f);

        //buildingDesc.gameObject.SetActive(!buildingDesc.gameObject.activeSelf);
    }

    private void OpenMenu()
    {
        selectParent.alpha = 1;
        DOTween.To(() => radialMenu.Radius, x => radialMenu.Radius = x, radius, 0.3f).SetEase(Ease.OutBack).
            OnComplete(delegate
            {
                visible = true;
                selectParent.interactable = true;
                selectParent.blocksRaycasts = true;
            });
    }

    private void CloseMenu()
    {
        btnTutorial[] buttons = GetComponentsInChildren<btnTutorial>();

        foreach (btnTutorial b in buttons)
        {
            if (b.Lerping)
            {
                b.DeactivateLerping();
            }
        }

        selectParent.interactable = false;
        selectParent.blocksRaycasts = false;
        DOTween.To(() => radialMenu.Radius, x => radialMenu.Radius = x, 0, 0.3f).SetEase(Ease.InBack).
            OnComplete(delegate
            {
                visible = false;
                selectParent.alpha = 0;
            });
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
