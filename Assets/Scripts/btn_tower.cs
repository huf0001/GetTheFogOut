using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class btn_tower : MonoBehaviour
{
    [SerializeField]
    private GameObject obj_prefab;
    public GameObject Obj_prefab { get => obj_prefab; }

    [SerializeField]
    private Button _button;
    private WorldController WC;
    public KeyCode _key;

    // Matt Germon: Test Code to see if moving the mouse over a button would trigger.
    // considering as a means of making "Tooltip" appear in the Canvas.
    //private bool IsMouseOver;

    // void Start()
    // {
    //     IsMouseOver = false;
    // }

    void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            _button.onClick.Invoke();
            WC.InBuildMode = true;
        }
    }

    // Matt Germon: Test Code to see if moving the mouse over a button would trigger.
    // considering as a means of making "Tooltip" appear in the Canvas.
    // void OnMouseOver()
    // {
    //     IsMouseOver = true;
    //     Debug.Log("Mouse is over the Button.");
    // }

    // void OnMouseExit()
    // {
    //     IsMouseOver = false;
    //     Debug.Log("Mouse is off the Button.");
    // }

    private void Awake()
    {
        _button = GetComponent<Button>();
        WC = FindObjectOfType<WorldController>();
    }

}
