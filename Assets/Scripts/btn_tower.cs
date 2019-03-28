using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btn_tower : MonoBehaviour
{
    [SerializeField]
    private GameObject obj_prefab;

    public GameObject Obj_prefab { get => obj_prefab; }

    public KeyCode _key;
    private Button _button;



    void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            _button.onClick.Invoke();
            //   changeColor();
        }

    }

    private void Awake()
    {
        _button = GetComponent<Button>();
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
