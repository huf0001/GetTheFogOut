using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DialogueBox : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] DialogueBoxController dialogueBoxController;
    [SerializeField] float popUpSpeed = 0.5f;
    [SerializeField] private int lerpTextInterval = 3;

    [SerializeField] private string squareBracketColour;
    [SerializeField] private string squigglyBracketColour;
    [SerializeField] private string vBracketColour;

    [SerializeField] private List<Sprite> aiSprites;

    //Non-Serialized Fields
    [SerializeField] private List<string> textToDisplay = new List<string>();
    private Vector2 originalRectTransformPosition;
    private RectTransform dialogueRectTransform;
    [SerializeField] private bool activated = false;
    private string currentText = "";
    private string pendingText = "";
    private string pendingColouredText = "";
    private int lerpTextMinIndex = 0;
    private int lerpTextMaxIndex = 0;
    //private int clickCount = 0;

    private bool coloured = false;
    private bool lerpFinished = true;
    private string textColourString;

    //Public Properties
    public bool Activated { get => activated; }
    public int DialogueCount { get => textToDisplay.Count; }

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    //Awake checks that all the colour strings are valid
    private void Awake()
    {
        if (squareBracketColour == "")
        {
            Debug.LogError("DialogueBox.squareBracketColour is empty. It needs to have a value to work. Pick a colour (hexadecimal or string name) and fill in the field.");
        }
        else if (squareBracketColour[0].ToString() != "#")
        {
            squareBracketColour = $"\"{squareBracketColour}\"";
        }

        if (squareBracketColour == "")
        {
            Debug.LogError("DialogueBox.squigglyBracketColour is empty. It needs to have a value to work. Pick a colour (hexadecimal or string name) and fill in the field.");
        }
        else if (squigglyBracketColour[0].ToString() != "#")
        {
            squigglyBracketColour = $"\"{squigglyBracketColour}\"";
        }

        if (squareBracketColour == "")
        {
            Debug.LogError("DialogueBox.vBracketColour is empty. It needs to have a value to work. Pick a colour (hexadecimal or string name) and fill in the field.");
        }
        else if (vBracketColour[0].ToString() != "#")
        {
            vBracketColour = $"\"{vBracketColour}\"";
        }
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

    //Lerps text, checks if player used spacebar to progress text
    private void Update()
    {
        if (!lerpFinished)
        {
            pendingText = "";
            pendingColouredText = "";
            coloured = false;
            //Debug.Log($"index: {lerpTextMaxIndex}, length: {currentText.Length}");
            foreach (char c in currentText.Substring(0, lerpTextMaxIndex))
            {
                if (coloured)
                {
                    if (c.ToString() == "]" || c.ToString() == "}" || c.ToString() == "<")
                    {
                        coloured = false;
                        pendingText += $"<color={textColourString}><b>{pendingColouredText}</b></color>";
                        pendingColouredText = "";
                    }
                    else
                    {
                        pendingColouredText += c;
                    }
                }
                else
                {
                    if (c.ToString() == "[")
                    {
                        coloured = true;
                        textColourString = squareBracketColour;
                    }
                    else if (c.ToString() == "{")
                    {
                        coloured = true;
                        textColourString = squigglyBracketColour;
                    }
                    else if (c.ToString() == ">")
                    {
                        coloured = true;
                        textColourString = vBracketColour;
                    }
                    else
                    {
                        pendingText += c;
                    }
                }
            }

            if (coloured)
            {
                pendingText += $"<color={textColourString}><b>{pendingColouredText}</b></color>";
            }

            textBox.text = pendingText;

            if (lerpTextMaxIndex < currentText.Length - 1)
            {
                lerpTextMaxIndex = Mathf.Min(lerpTextMaxIndex + lerpTextInterval, currentText.Length - 1);
            }
            else
            {
                //Debug.Log("TextLerpFinished");
                lerpFinished = true;
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            RegisterDialogueRead();
        }
    }

    //Utility Methods - Changeover the dialogue list-------------------------------------------------------------------------------------------------

    //Activates the dialogue box; takes a single string
    public void ActivateDialogueBox(string text, float invokeDelay)
    {
        List<string> texts = new List<string>();
        texts.Add(text);
        ActivateDialogueBox(texts, invokeDelay);
    }

    //Activates the dialogue box; takes a list of strings
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

    //Displays the dialogue box once it's been activated and the invocation delay has finished
    private void ShowDialogueBox()
    {
        DisplayNext();
        dialogueRectTransform.DOAnchorPosY(originalRectTransformPosition.y - 100f, popUpSpeed).From(true).SetEase(Ease.OutBack).SetUpdate(true);
        gameObject.SetActive(true);
    }

    //Changes over the dialogue in the list; used instead of ActivateDialogueBox when the dialogue box is already active
    public void ChangeDialogue(List<string> texts)
    {
        textToDisplay = new List<string>(texts);
        LerpNext();
    }

    //Utility Methods - Display next string in list--------------------------------------------------------------------------------------------------

    //Shows the next section of dialogue in one hit
    private void DisplayNext()
    {
        lerpFinished = false;
        textBox.text = "";
        currentText = textToDisplay[0];
        textToDisplay.Remove(textToDisplay[0]);
        lerpTextMaxIndex = currentText.Length - 1;
    }

    //Lerps the next lot of dialogue onto the dialogue box
    private void LerpNext()
    {
        //Debug.Log("LerpingNext");
        lerpFinished = false;
        textBox.text = "";
        currentText = textToDisplay[0];
        textToDisplay.Remove(textToDisplay[0]);
        lerpTextMaxIndex = 0;
    }

    //Utility Methods - Progress / Finish Dialogue---------------------------------------------------------------------------------------------------

    //Called by OnClick to register that the player has read the currently displayed dialogue
    public void RegisterDialogueRead()
    {
        //clickCount++;
        //Debug.Log($"Click count: {clickCount}");
        //Debug.Log("Registering dialogue read");
        if (textToDisplay.Count > 0)
        {
            LerpNext();
        }
        else if (activated)
        {
            //Debug.Log("RegisterDialogueRead calling DeactivateDialogue");
            DeactivateDialogueBox();
        }
    }

    //Tweens the dialogue box out
    private void DeactivateDialogueBox()
    {
        dialogueRectTransform.DOAnchorPosY(originalRectTransformPosition.y - 100f, popUpSpeed).SetEase(Ease.InBack).SetUpdate(true).OnComplete(
            delegate 
            {
                //Reset position after tweening
                dialogueRectTransform.anchoredPosition = originalRectTransformPosition;
                gameObject.SetActive(false);
                textBox.text = "";

                if (TutorialController.Instance.TutorialStage != TutorialStage.Finished)
                {
                    TutorialController.Instance.RegisterDialogueRead();
                }
                else
                {
                    ObjectiveController.Instance.RegisterDialogueRead();
                }

                activated = false;
            });
    }
}
