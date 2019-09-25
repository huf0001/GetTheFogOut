using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class AbilityMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup buttons;
    [SerializeField] private float radius = 150;
    private static AbilityMenu instance;
    private RadialMenu radialMenu;
    private Button toggleButton;
    public bool Visible { get; set; }

    private void Start()
    {
        radialMenu = buttons.GetComponent<RadialMenu>();
        toggleButton = GetComponent<Button>();
    }
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more ability controllers.");
        }
        Instance = this;
    }
    public static AbilityMenu Instance
    {
        get { return instance; }
        set { instance = value; }
    }

    public void ToggleMenu()
    {
        if (Visible)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-UI_Back", GetComponent<Transform>().position);
            CloseMenu();
        }
        else
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-UI_Select", GetComponent<Transform>().position);
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        toggleButton.interactable = false;
        Visible = true;
        buttons.alpha = 1;
        DOTween.To(() => radialMenu.Radius, x => radialMenu.Radius = x, radius, 0.3f).SetEase(Ease.OutBack).
            OnComplete(delegate
            {
                buttons.interactable = true;
                buttons.blocksRaycasts = true;
                toggleButton.interactable = true;
            });
    }

    private void CloseMenu()
    {
        if (AbilityController.Instance.IsAbilitySelected)
        {
            AbilityController.Instance.CancelAbility();
        }
        toggleButton.interactable = false;
        buttons.interactable = false;
        buttons.blocksRaycasts = false;
        Visible = false;
        DOTween.To(() => radialMenu.Radius, x => radialMenu.Radius = x, 0, 0.3f).SetEase(Ease.InBack).
            OnComplete(delegate
            {
                buttons.alpha = 0;
                toggleButton.interactable = true;
            });
        if (AbilityController.Instance.AbilityDescGO.activeSelf)
        {
            AbilityController.Instance.AbilityDescGO.SetActive(false);
        }
    }
}
