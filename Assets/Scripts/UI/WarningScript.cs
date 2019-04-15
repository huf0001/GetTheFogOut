using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningScript : MonoBehaviour
{
    Image tint;
    State state = State.Normal;

    // Start is called before the first frame update
    void Start()
    {
        tint = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public enum State
    {
        Normal,
        Warning,
        Danger
    }
    
    public void ChangeState(State s)
    {
        switch (s)
        {
            case State.Normal:
                tint.color = new Color32(63, 63, 63, 255);
                state = s;
                break;
            case State.Warning:
                tint.color = new Color32(240, 176, 64, 255);
                state = s;
                break;
            case State.Danger:
                tint.color = new Color32(200, 0, 0, 255);
                state = s;
                break;
        }
    }
}
