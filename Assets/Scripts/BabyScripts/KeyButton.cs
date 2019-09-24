using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Some code taken from https://answers.unity.com/questions/934527/ui-button-click-through-keyboardcode.html

[RequireComponent(typeof(Button))]
public class KeyButton : MonoBehaviour
{
    public Key key;
    public Button button {get; private set;}

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[key].wasPressedThisFrame && button.interactable && AbilityMenu.Instance.Visible)
        {
            button.onClick.Invoke();
        }
    }
}
