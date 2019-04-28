using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btnTutorial : MonoBehaviour
{
    [SerializeField] private ButtonType buildingType;

    //Non-Serialized Fields
    private TutorialController tutorialController;
    private bool reportClick = false;
    private Button button;

    private bool lerping = false;
    private bool lerpForward = true;
    private float lerpProgress = 0;
    private float lerpMultiplier = 1;

    private Color uiNormalColour;
    private Color uiHighlightColour;

    //Public Properties
    public Button Button { get => button; }
    public bool Lerping { get => lerping;}
    public bool ReportClick { get => reportClick; set => reportClick = value; }

    private void Start()
    {
        button = GetComponent<Button>();
        tutorialController = WorldController.Instance.TutorialController;
        uiNormalColour = tutorialController.UINormalColour;
        uiHighlightColour = tutorialController.UIHighlightColour;
    }

    private void Update()
    {
        if (buildingType == tutorialController.CurrentlyLerping)
        {
            if (!lerping)
            {
                ActivateLerping();
            }

            LerpButtonColour();
        }
        else if (lerping)
        {
            DeactivateLerping();
        }
    }

    private void LerpButtonColour()
    {
        //lerp button's NormalColor's values
        ColorBlock cb = button.colors;
        cb.normalColor = Color.Lerp(uiNormalColour, uiHighlightColour, lerpProgress);
        button.colors = cb;

        UpdateLerpValues();
    }

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
        }
        else if (lerpProgress < 0)
        {
            lerpProgress = 0;
            lerpForward = true;
        }
    }

    private void ActivateLerping()
    {
        lerping = true;
        lerpProgress = 0;
        lerpForward = true;
    }

    public void DeactivateLerping()
    {
        ColorBlock cb = button.colors;
        cb.normalColor = uiNormalColour;
        button.colors = cb;

        lerping = false;
    }

    public void ReportClickToTutorialController()
    {
        if (reportClick)
        {
            Debug.Log(tutorialController);
            tutorialController.RegisterButtonClicked();
        }
    }
}
