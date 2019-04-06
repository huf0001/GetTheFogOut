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
    //     Debug.Log("Mouse is over the Button.");
    // }

    // void OnMouseExit()
    // {
    //     Debug.Log("Mouse is off the Button.");
    // }

    private void Awake()
    {
        _button = GetComponent<Button>();
        WC = FindObjectOfType<WorldController>();
    }
/*
 * //no longer needed -  unusable function - fly away objects
 * 
    public void GetObjInput()
    {
        if (Input.GetKeyDown(_key))
        {
            
            if (Replc_Obj == null)
            {
                Replc_Obj = Instantiate(Obj_prefab);
                temp = _key;
            }
            else
            {
                if (temp != _key)
                {
                    Destroy(Replc_Obj);
                    Replc_Obj = Instantiate(Obj_prefab);
                }
                temp = KeyCode.None;

            }
            _button.onClick.Invoke();
       //     _button.onClick.AddListener(MoveObjToMouse);

        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            TowerManager tm = FindObjectOfType<TowerManager>();
            tm.EscToCancel();
            Destroy(Replc_Obj);
        }
    }

    public void MoveObjToMouse()
    {
        RaycastHit hitinfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitinfo))
        {
            //Vector3 vec = new Vector3(Mathf.Round(hitinfo.point.x), Mathf.Round(hitinfo.point.y), Mathf.Round(hitinfo.point.z));
            Replc_Obj.transform.position = hitinfo.point;
        }
    }
    */


}
