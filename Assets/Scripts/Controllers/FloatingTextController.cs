using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextController : MonoBehaviour
{
    [SerializeField] private FloatingText popupText;
    private GameObject canvas;
    private GameObject childtxt;

    private void Start()
    {
        canvas = GameObject.Find("TooltipCanvas");
    }

    public void CreateFloatingText(string text, Transform location)
    {
        FloatingText instance = Instantiate(popupText);

        childtxt = instance.gameObject.transform.GetChild(0).gameObject;
        RectTransform rt = childtxt.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(25, 10);

        RectTransform RT = instance.GetComponent<RectTransform>();
        RT.sizeDelta = new Vector2(0, 0);

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(location.position);
        screenPosition.y += 50;
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.SetText(text);
    }
}
