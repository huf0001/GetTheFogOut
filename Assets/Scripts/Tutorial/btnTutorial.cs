using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btnTutorial : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private ButtonType buildingType;

    //Non-Serialized Fields
    private TutorialController tutorialController;
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

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    //Gets some necessary values
    private void Start()
    {
        button = GetComponent<Button>();
        tutorialController = TutorialController.Instance;
        uiNormalColour = tutorialController.UINormalColour;
        uiHighlightColour = tutorialController.UIHighlightColour;
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

    //Controls when lerping happens based on outside variables
    private void Update()
    {
        if (tutorialController.ButtonAllowed(buildingType) && tutorialController.TutorialStage != TutorialStage.Finished)
        {
            button.interactable = true;

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
        else if (tutorialController.TutorialStage != TutorialStage.Finished)
        {
            button.interactable = false;

            if (lerping)
            {
                DeactivateLerping();
            }
        }
    }

    //Lerps the button's colour
    private void LerpButtonColour()
    {
        ColorBlock cb = button.colors;
        cb.normalColor = Color.Lerp(uiNormalColour, uiHighlightColour, lerpProgress);
        button.colors = cb;

        UpdateLerpValues();
    }

    //Updates the lerp values
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

    //Edits variables ready to start up the lerping
    private void ActivateLerping()
    {
        lerping = true;
        lerpProgress = 0;
        lerpForward = true;
    }

    //Resets the lerped button back to normal
    public void DeactivateLerping()
    {
        ColorBlock cb = button.colors;
        cb.normalColor = uiNormalColour;
        button.colors = cb;

        lerping = false;
    }
}
