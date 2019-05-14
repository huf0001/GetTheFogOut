using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] protected DialogueBox aiText;
    [SerializeField] private List<KeyDialoguePair> dialogue;

    //Non-Serialized fields
    protected bool instructionsSent = false;
    protected bool dialogueRead = false;
    protected bool buttonClicked = false;

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
        buttonClicked = true;
    }
}
