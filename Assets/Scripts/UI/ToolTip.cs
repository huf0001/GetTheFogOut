using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToolTip : MonoBehaviour
{
    TextMeshProUGUI toolTipText;
    [SerializeField] Vector3 offset;

    void Start()
    {
        offset = new Vector3(-GetComponent<RectTransform>().sizeDelta.x / 2, 5, 0);
        toolTipText = GetComponentInChildren<TextMeshProUGUI>();
        StartCoroutine(DisableToolTip());
    }

    void Update()
    {
    }

    IEnumerator DisableToolTip()
    {
        yield return new WaitForSeconds(0.0001f);
        gameObject.SetActive(false);
    }

    public void UpdateText(PlaneObject obj)
    {
        transform.position = Camera.main.WorldToScreenPoint(obj.transform.position) + offset;

        toolTipText.text = obj.name;
    }
}
