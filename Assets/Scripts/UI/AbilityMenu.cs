using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class AbilityMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup buttons;
    [SerializeField] private float radius = 150;

    private RadialMenu radialMenu;
    private Button toggleButton;
    private bool open;

    private void Start()
    {
        radialMenu = buttons.GetComponent<RadialMenu>();
        toggleButton = GetComponent<Button>();
    }

    public void ToggleMenu()
    {
        if (open) CloseMenu();
        else OpenMenu();
    }

    private void OpenMenu()
    {
        toggleButton.interactable = false;
        open = true;
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
        toggleButton.interactable = false;
        buttons.interactable = false;
        buttons.blocksRaycasts = false;
        open = false;
        DOTween.To(() => radialMenu.Radius, x => radialMenu.Radius = x, 0, 0.3f).SetEase(Ease.InBack).
            OnComplete(delegate
            {
                buttons.alpha = 0;
                toggleButton.interactable = true;
            });
    }
}
