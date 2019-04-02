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
    public static GameObject Replc_Obj;
    public KeyCode _key;

    private KeyCode temp;
    private Button _button;


   // private GameObject tmp;


    void Update()
    {
        // Debug.Log();

        if (Replc_Obj != null)
        {
            MoveObjToMouse();
        }
        
       GetObjInput();
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

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
    /*
      //TODO: change color on button pressed or clicked

    public void changeColor()
    {
        //  Debug.Log("Changing highlighed color");
        float r = 0.1f;
        float g = 0.4f;
        float b = 1f;
        ColorBlock colorVar = _button.colors;

        colorVar.pressedColor = new Color(r, g, b);
        colorVar.normalColor = new Color(r, g, b);
        colorVar.highlightedColor = new Color(r, g, b);
        _button.colors = colorVar;
    }

    public void changeColorToNormal()
    {
        float r = 1f;
        float g = 1f;
        float b = 1f;
        ColorBlock colorVar = _button.colors;

        colorVar.pressedColor = new Color(r, g, b);
        colorVar.normalColor = new Color(r, g, b);
        colorVar.highlightedColor = new Color(r, g, b);
        _button.colors = colorVar;
    }
    */



}
