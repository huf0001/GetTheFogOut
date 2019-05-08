using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DialogueBox : MonoBehaviour
{
    //Serialized Fields
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] DialogueBoxController dialogueBoxController;
    [SerializeField] float popUpSpeed = 0.5f;

    //Non-Serialized Fields
    private List<string> textToDisplay = new List<string>();
    private Vector2 originalRectTransformPosition;
    private RectTransform dialogueRectTransform;
    private bool activated = false;
    private string currentText = "";
    private int lerpTextIndex = 0;
    private int lerpTextInterval = 3;

    //Public Properties
    public bool Activated { get => activated; }
    public int DialogueCount { get => textToDisplay.Count; }

    private void Update()
    {
        if (textBox.text != currentText)
        {
            textBox.text = currentText.Substring(0, lerpTextIndex);
            lerpTextIndex += lerpTextInterval;

            if (lerpTextIndex > currentText.Length)
            {
                lerpTextIndex = currentText.Length;
            }
        }
    }

    public void ActivateDialogueBox(string text, float invokeDelay)
    {
        List<string> texts = new List<string>();
        texts.Add(text);
        ActivateDialogueBox(texts, invokeDelay);
    }

    public void ActivateDialogueBox(List<string> texts, float invokeDelay)
    {
        if (texts.Count > 0)
        {
            activated = true;

            //Caches required tweening information for performance saving
            dialogueRectTransform = GetComponent<RectTransform>();
            originalRectTransformPosition = GetComponent<RectTransform>().anchoredPosition;

            //Debug.Log("DialogueBoxActivated");
            textToDisplay = new List<string>(texts);

            Invoke("ShowDialogueBox", invokeDelay);
        } else
        {
            Debug.LogError("No text to display in dialogue box");
        }
    }

    private void DisplayNext()
    {
        textBox.text = textToDisplay[0];
        currentText = textToDisplay[0];
        textToDisplay.Remove(textToDisplay[0]);
    }

    private void LerpNext()
    {
        lerpTextIndex = 0;
        textBox.text = "";
        currentText = textToDisplay[0];
        textToDisplay.Remove(textToDisplay[0]);
    }

    private void ShowDialogueBox()
    {
        DisplayNext();
        dialogueRectTransform.DOAnchorPosY(originalRectTransformPosition.y - 100f, popUpSpeed).From(true).SetEase(Ease.OutBack).SetUpdate(true);
        gameObject.SetActive(true);
    }

    public void RegisterDialogueRead()
    {
        if (textToDisplay.Count > 0)
        {
            LerpNext();
        }
        else if (activated)
        {
            DeactivateDialogueBox();
        }
    }

    private void DeactivateDialogueBox()
    {
        dialogueRectTransform.DOAnchorPosY(originalRectTransformPosition.y - 100f, popUpSpeed).SetEase(Ease.InBack).SetUpdate(true).OnComplete(
            delegate 
            {
                //Reset position after tweening
                dialogueRectTransform.anchoredPosition = originalRectTransformPosition;
                gameObject.SetActive(false);
                textBox.text = "";
                dialogueBoxController.RegisterDialogueRead();
                activated = false;
            });
    }

    //public void ReactivateDialogueBox(string text, float invokeDelay)
    //{
    //    List<string> texts = new List<string>();
    //    texts.Add(text);
    //    ReactivateDialogueBox(texts, invokeDelay);
    //}

    //public void ReactivateDialogueBox(List<string> texts, float invokeDelay)
    //{
    //    Debug.Log("Reactivating DialogueBox");
    //    DeactivateDialogueBox();

    //    if (texts.Count > 0)
    //    {
    //        activated = true;

    //        //Caches required tweening information for performance saving
    //        dialogueRectTransform = GetComponent<RectTransform>();
    //        originalRectTransformPosition = GetComponent<RectTransform>().anchoredPosition;

    //        //Debug.Log("DialogueBoxActivated");
    //        textToDisplay = new List<string>(texts);

    //        Invoke("ShowDialogueBox", invokeDelay);
    //    }
    //    else
    //    {
    //        Debug.LogError("No text to display in dialogue box");
    //    }
    //}

    public void ChangeDialogue(List<string> texts)
    {
        textToDisplay = new List<string>(texts);
        LerpNext();
    }
}
