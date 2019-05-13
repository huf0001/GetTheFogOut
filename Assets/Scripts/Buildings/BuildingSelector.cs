using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingSelector : MonoBehaviour
{
    //[SerializeField] TextMeshProUGUI buildingDesc;
    [SerializeField] bool visible = false;
    private RectTransform parent;

    public bool Visible { get => visible; set => visible = value; }

    void Start()
    {
        parent = GetComponentInParent<RectTransform>();
    }

    void Update()
    {
        if (visible)
        {
            parent.LookAt(Camera.main.transform);
        }
    }

    public void ToggleVisibility()
    {
        if (gameObject.activeSelf)
        {
            btnTutorial[] buttons = GetComponentsInChildren<btnTutorial>();

            foreach (btnTutorial b in buttons)
            {
                if (b.Lerping)
                {
                    b.DeactivateLerping();
                }
            }
        }

        gameObject.SetActive(!gameObject.activeSelf);
        visible = !visible;
        //buildingDesc.gameObject.SetActive(!buildingDesc.gameObject.activeSelf);
    }
}
