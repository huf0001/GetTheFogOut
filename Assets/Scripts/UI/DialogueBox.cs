using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using DG.Tweening;

public enum AIExpression
{
    None,
    Happy,
    Neutral,
    Sad,
    Excited,
    Shocked
}

[Serializable]
public class ColourTag
{
    [SerializeField] private char openingTag;
    [SerializeField] private char closingTag;
    [SerializeField] private Color colour;
    private string colourName;

    public char OpeningTag { get => openingTag; }
    public char ClosingTag { get => closingTag; }
    public Color Colour { get => colour; }
    public string ColourName { get => colourName; set => colourName = value; }
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
    [SerializeField] private List<ExpressionDialoguePair> expressionDialoguePairs;

    //Public Properties
    public string Key { get => key; }
    public List<ExpressionDialoguePair> ExpressionDialoguePairs { get => expressionDialoguePairs; }
}

public class DialogueBox : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("Text Box")]
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private Image aiImage;

    [Header("Tween/Lerp Speeds")]
    [SerializeField] float popUpSpeed = 0.5f;
    [SerializeField] private int lerpTextInterval = 3;

    [Header("Available Expressions")]
    [SerializeField] private AIExpression currentExpression;
    [SerializeField] private Sprite aiHappy;
    [SerializeField] private Sprite aiNeutral;
    [SerializeField] private Sprite aiSad;
    [SerializeField] private Sprite aiExcited;
    [SerializeField] private Sprite aiShocked;

    [Header("Images")]
    [SerializeField] private Image completeArrow;
    [SerializeField] private Image continueArrow;

    [Header("Dialogue")]
    [SerializeField] private List<ColourTag> colourTags;
    [SerializeField] private List<DialogueSet> dialogue;

    [Header("Objective Buttons")]
    [SerializeField] private Image countdown;
    [SerializeField] private Image objButton;

    [Header("Camera Controller")]
    [SerializeField] private CameraController cameraController;

    //Non-Serialized Fields
    [Header("Temporarily Serialized")]
    private Vector2 originalRectTransformPosition;
    private RectTransform dialogueRectTransform;
    private Vector2 arrowInitialPosition;
    [SerializeField] private bool activated = false;
    [SerializeField] private bool clickable = false;

    private Dictionary<string, List<ExpressionDialoguePair>> dialogueDictionary = new Dictionary<string, List<ExpressionDialoguePair>>();
    private string currentDialogueKey = "";
    private string lastDialogueKey = "";
    private int dialogueIndex = 0;
    private string currentText = "";
    private string pendingText = "";
    private string pendingColouredText = "";
    //private int lerpTextMinIndex = 0;
    private int lerpTextMaxIndex = 0;

    private ColourTag colourTag = null;
    private bool lerpFinished = true;

    private string nextDialogueKey = "";
    private float nextInvokeDelay = 0f;
    private bool deactivationSubmitted = false;
    private bool nextDialogueSetReady = false;

    private bool dialogueRead = false;
    private bool deactivating = false;

    private RectTransform continueArrowTransform;
    private RectTransform completeArrowTransform;

    private float dialogueTimer = 0;

    //Public Properties
    public bool Activated { get => activated; }
    public string CurrentDialogueSet { get => currentDialogueKey; }
    public bool Deactivating { get => deactivating; }
    public int DialogueIndex { get => dialogueIndex; }
    public float DialogueTimer { get => dialogueTimer; }
    public string LastDialogueSet { get => lastDialogueKey; }

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    //Awake checks that all the colour strings are valid, and gets the starting AI expression
    private void Awake()
    {
        aiImage.sprite = aiNeutral;
        arrowInitialPosition = completeArrow.GetComponent<RectTransform>().anchoredPosition;

        foreach (ColourTag c in colourTags)
        {
            c.ColourName = $"#{ColorUtility.ToHtmlStringRGB(c.Colour)}";
        }
        
        if (aiImage.sprite == aiHappy)
        {
            currentExpression = AIExpression.Happy;
        }
        else if (aiImage.sprite == aiNeutral)
        {
            currentExpression = AIExpression.Neutral;
        }
        else if (aiImage.sprite == aiSad)
        {
            currentExpression = AIExpression.Sad;
        }
        else if (aiImage.sprite == aiExcited)
        {
            currentExpression = AIExpression.Excited;
        }
        else if (aiImage.sprite == aiShocked)
        {
            currentExpression = AIExpression.Shocked;
        }

        foreach (DialogueSet ds in dialogue)
        {
            if (dialogueDictionary.ContainsKey(ds.Key))
            {
                Debug.Log($"DialogueBox has multiple dialogue sets with the dialogue key {ds.Key}. Each dialogue key should be unique.");
            }
            else
            {
                dialogueDictionary[ds.Key] = ds.ExpressionDialoguePairs;
            }
        }
    }

    private void Start()
    {
        WorldController.Instance.Inputs.InputMap.ProceedDialogue.performed += ctx => RegisterDialogueRead();

        continueArrowTransform = continueArrow.GetComponent<RectTransform>();
        completeArrowTransform = completeArrow.GetComponent<RectTransform>();
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

    //Checks for new dialogue, lerps text, checks if player wants to progress text
    private void Update()
    {
        if (clickable)
        {
            dialogueTimer += Time.deltaTime;

            if (dialogueDictionary.ContainsKey(currentDialogueKey))
            {
                if (!continueArrow.enabled && dialogueIndex < dialogueDictionary[currentDialogueKey].Count)
                {
                    if (completeArrow.enabled)
                    {
                        DOTween.Kill(completeArrowTransform);
                        completeArrowTransform.anchoredPosition = arrowInitialPosition;
                        completeArrow.enabled = false;
                    }

                    continueArrow.enabled = true;
                    continueArrowTransform.DOAnchorPosX(arrowInitialPosition.x + 5, 0.3f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                }
                else if (!completeArrow.enabled && dialogueIndex == dialogueDictionary[currentDialogueKey].Count)
                {
                    if (continueArrow.enabled)
                    {
                        DOTween.Kill(continueArrowTransform);
                        continueArrowTransform.anchoredPosition = arrowInitialPosition;
                        continueArrow.enabled = false;
                    }

                    completeArrow.enabled = true;
                    completeArrowTransform.DOAnchorPosY(arrowInitialPosition.y - 5, 0.3f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                }
            }
            else
            {
                Debug.Log($"Cannot display dialogue set {currentDialogueKey}; Dialogue Key {currentDialogueKey} doesn't exist.");
            }
        }

        if (nextDialogueKey != "" && !deactivating)
        {
            if (!activated)
            {
                ActivateDialogueBox(nextDialogueKey, nextInvokeDelay);

                nextDialogueKey = "";
                nextInvokeDelay = 0f;
            }
            else
            {
                ChangeDialogue(nextDialogueKey);

                nextDialogueKey = "";
                nextInvokeDelay = 0f;
            }
        }
        else if (deactivationSubmitted && activated)
        {
            lastDialogueKey = currentDialogueKey;
            currentDialogueKey = "";
            clickable = false;
            DeactivateDialogueBox();
        }

        deactivationSubmitted = false;

        if (nextDialogueSetReady && dialogueDictionary.ContainsKey(currentDialogueKey) && dialogueIndex < dialogueDictionary[currentDialogueKey].Count)
        {
            DisplayNext();
            nextDialogueSetReady = false;
        }
        else
        {
            if (nextDialogueSetReady && (dialogueDictionary.ContainsKey(currentDialogueKey) || dialogueIndex >= dialogueDictionary[currentDialogueKey].Count))
            {
                nextDialogueSetReady = false;

                if (dialogueDictionary.ContainsKey(currentDialogueKey))
                {
                    Debug.Log($"Warning: nextDialogueSetReady was true, but dialogue key {currentDialogueKey} doesn't exist in dialogueDictionary.");
                }
                else
                {
                    Debug.Log($"Warning: nextDialogueSetReady was true and dialogue key {currentDialogueKey} exists, but dialogueIndex {dialogueIndex} is an invalid index, given dialogueDictionary[{currentDialogueKey}].Count ({dialogueDictionary[currentDialogueKey].Count}).");
                }
            }

            if (clickable && dialogueRead)
            {
                RegisterDialogueRead();
            }
        }

        if (!lerpFinished)
        {
            pendingText = "";
            pendingColouredText = "";
            colourTag = null;

            foreach (char c in currentText.Substring(0, lerpTextMaxIndex))
            {
                if (colourTag != null)
                {
                    if (c == colourTag.ClosingTag)
                    {
                        pendingText += $"<color={colourTag.ColourName}><b>{pendingColouredText}</b></color>";
                        pendingColouredText = "";
                        colourTag = null;
                    }
                    else
                    {
                        pendingColouredText += c;
                    }
                }
                else
                {
                    foreach (ColourTag t in colourTags)
                    {
                        if (c == t.OpeningTag)
                        {
                            colourTag = t;
                            break;
                        }
                    }

                    if (colourTag == null)
                    {
                        pendingText += c;
                    }
                }
            }

            if (colourTag != null)
            {
                pendingText += $"<color={colourTag.ColourName}><b>{pendingColouredText}</b></color>";
            }

            textBox.text = pendingText;

            if (lerpTextMaxIndex < currentText.Length)// - 1)
            {
                lerpTextMaxIndex = Mathf.Min(lerpTextMaxIndex + lerpTextInterval, currentText.Length);// - 1);
            }
            else
            {
                lerpFinished = true;
            }
        }
    }

    //Utility Methods - Changeover the dialogue list-------------------------------------------------------------------------------------------------

    //Submits a dialogue set and invoke delay to the DialogueBox to change over to during the next update
    public void SubmitDialogueSet(string key, float invokeDelay)
    {
        if (dialogueDictionary.ContainsKey(key))
        {
            nextDialogueKey = key;
            nextInvokeDelay = invokeDelay;
        }
        else
        {
            Debug.Log("Dialogue key '" + key + "' is invalid.");
        }
    }

    //Activates the dialogue box; takes a list of strings
    private void ActivateDialogueBox(string key, float invokeDelay)
    {
        if (dialogueDictionary.ContainsKey(key) && dialogueDictionary[key].Count > 0)
        {
            //Caches required tweening information for performance saving
            dialogueRectTransform = GetComponent<RectTransform>();
            originalRectTransformPosition = GetComponent<RectTransform>().anchoredPosition;

            dialogueIndex = 0;
            lastDialogueKey = currentDialogueKey == "" ? lastDialogueKey : currentDialogueKey;
            currentDialogueKey = key;

            activated = true;
            nextDialogueSetReady = false;

            Invoke(nameof(ShowDialogueBox), invokeDelay);
        }
        else
        {
            Debug.LogError($"dialogueDictionary[{key}] contains no dialogue set for DialogueBox to display.");
        }
    }

    //Displays the dialogue box once it's been activated and the invocation delay has finished
    private void ShowDialogueBox()
    {
        nextDialogueSetReady = true;

        countdown.rectTransform.DOAnchorPosY(-18 + 150, popUpSpeed).SetEase(Ease.OutBack);
        objButton.rectTransform.DOAnchorPosY(-18 + 150, popUpSpeed).SetEase(Ease.OutBack);
        dialogueRectTransform.DOAnchorPosY(-18, popUpSpeed).SetEase(Ease.OutBack).SetUpdate(true).OnComplete(
            delegate
            {
                clickable = true;
                dialogueTimer = 0;
            });
    }

    //Changes over the dialogue in the list; used instead of ActivateDialogueBox when the dialogue box is already active
    private void ChangeDialogue(string key)
    {
        if (dialogueDictionary.ContainsKey(key) && dialogueDictionary[key].Count > 0)
        {
            lastDialogueKey = currentDialogueKey;
            currentDialogueKey = key;
            dialogueIndex = 0;
            LerpNext();
        }
        else
        {
            Debug.LogError($"dialogueDictionary[{key}] contains no dialogue set for DialogueBox to display.");
        }
    }

    //Utility Methods - Display next set of content--------------------------------------------------------------------------------------------------

    //Shows the next section of dialogue in one hit
    private void DisplayNext()
    {
        lerpFinished = false;
        textBox.text = "";
        currentText = dialogueDictionary[currentDialogueKey][dialogueIndex].Dialogue;

        if (dialogueDictionary[currentDialogueKey][dialogueIndex].AIExpression != currentExpression)
        {
            ChangeAIExpression(dialogueDictionary[currentDialogueKey][dialogueIndex].AIExpression);
        }
        //else
        //{
        //    Debug.Log($"Keeping dialogue expression as {currentExpression}");
        //}

        dialogueIndex++;
        lerpTextMaxIndex = currentText.Length - 1;
        dialogueTimer = 0;
    }

    //Lerps the next lot of dialogue onto the dialogue box
    private void LerpNext()
    {
        lerpFinished = false;
        textBox.text = "";
        currentText = dialogueDictionary[currentDialogueKey][dialogueIndex].Dialogue;

        if (dialogueDictionary[currentDialogueKey][dialogueIndex].AIExpression != currentExpression)
        {
            ChangeAIExpression(dialogueDictionary[currentDialogueKey][dialogueIndex].AIExpression);
        }
        //else
        //{
        //    Debug.Log($"Keeping dialogue expression as {currentExpression}");
        //}

        dialogueIndex++;
        lerpTextMaxIndex = 0;
        dialogueTimer = 0;
    }

    //Updates the sprite of aiImage
    private void ChangeAIExpression(AIExpression expression)
    {
        //Debug.Log($"Changing AIExpression from {currentExpression} to {expression}");
        currentExpression = expression;

        switch (expression)
        {
            case AIExpression.Happy:
                aiImage.sprite = aiHappy;
                break;
            case AIExpression.Neutral:
                aiImage.sprite = aiNeutral;
                break;
            case AIExpression.Sad:
                aiImage.sprite = aiSad;
                break;
            case AIExpression.Excited:
                aiImage.sprite = aiExcited;
                break;
            case AIExpression.Shocked:
                aiImage.sprite = aiShocked;
                break;
        }

        //Debug.Log($"AIExpression is now {currentExpression}");
    }

    //Utility Methods - Progress / Finish Dialogue---------------------------------------------------------------------------------------------------

    //Called by OnClick to register that the player has read the currently displayed dialogue
    public void RegisterDialogueRead()
    {
        if (clickable)
        {
            DOTween.Kill(continueArrowTransform);
            continueArrowTransform.anchoredPosition = arrowInitialPosition;
            continueArrow.enabled = false;

            DOTween.Kill(completeArrowTransform);
            completeArrowTransform.anchoredPosition = arrowInitialPosition;
            completeArrow.enabled = false;

            if (dialogueIndex < dialogueDictionary[currentDialogueKey].Count)
            {
                LerpNext();
            }
            else if (activated)
            {
                lastDialogueKey = currentDialogueKey;
                currentDialogueKey = "";
                clickable = false;
                DeactivateDialogueBox();
            }
        }
    }

    //Tweens the dialogue box out
    private void DeactivateDialogueBox()
    {
        dialogueTimer = 0;
        deactivating = true;
        countdown.rectTransform.DOAnchorPosY(20, popUpSpeed).SetEase(Ease.InBack);
        objButton.rectTransform.DOAnchorPosY(20, popUpSpeed).SetEase(Ease.InBack);
        dialogueRectTransform.DOAnchorPosY(originalRectTransformPosition.y, popUpSpeed).SetEase(Ease.InBack).SetUpdate(true).OnComplete(
            delegate
            {
                //Reset position after tweening
                textBox.text = "";
                deactivating = false;

                if (TutorialController.Instance.Stage != TutorialStage.Finished)
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

    //Lets other classes call for the dialogue box to be deactivated.
    public void SubmitDeactivation()
    {
        deactivationSubmitted = true;
    }
}
