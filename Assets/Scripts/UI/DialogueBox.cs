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
    private bool deactivated = true;

    //Public Properties
    public int DialogueCount { get => textToDisplay.Count; }

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
            deactivated = false;

            //Caches required tweening information for performance saving
            dialogueRectTransform = GetComponent<RectTransform>();
            originalRectTransformPosition = GetComponent<RectTransform>().anchoredPosition;

            //Debug.Log("DialogueBoxActivated");
            textToDisplay = new List<string>(texts);
            DisplayNext();

            Invoke("ShowDialogueBox", invokeDelay);
        } else
        {
            Debug.LogError("No text to display in dialogue box");
        }
    }

    private void DisplayNext()
    {
        textBox.text = textToDisplay[0];
        textToDisplay.Remove(textToDisplay[0]);
    }

    private void ShowDialogueBox()
    {
        //WorldController.Instance.SetPause(true);
        dialogueRectTransform.DOAnchorPosY(originalRectTransformPosition.y - 100f, popUpSpeed).From(true).SetEase(Ease.OutBack).SetUpdate(true);
        gameObject.SetActive(true);
    }

    public void RegisterDialogueRead()
    {
        if (textToDisplay.Count > 0)
        {
            DisplayNext();
        }
        else if (!deactivated)
        {
            deactivated = true;
            DeactivateDialogueBox();
        }
    }

    private void DeactivateDialogueBox()
    {
        dialogueRectTransform.DOAnchorPosY(originalRectTransformPosition.y - 100f, popUpSpeed).SetEase(Ease.InBack).SetUpdate(true).OnComplete(
            delegate {
                //Reset position after tweening
                dialogueRectTransform.anchoredPosition = originalRectTransformPosition;
                gameObject.SetActive(false);
                textBox.text = "";
                dialogueBoxController.RegisterDialogueRead();
                //WorldController.Instance.SetPause(false);
                });
    }
}
