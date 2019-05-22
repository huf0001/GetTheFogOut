using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraKey : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private float lerpOpaque = 0.8f;

    //Non-Serialized Fields
    private Button button;

    private bool lerping = false;
    private bool lerpForward = true;
    private float lerpProgress = 0;
    private float lerpMultiplier = 1;

    private bool lerpInCalled = false;
    private bool lerpOutCalled = false;
    private bool finished = false;

    //Public Properties

    public bool LerpInCalled { get => lerpInCalled; }
    public bool LerpOutCalled { get => lerpOutCalled; }
    public bool Finished { get => Finished; }

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    //Gets the attached button component
    private void Start()
    {
        button = GetComponent<Button>();
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

    //Calls other methods if lerping
    private void Update()
    {
        if (lerping)
        {
            UpdateLerpValues();
            Lerp();
        }
    }

    //Handles the lerp values
    private void UpdateLerpValues()
    {
        if (lerpForward)
        {
            lerpProgress += Time.deltaTime * lerpMultiplier;
        }
        else
        {
            lerpProgress -= Time.deltaTime * lerpMultiplier;
        }

        if (lerpProgress > 1)
        {
            lerpProgress = 1;
            lerpForward = false;
            lerping = false;
        }
        else if (lerpProgress < 0)
        {
            lerpProgress = 0;
            lerpForward = true;
            lerping = false;

            if (lerpOutCalled)
            {
                finished = true;
            }
        }
    }

    //Does the lerping
    private void Lerp()
    {
        ColorBlock cb = button.colors;
        Color nc = cb.normalColor;
        nc.a = Mathf.Lerp(0, lerpOpaque, lerpProgress);
        button.colors = cb;
    }

    //Utility Methods--------------------------------------------------------------------------------------------------------------------------------

    //Called by TutorialController to begin lerping the key's image in
    public void LerpIn()
    {
        lerping = true;
        lerpInCalled = true;
    }

    //Called by TutorialController to begin lerping the key's image out
    public void LerpOut()
    {
        lerping = true;
        lerpForward = false;
        lerpOutCalled = true;
    }
}
