using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[Serializable]
public class KeyDialoguePair
{
    //Serialized Fields
    [SerializeField] private string key;
    [SerializeField, TextArea] private List<string> dialogue;

    //Public Properties
    public string Key { get => key; }
    public List<string> Dialogue { get => dialogue; }

    public KeyDialoguePair(string k, List<string> d)
    {
        key = k;
        dialogue = d;
    }
}

public class DialogueBoxController : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] GameObject objectiveWindow;
    [SerializeField] GameObject objectiveWindowOpenArrows;

    [SerializeField] protected DialogueBox aiText;
    [SerializeField] private List<KeyDialoguePair> dialogue;

    //Non-Serialized fields
    protected bool instructionsSent = false;
    protected bool dialogueRead = false;
    protected bool tileClicked = false;

    protected bool objWindowVisible = false;

    public bool ObjWindowVisible { get => objWindowVisible; set => objWindowVisible = value; }

    public void RegisterDialogueRead()
    {
        dialogueRead = true;
    }

    protected void ResetDialogueRead()
    {
        dialogueRead = false;
    }

    protected virtual void SendDialogue(string dialogueKey, float invokeDelay)
    {
        if (!instructionsSent)
        {
            //Activate DialogueBox, passing dialogue to it
            if (aiText.Activated)
            {
                //aiText.ReactivateDialogueBox(GetDialogue(dialogueKey), invokeDelay);
                aiText.ChangeDialogue(GetDialogue(dialogueKey));
            }
            else
            {
                aiText.ActivateDialogueBox(GetDialogue(dialogueKey), invokeDelay);
            }

            //Set dialogueSent to true so that the dialogue box isn't being repeatedly activated
            instructionsSent = true;
        }
    }

    private List<string> GetDialogue(string key)
    {
        foreach (KeyDialoguePair p in dialogue)
        {
            if (p.Key == key)
            {
                return p.Dialogue;
            }
        }

        Debug.Log("Dialogue key '" + key + "' is invalid.");
        return null;
    }

    public void RegisterButtonClicked()
    {
        tileClicked = true;
    }

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
