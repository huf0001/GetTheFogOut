using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class DialogueBoxController : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Objective UI Elements")]
    [SerializeField] GameObject objectiveWindow;
    [SerializeField] GameObject objectiveWindowOpenArrows;
    [SerializeField] protected DialogueBox dialogueBox;

    //Non-Serialized fields
    protected bool dialogueRead = false;
    protected bool tileClicked = false;
    protected bool objWindowVisible = false;

    //Public Properties
    public bool ObjWindowVisible { get => objWindowVisible; set => objWindowVisible = value; }

    //Utility Methods--------------------------------------------------------------------------------------------------------------------------------

    //Passes dialogue to the DialogueBox
    protected virtual void SendDialogue(string dialogueKey, float invokeDelay)
    {
        //Pass dialogue to DialogueBox for it to display during its next update
        dialogueBox.SubmitDialogueSet(dialogueKey, invokeDelay);
    }

    //Called by btnTutorial to register that that button has been clicked
    public void RegisterMouseClicked()
    {
        tileClicked = true;
    }

    //Called by DialogueBox to register that dialogue has all been read
    public void RegisterDialogueRead()
    {
        dialogueRead = true;
    }

    //Resets the dialogueRead variable
    protected void ResetDialogueRead()
    {
        dialogueRead = false;
    }

    //Toggles visibility of the objective window
    public void ToggleObjWindow()
    {
        if (!objWindowVisible)
        {
            objectiveWindow.GetComponent<RectTransform>().DOAnchorPosY(205, 0.3f).SetEase(Ease.OutCubic);
            objectiveWindowOpenArrows.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 270), 0.3f);
            TutorialController.Instance.ObjWindowVisible = true;
            ObjectiveController.Instance.ObjWindowVisible = true;
        }
        else
        {
            objectiveWindow.GetComponent<RectTransform>().DOAnchorPosY(25, 0.3f).SetEase(Ease.InCubic);
            objectiveWindowOpenArrows.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 90), 0.3f);
            TutorialController.Instance.ObjWindowVisible = false;
            ObjectiveController.Instance.ObjWindowVisible = false;
        }
    }
}
