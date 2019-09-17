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
    [SerializeField] private Image silhouette;
    [SerializeField] private Sprite[] silhouettes;

    private RectTransform rectTransform;
    private Rect screen;
    private CanvasGroup icon;
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
            GetComponent<Image>().color = value;
            exclamationMark.color = value;
        }
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        screen = new Rect(0, 0, Screen.width, Screen.height);
        icon = GetComponent<CanvasGroup>();
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
            default:
                break;
        }
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
