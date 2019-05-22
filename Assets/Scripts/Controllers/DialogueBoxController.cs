using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public enum AIExpression
{
    None,
    Happy,
    Neutral,
    Sad
}

[Serializable]
public class ExpressionDialoguePair
{
    //Serialized Fields
    [SerializeField] private AIExpression aiExpression = AIExpression.Neutral;
    [SerializeField, TextArea(15, 20)] private string dialogue;

    //Public Properties
    public AIExpression AIExpression { get => aiExpression; }
    public string Dialogue { get => dialogue; }
}

[Serializable]
public class DialogueSet
{
    //Serialized Fields
    [SerializeField] private string key;
    [SerializeField] private List<ExpressionDialoguePair> expressionDialoguePair;

    //Public Properties
    public string Key { get => key; }
    public List<ExpressionDialoguePair> ExpressionDialoguePair { get => expressionDialoguePair; }
}

public class DialogueBoxController : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] GameObject objectiveWindow;
    [SerializeField] GameObject objectiveWindowOpenArrows;

    [SerializeField] protected DialogueBox aiText;
    [SerializeField] private List<DialogueSet> dialogue;

    //Non-Serialized fields
    protected bool dialogueRead = false;
    protected bool tileClicked = false;
    protected bool objWindowVisible = false;

    //Public Properties
    public bool ObjWindowVisible { get => objWindowVisible; set => objWindowVisible = value; }

    //Utility Methods--------------------------------------------------------------------------------------------------------------------------------

    //Retrieves dialogue from the list using the dialogue key
    private List<ExpressionDialoguePair> GetExpressionDialoguePairs(string key)
    {
        foreach (DialogueSet p in dialogue)
        {
            if (p.Key == key)
            {
                return p.ExpressionDialoguePair;
            }
        }

        Debug.Log("Dialogue key '" + key + "' is invalid.");
        return null;
    }

    //Passes dialogue to the DialogueBox
    protected virtual void SendDialogue(string dialogueKey, float invokeDelay)
    {
        //Activate DialogueBox, passing dialogue to it
        if (aiText.Activated)
        {
            aiText.ChangeDialogue(GetExpressionDialoguePairs(dialogueKey));
        }
        else
        {
            aiText.ActivateDialogueBox(GetExpressionDialoguePairs(dialogueKey), invokeDelay);
        }
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
            objectiveWindow.GetComponent<RectTransform>().DOAnchorPosX(5, 0.3f).SetEase(Ease.OutCubic);
            objectiveWindowOpenArrows.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 180), 0.3f);
            TutorialController.Instance.ObjWindowVisible = true;
            ObjectiveController.Instance.ObjWindowVisible = true;
        }
        else
        {
            objectiveWindow.GetComponent<RectTransform>().DOAnchorPosX(-250, 0.3f).SetEase(Ease.InCubic);
            objectiveWindowOpenArrows.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 0), 0.3f);
            TutorialController.Instance.ObjWindowVisible = false;
            ObjectiveController.Instance.ObjWindowVisible = false;
        }
    }
}
