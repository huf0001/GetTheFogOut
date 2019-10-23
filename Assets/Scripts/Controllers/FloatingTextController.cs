using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextController : MonoBehaviour
{
    [SerializeField] private FloatingText popupText;
    private GameObject canvas;
    private Camera cam;
    
    private void Start()
    {
        canvas = GameObject.Find("Warnings");
        cam = Camera.main;
    }

    public void CreateFloatingText(string text, Transform location)
    {
        FloatingText instance = Instantiate(popupText);

        Vector2 screenPosition = cam.WorldToScreenPoint(location.position);
        screenPosition.y += Screen.height / 13;
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.SetText(text);
    }
}
