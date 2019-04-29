using System.Collections;
using System.Collections.Generic;
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

    public void ActivateDialogueBox(string text, float invokeDelay)
    {
        List<string> texts = new List<string>();
        texts.Add(text);
        ActivateDialogueBox(texts, invokeDelay);
    }

    public void ActivateDialogueBox(List<string> texts, float invokeDelay)
    {
        //Caches required tweening information for performance saving
        dialogueRectTransform = GetComponent<RectTransform>();
        originalRectTransformPosition = GetComponent<RectTransform>().anchoredPosition;

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
        dialogueRectTransform.DOAnchorPosY(originalRectTransformPosition.y - 100f, popUpSpeed).From(true).SetEase(Ease.OutBack).SetUpdate(true);
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
                WorldController.Instance.SetPause(false);
                });
    }
}
