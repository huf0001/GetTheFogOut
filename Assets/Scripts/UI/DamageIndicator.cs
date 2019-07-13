using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    public Building Building { get; set; }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        screen = new Rect(0, 0, Screen.width, Screen.height);
        icon = GetComponent<CanvasGroup>();
        exclamationMark = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        Vector3 lookAtPos = Camera.main.WorldToScreenPoint(Building.transform.position);

        if (!screen.Contains(lookAtPos))
        {
            if (icon.alpha == 0)
            {
                icon.alpha = 1;
            }

            Vector3 newPos = lookAtPos;
            newPos.x = Mathf.Clamp(newPos.x, leftEdgeBuffer, Screen.width - rightEdgeBuffer);
            newPos.y = Mathf.Clamp(newPos.y, bottomEdgeBuffer, Screen.height - topEdgeBuffer);
            transform.position = newPos;

            Vector3 dir = -(lookAtPos - transform.position).normalized;
            float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.AngleAxis(rotZ + 180, Vector3.forward);
            exclamationMark.rectTransform.rotation = Quaternion.identity;
        }
        else if (icon.alpha == 1)
        {
            icon.alpha = 0;
        }
    }

    public void Enable()
    {
        // enable indicator when building damaged and has ref already
    }

    public void Disable()
    {
        // disable indicator when building no longer being damaged
    }
}
