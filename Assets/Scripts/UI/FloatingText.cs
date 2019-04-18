using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI damageText;

    public void SetText(string text)
    {
        damageText.text = text;
    }
}
