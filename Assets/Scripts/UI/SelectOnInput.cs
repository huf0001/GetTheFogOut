using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SelectOnInput : MonoBehaviour {

    public EventSystem eventSystem;
    public GameObject selectedObject;

    private bool buttonSelected;

    void Start ()
    {
    	eventSystem.SetSelectedGameObject(selectedObject);
    }
    
    void Update () 
    {
        if ((Input.GetAxisRaw("Horizontal") != 0 || (Input.GetAxisRaw("Vertical") != 0) && buttonSelected == false))
        {
            eventSystem.SetSelectedGameObject(selectedObject);
            buttonSelected = true;
        }
    }

    private void OnDisable()
    {
        buttonSelected = false;
    }
}