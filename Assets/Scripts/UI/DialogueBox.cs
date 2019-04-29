using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueBox : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] DialogueBoxController dialogueBoxController;

    //Non-Serialized Fields
    private List<string> textToDisplay = new List<string>();

    public void ActivateDialogueBox(string text, float invokeDelay)
    {
        List<string> texts = new List<string>();
        texts.Add(text);
        ActivateDialogueBox(texts, invokeDelay);
    }

    public void ActivateDialogueBox(List<string> texts, float invokeDelay)
    {
        //Debug.Log("DialogueBoxActivated");
        textToDisplay.AddRange(texts);
        DisplayNext();

        Invoke("Pause", invokeDelay);
    }

    private void DisplayNext()
    {
        textBox.text = textToDisplay[0];
        textToDisplay.Remove(textToDisplay[0]);
    }

    private void Pause()
    {
        WorldController.Instance.SetPause(true);
        gameObject.SetActive(true);
    }

    public void RegisterDialogueRead()
    {
        if (textToDisplay.Count > 0)
        {
            DisplayNext();
        }
        else
        {
            dialogueBoxController.RegisterDialogueRead();
            DeactivateDialogueBox();
        }
    }

    private void DeactivateDialogueBox()
    {
        gameObject.SetActive(false);
        textBox.text = "";
        WorldController.Instance.SetPause(false);
    }
}
