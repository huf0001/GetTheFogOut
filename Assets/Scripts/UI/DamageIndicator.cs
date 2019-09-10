using System.Collections;
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
    private Camera cam;
    public Locatable Locatable { get; set; }
    public bool On
    {
        get => on;
        set
        {
            on = value;

            if (!value)
            {
                icon.alpha = 0;
            }
        }
    }

    public Color32 Colour
    {
        set
        {
            GetComponent<Image>().color = value;
            exclamationMark = GetComponentInChildren<TextMeshProUGUI>();
            exclamationMark.color = value;
        }
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        screen = new Rect(0, 0, Screen.width, Screen.height);
        icon = GetComponent<CanvasGroup>();
        if (!exclamationMark) exclamationMark = GetComponentInChildren<TextMeshProUGUI>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (On)
        {
            Vector3 lookAtPos = Camera.main.WorldToScreenPoint(Locatable.transform.position);

            if (!screen.Contains(lookAtPos))
            {
                rectTransform.position = lookAtPos;
                ClampIcon();
                RotateIcon(lookAtPos);

                if (icon.alpha == 0)
                {
                    icon.alpha = 1;
                }
            }
            else if (Locatable as Building)
            {
                rectTransform.position = lookAtPos;
                ClampIcon(new Vector2(0, 50));
                RotateIcon(lookAtPos);

                if (icon.alpha == 0)
                {
                    icon.alpha = 1;
                }
            }
            else if (icon.alpha == 1)
            {
                icon.alpha = 0;
            }
        }
    }

    private void ClampIcon(Vector2 adjustment = new Vector2())
    {
        Vector3 newPos = rectTransform.anchoredPosition + adjustment;
        newPos.x = Mathf.Clamp(newPos.x, leftEdgeBuffer, 1280 - rightEdgeBuffer);
        newPos.y = Mathf.Clamp(newPos.y, bottomEdgeBuffer, 720 - topEdgeBuffer);
        rectTransform.anchoredPosition = newPos;
    }

    private void RotateIcon(Vector3 lookAtPos)
    {
        Vector3 dir = -(lookAtPos - rectTransform.position).normalized;
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.AngleAxis(rotZ + 180, Vector3.forward);
        exclamationMark.rectTransform.rotation = Quaternion.identity;
    }
}
