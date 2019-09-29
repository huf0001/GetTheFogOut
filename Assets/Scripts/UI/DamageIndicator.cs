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
    [SerializeField] private TextMeshProUGUI exclamationMark;
    [SerializeField] private Gradient damageGradient;
    [SerializeField] private Image silhouette;
    [SerializeField] private Sprite[] silhouettes;

    private RectTransform rectTransform;
    private Rect screen;
    private CanvasGroup canvasGroup;
    private bool on = true;
    private Transform camTarget;
    private Camera cam;
    private Image icon;

    public Locatable Locatable { get; set; }
    public bool On
    {
        get => on;
        set
        {
            on = value;

            if (!value)
            {
                canvasGroup.alpha = 0;
                CancelInvoke(nameof(ChangeSprite));
            }
            else if (Locatable as Building)
            {
                InvokeRepeating(nameof(ChangeSprite), 0.5f, 0.5f);
            }
        }
    }

    public Color32 Colour
    {
        set
        {
            icon.color = value;
            exclamationMark.color = value;
            silhouette.color = value;
        }
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        screen = new Rect(0, 0, Screen.width, Screen.height);
        canvasGroup = GetComponent<CanvasGroup>();
        icon = GetComponent<Image>();
        cam = Camera.main;

        Building build = Locatable as Building;
        switch (build?.BuildingType)
        {
            case BuildingType.AirCannon:
                silhouette.sprite = silhouettes[2];
                break;
            case BuildingType.Extender:
                silhouette.sprite = silhouettes[4];
                break;
            case BuildingType.FogRepeller:
                silhouette.sprite = silhouettes[3];
                break;
            case BuildingType.Generator:
                silhouette.sprite = silhouettes[0];
                break;
            case BuildingType.Harvester:
                silhouette.sprite = silhouettes[1];
                break;
            case BuildingType.Hub:
                silhouette.sprite = silhouettes[5];
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        if (On)
        {
            Vector3 lookAtPos = Camera.main.WorldToScreenPoint(Locatable.transform.position);

            if (Locatable as Building)
            {
                rectTransform.position = lookAtPos;
                ClampIcon(new Vector2(0, 50));
                RotateIcon(lookAtPos);

                if (canvasGroup.alpha == 0)
                {
                    canvasGroup.alpha = 1;
                }
                Colour = damageGradient.Evaluate(((Building)Locatable).Health / ((Building)Locatable).MaxHealth);
            }
            else if (!screen.Contains(lookAtPos))
            {
                rectTransform.position = lookAtPos;
                ClampIcon();
                RotateIcon(lookAtPos);

                if (canvasGroup.alpha == 0)
                {
                    canvasGroup.alpha = 1;
                }
            }
            else if (canvasGroup.alpha == 1)
            {
                canvasGroup.alpha = 0;
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
        silhouette.rectTransform.rotation = Quaternion.identity;
    }

    private void ChangeSprite()
    {
        if (silhouette.enabled)
        {
            silhouette.enabled = false;
            exclamationMark.enabled = true;
        }
        else
        {
            silhouette.enabled = true;
            exclamationMark.enabled = false;
        }
    }
}
