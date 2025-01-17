﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using DG.Tweening;

[Serializable]
public struct HudColour
{
    public string key;
    public Color value;
}

public class UIController : MonoBehaviour
{
    // Fields ----------------------------------------------------------------------------------------------------------
    public static UIController instance = null;

    TextMeshProUGUI powerText, organicText, mineralText, fuelText;
    public GameObject endGame, pauseGame;
    public BuildingSelector buildingSelector;
    public BuildingInfo buildingInfo;
    public TextMeshProUGUI endGameText;
    public bool buttonClosed = true;

    private GameObject cursor;
    private Slider powerSlider;
    private float power = 0, powerChange = 0, mineral = 0;
    private float powerVal = 0.0f, mineralVal = 0.0f;
    private float powerTime = 0.0f, mineralTime = 0.0f;
    private Image launchButtonImage;
    
    ResourceController resourceController = null;
    private int index, temp;
    private MeshRenderer tile;
    private Vector2 arrowInitialPosition;
    private RectTransform abilityImageParent;
    private Vector3 abilityImageOriginalPos;

    private float currentPowerValDisplayed = 0;
    private bool unlockingAbility = false;
    private bool openingButton = false;
    private bool closingButton = false;

    // SerializedFields ------------------------------------------------------------------------------------------------
    [SerializeField] private Image hud;
    [SerializeField] private Image powerImg;
    [SerializeField] private Sprite[] powerLevelSprites;
    [SerializeField] private Animator powerThresholds;
    [SerializeField] private TextMeshProUGUI objWindowText;
    [SerializeField] private TextMeshProUGUI hudObjText;
    [SerializeField] private Image objBG;
    [SerializeField] private Image objTitleBG;
    [SerializeField] private Image objArrowBG;
    [SerializeField] private Image dialogueBoxBG;
    [SerializeField] private HudColour[] hudColours;

    [Header("Tile Colours")]
    [SerializeField] private Color powerLow;
    [SerializeField] private Color powerMedium;
    [SerializeField] private Color powerHigh;
    [SerializeField] private Color powerMax;
    [SerializeField] public Color powerCurrent;
    [Header("Objective Buttons")]
    [SerializeField, FormerlySerializedAs("launchCanvas")] private GameObject objectiveProceedCanvas;
    [SerializeField, FormerlySerializedAs("launchButtonBG")] private Image objectiveButtonBG;
    [SerializeField, FormerlySerializedAs("launchButton")] private Button objectiveButton;
    [SerializeField] private Sprite[] objectiveButtonSprites;
    [Header("Countdown Slider")]
    [SerializeField] private Image countdownSliderBG;
    [SerializeField] private CanvasGroup countdownSliderCG;
    [SerializeField] private Slider countdownSlider;
    [SerializeField] private TextMeshProUGUI countdownText;
    [Header("Ability Unlock")]
    [SerializeField] private Canvas abilityUnlockCanvas;
    [SerializeField] private RectTransform abilityUnlockBG;
    [SerializeField] private RectTransform proceedArrows;
    [SerializeField] private Image abilityImage;
    [SerializeField] private Sprite[] abilitySprites;
    [SerializeField] private Button[] abilityButtons;
    [SerializeField] private Image[] abilityButtonIcons;
    [SerializeField] private TextMeshProUGUI[] abilityButtonUnknown;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescText;
    [SerializeField, TextArea] private string[] abilityDescriptions;
    [SerializeField] private TextMeshProUGUI[] abilityHotkeyTextboxes;
    [Header("Upgrades")]
    [SerializeField] private GameObject upgradesCanvas;
    [SerializeField] private Transform upgradesBg;
    
    public float CurrentPowerValDisplayed { get => currentPowerValDisplayed; }
    public bool UpgradeWindowVisible { get; set; }

    // Startup Methods -------------------------------------------------------------------------------------------------
    
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            objWindowText.text = "** Initialising **";
            hudObjText.text = "<b>** Initialising **</b>\n\n** Initialising **\n\n";
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        index = 0;
        temp = 2;

        //objBG.alphaHitTestMinimumThreshold = 0.1f;
        //objTitleBG.alphaHitTestMinimumThreshold = 0.1f;
        //objArrowBG.alphaHitTestMinimumThreshold = 0.1f;
        hud.alphaHitTestMinimumThreshold = 0.35f;

        launchButtonImage = objectiveButton.image;
        arrowInitialPosition = proceedArrows.GetComponent<RectTransform>().anchoredPosition;
        abilityImageParent = abilityImage.rectTransform.parent.GetComponent<RectTransform>();
        abilityImageOriginalPos = abilityImageParent.anchoredPosition;
        FindSliders();
    }

    // find sliders and text
    void FindSliders()
    {
        powerText = GameObject.Find("PowerLevel").GetComponent<TextMeshProUGUI>();
        mineralText = GameObject.Find("MineralLevel").GetComponent<TextMeshProUGUI>();
    }
    
    
    // Update Methods --------------------------------------------------------------------------------------------------
    
    // Update is called once per frame
    void Update()
    {
        if (resourceController == null)
        {
            resourceController = ResourceController.Instance;
        }

        if (WorldController.Instance.Inputs.InputMap.Pause.triggered && UpgradeWindowVisible)
        {
            HideUpgradeWindow();
        }

        powerTime += Time.deltaTime;
        mineralTime += Time.deltaTime;
        UpdateResourceText();
    }

    // Power Threshold image show and hide methods
    public void ShowPowerThresholds()
    {
        powerThresholds.SetBool("Hover", true);
    }

    public void HidePowerThresholds()
    {
        powerThresholds.SetBool("Hover", false);
    }

    // Functions dealing with the drop down objective button
    public void ShowRepairButton(string controller)
    {
        DOTween.Kill("ObjectiveButtonClose");
        objectiveProceedCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[0];

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(objectiveButtonBG.DOFade(0.93f, 1))
            .Append(launchButtonImage.DOFade(1, 0.5f))
            .SetId("ObjectiveButtonOpen")
            .OnComplete(
            delegate
            {
                objectiveButton.enabled = true;
                buttonClosed = false;
                objectiveButton.onClick.AddListener(
                    delegate
                    {
                        int mineralGoal;

                        if (controller == "O")
                        {
                            mineralGoal = ObjectiveController.Instance.MineralTarget;
                        }
                        else
                        {
                            mineralGoal = TutorialController.Instance.CollectedMineralsGoal;
                        }

                        if (ResourceController.Instance.StoredMineral >= mineralGoal)
                        {
                            ResourceController.Instance.StoredMineral -= mineralGoal;
                            objectiveButton.enabled = false;

                            if (controller == "O")
                            {
                                ObjectiveController.Instance.IncrementStage();
                            }
                            else
                            {
                                TutorialController.Instance.CompleteMineralCollection();
                            }

                            CloseButton();
                        }
                    });
                launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 1).SetLoops(-1, LoopType.Yoyo);
            });
    }

    public void ShowAttachButton()
    {
        objectiveProceedCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[1];

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(objectiveButtonBG.DOFade(0.93f, 1))
            .Append(launchButtonImage.DOFade(1, 0.5f))
            .OnComplete(
            delegate
            {
                objectiveButton.enabled = true;
                buttonClosed = false;
                objectiveButton.onClick.AddListener(
                    delegate
                    {
                        objectiveButton.enabled = false;
                        ObjectiveController.Instance.IncrementSubStage();
                        CloseButton();
                    });
                launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 1).SetLoops(-1, LoopType.Yoyo);
            });
    }

    public void ShowCountdownSlider()
    {
        countdownSlider.maxValue = ObjectiveController.Instance.Countdown;
        Sequence sequence = DOTween.Sequence();
        sequence.Insert(1.5f, countdownSliderBG.DOFade(0.93f, 1))
            .Append(countdownSliderCG.DOFade(1, 0.5f).OnComplete(
                delegate
                {
                    countdownSliderCG.blocksRaycasts = true;
                    countdownText.DOFade(0.7f, 0.7f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo);
                }));
    }

    public void UpdateCountdownSlider()
    {
        if (countdownSlider.value < countdownSlider.maxValue)
        {
            countdownSlider.value += Time.deltaTime;
            if (countdownSlider.value > countdownSlider.maxValue)
            {
                countdownSlider.value = countdownSlider.maxValue;
            }
        }
    }

    public void HideCountdownSlider()
    {
        DOTween.Kill(countdownText);
        countdownSliderCG.alpha = 0;
        countdownSliderBG.color = new Color(0, 0, 0, 0);
    }

    public void ShowLaunchButton()
    {
        launchButtonImage.sprite = objectiveButtonSprites[2];
        objectiveButtonBG.rectTransform.localScale = new Vector3(2, 2, 2);

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(objectiveButtonBG.DOFade(0.93f, 1))
            .Append(launchButtonImage.DOFade(1, 0.5f))
            .OnComplete(
            delegate
            {
                objectiveButton.enabled = true;
                objectiveButton.onClick.AddListener(delegate
                {
                    WinGame();
                });
                launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 0.5f).SetLoops(-1, LoopType.Yoyo);
                buttonClosed = false;
            });
    }

    public void ShowActivateButton()
    {
        if (buttonClosed && !openingButton)
        {
            openingButton = true;
            objectiveProceedCanvas.SetActive(true);
            launchButtonImage.sprite = objectiveButtonSprites[3];

            Sequence showLaunch = DOTween.Sequence();
            showLaunch.Append(objectiveButtonBG.DOFade(0.93f, 1))
                .Append(launchButtonImage.DOFade(1, 0.5f))
                .OnComplete(
                    delegate
                    {
                        Debug.Log("Mark1");
                        objectiveButton.enabled = true;
                        buttonClosed = false;
                        objectiveButton.onClick.AddListener(
                            delegate
                            {
                                Debug.Log("Mark2");
                                if (TutorialController.Instance.DefencesOperable())     //If returns false, DefencesOperable() sets up the BuildDefencesInRange() stage
                                {
                                    TutorialController.Instance.ActivateDefences();
                                }

                                objectiveButton.enabled = false;
                                CloseButton();
                            });
                        launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 1).SetLoops(-1, LoopType.Yoyo);
                        openingButton = false;
                    });
        }
    }

    public void CloseButton()
    {
        if (!buttonClosed && !closingButton)
        {
            closingButton = true;
            if (DOTween.IsTweening("ObjectiveButtonOpen")) DOTween.Kill("ObjectiveButtonOpen");
            else DOTween.Kill(launchButtonImage);
            launchButtonImage.color = new Color(1, 1, 1);

            Sequence showLaunch = DOTween.Sequence();
            showLaunch.Append(launchButtonImage.DOFade(0, 0.5f))
            .Append(objectiveButtonBG.DOFade(0, 1))
            .SetId("ObjectiveButtonClose")
            .OnComplete(
            delegate
            {
                objectiveButton.onClick.RemoveAllListeners();
                objectiveButton.enabled = false;
                if (ObjectiveController.Instance.CurrStage != (int)ObjectiveStage.SurvivalStage)
                {
                    objectiveProceedCanvas.SetActive(false);
                }
                buttonClosed = true;
                closingButton = false;
            });
        }
    }

    // Ability unlock screen
    public void AbilityUnlock(Ability ability)
    {
        Debug.Log("AbilityUnlock");

        if (!unlockingAbility)
        {
            Debug.Log("Unlocking new ability");
            Time.timeScale = 0.1f;
            unlockingAbility = true;
            abilityUnlockCanvas.gameObject.SetActive(true);

            switch (ability.AbilityType)
            {
                case AbilityEnum.Artillery:
                    abilityImage.sprite = abilitySprites[0];
                    abilityNameText.text = "Artillery Blast";
                    abilityDescText.text = abilityDescriptions[0];
                    abilityButtons[0].interactable = true;
                    abilityButtonIcons[0].enabled = true;
                    abilityButtonUnknown[0].enabled = false;
                    abilityHotkeyTextboxes[0].color = new Color(1, 1, 1);
                    break;
                case AbilityEnum.BuildingDefence:
                    abilityImage.sprite = abilitySprites[1];
                    abilityNameText.text = "Defence Mode";
                    abilityDescText.text = abilityDescriptions[1];
                    abilityButtons[1].interactable = true;
                    abilityButtonIcons[1].enabled = true;
                    abilityButtonUnknown[1].enabled = false;
                    abilityHotkeyTextboxes[1].color = new Color(1, 1, 1);
                    break;
                case AbilityEnum.FreezeFog:
                    abilityImage.sprite = abilitySprites[2];
                    abilityNameText.text = "Freeze Fog";
                    abilityDescText.text = abilityDescriptions[2];
                    abilityButtons[2].interactable = true;
                    abilityButtonIcons[2].enabled = true;
                    abilityButtonUnknown[2].enabled = false;
                    abilityHotkeyTextboxes[2].color = new Color(1, 1, 1);
                    break;
                case AbilityEnum.Overclock:
                    abilityImage.sprite = abilitySprites[3];
                    abilityNameText.text = "Overclock";
                    abilityDescText.text = abilityDescriptions[3];
                    abilityButtons[3].interactable = true;
                    abilityButtonIcons[3].enabled = true;
                    abilityButtonUnknown[3].enabled = false;
                    abilityHotkeyTextboxes[3].color = new Color(1, 1, 1);
                    break;
                case AbilityEnum.Sonar:
                    abilityImage.sprite = abilitySprites[4];
                    abilityNameText.text = "Sonar";
                    abilityDescText.text = abilityDescriptions[4];
                    abilityButtons[4].interactable = true;
                    abilityButtonIcons[4].enabled = true;
                    abilityButtonUnknown[4].enabled = false;
                    abilityHotkeyTextboxes[4].color = new Color(1, 1, 1);
                    break;
            }

            proceedArrows.DOAnchorPosY(arrowInitialPosition.y - 5, 0.3f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
            abilityUnlockBG.DOScale(1, 0.3f).SetUpdate(true).SetEase(Ease.OutBack).OnComplete(
                delegate
                {
                    Debug.Log("Finished showing ability unlock");
                    unlockingAbility = false;
                });
        }
        else
        {
            Debug.Log("Waiting for unlock to finish before unlocking new ability");
        }
    }


    public void FinishAbilityUnlock()
    {
        Debug.Log("FinishAbilityUnlock");

        if (!unlockingAbility)
        {
            Debug.Log("FinishAbilityUnlock; OpenAbilityUnlock finished");
            DOTween.Complete(abilityUnlockBG);
            DOTween.Kill(proceedArrows);
            proceedArrows.anchoredPosition = arrowInitialPosition;
            abilityImageParent.DOScale(0.45f, 0.6f);
            abilityImageParent.DOMove(abilityButtons[0].transform.parent.position, 0.6f).OnComplete(
                delegate
                {
                    abilityImageParent.gameObject.SetActive(false);
                    abilityUnlockBG.DOScale(0.01f, 0.3f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(
                        delegate
                        {
                            abilityImageParent.anchoredPosition = abilityImageOriginalPos;
                            abilityImageParent.gameObject.SetActive(true);
                            abilityImageParent.localScale = new Vector3(1, 1, 1);
                            abilityUnlockCanvas.gameObject.SetActive(false);
                        });
                });
            Time.timeScale = 1;
            Debug.Log("FinishedUnlockingAbility");
        }
        else
        {
            Debug.Log("Waiting for unlock to finish");
        }
    }

    public void ShowUpgradeWindow()
    {
        upgradesCanvas.SetActive(true);
        UpgradeWindowVisible = true;
        upgradesBg.DOScale(1, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-UI_Select", transform.position);
        Time.timeScale = 0.1f;
    }

    public void HideUpgradeWindow()
    {
        DOTween.Complete(upgradesBg);
        upgradesBg.DOScale(0.01f, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(
            delegate
            {
                upgradesCanvas.SetActive(false);
                UpgradeWindowVisible = false;
            });
        Time.timeScale = 1;
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/2D-UI_Back", transform.position);
    }

    public void WinGame()
    {
        DOTween.Kill(launchButtonImage);
        Animator animator = WorldController.Instance.Hub.GetComponent<Animator>();
        animator.enabled = true;
        WorldController.Instance.GameWin = true;
        WorldController.Instance.GameOver = true;
        Fog.Instance.DamageOn = false;
    }

    // End Game Method
    public void EndGameDisplay(string text)
    {
        //endGameText.text = text;
        endGame.SetActive(true);
    }

    public void ToggleCursor(bool isOn)
    {
        cursor.SetActive(isOn);
    }

    public void ChangeUIColour(string key)
    {
        Color colour = Color.white;
        foreach (HudColour h in hudColours)
        {
            if (h.key == key)
            {
                colour = h.value;
            }
        }

        hud.color = colour;
        objBG.color = colour;
        objArrowBG.color = colour;
        dialogueBoxBG.color = colour;
        objTitleBG.color = colour;
        countdownSliderBG.color = new Color(colour.r, colour.g, colour.b, countdownSliderBG.color.a);
        objectiveButtonBG.color = new Color(colour.r, colour.g, colour.b, objectiveButtonBG.color.a);
    }

    private void ChangeColor(Color newColor, bool flash)
    {
        GameObject tileObject = GameObject.FindGameObjectWithTag("Tile");

        if (tileObject)
        {
            tile = tileObject.GetComponent<MeshRenderer>();
            if (!flash)
            {
                tile.sharedMaterial.DOColor(newColor, "_BaseColor", 1);
            }
            else
            {
                tile.sharedMaterial.SetColor("_BaseColor", GetAlpha(newColor, 0.1f));
                tile.sharedMaterial.DOColor(newColor, "_BaseColor", 1).SetLoops(-1, LoopType.Yoyo).SetSpeedBased()
                    .SetId("tile");
            }
        }
    }

    private Color GetAlpha(Color color, float avalue)
    {
        Color current = color;
        current.a = avalue;
        return current;
    }

    void UpdateResourceText()
    {
        if (resourceController != null)
        {
            // if the stored power is different, change values used for lerping
            if (resourceController.StoredPower != power)
            {
                powerVal = power;
                power = resourceController.StoredPower;
                powerTime = 0;
            }

            powerChange = resourceController.PowerChange;
            string colour;
            if (powerChange > 0)
            {
                colour = "#009900>+";
            }
            else if (powerChange < 0)
            {
                colour = "\"red\">";
            }
            else
            {
                colour = "#006273>±";
            }
            // update text values
            currentPowerValDisplayed = Mathf.Round(Mathf.Lerp(powerVal, power, powerTime));
            if (currentPowerValDisplayed >= 100)
            {
                currentPowerValDisplayed = 100;
            }
            powerText.text = currentPowerValDisplayed + "%" + "\n<size=80%><color=" + colour + powerChange.ToString("F1") + " %/s</color>";

            if (currentPowerValDisplayed == 0)
            {
                if (powerImg.sprite != powerLevelSprites[0])
                {
                    powerImg.sprite = powerLevelSprites[0];
                }
            }
            else if (currentPowerValDisplayed <= 25)
            {
                if (powerImg.sprite != powerLevelSprites[1])
                {
                    powerImg.sprite = powerLevelSprites[1];
                    index = 1;
                }
            }
            else if (currentPowerValDisplayed <= 50)
            {
                if (powerImg.sprite != powerLevelSprites[2])
                {
                    powerImg.sprite = powerLevelSprites[2];
                    index = 2;
                }
            }
            else if (currentPowerValDisplayed <= 75)
            {
                if (powerImg.sprite != powerLevelSprites[3])
                {
                    powerImg.sprite = powerLevelSprites[3];
                    index = 0;
                }
            }
            else if (currentPowerValDisplayed > 75)
            {
                if (powerImg.sprite != powerLevelSprites[4])
                {
                    powerImg.sprite = powerLevelSprites[4];
                    index = 3;
                }
            }

            if (temp != index)
            {
                temp = index;
                switch (index)
                {
                    case 0: // green
                        powerCurrent = powerHigh;
                        break;
                    case 1:  //red
                        powerCurrent = powerLow;
                        break;
                    case 2: // yellow
                        powerCurrent = powerMedium;
                        break;
                    case 3:
                        powerCurrent = powerMax;
                        break;
                }

                if (index == 1)
                {
                    ChangeColor(powerCurrent, true);
                }
                else
                {
                    DOTween.Kill("tile");
                    ChangeColor(powerCurrent, false);
                }
            }

            if (resourceController.StoredMineral != mineral)
            {
                mineralVal = mineral;
                mineral = resourceController.StoredMineral;
                mineralTime = 0;
            }

            float mineralChange = resourceController.MineralChange;

            if (mineralChange > 0)
            {
                colour = "#009900>+";
            }
            else if (mineralChange < 0)
            {
                colour = "\"red\">";
            }
            else
            {
                colour = "#006273>±";
            }

            mineralText.text = Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime)) + " units\n<size=80%><color=" + colour + mineralChange + "</color>";
        }
    }

    public void UpdateObjectiveText(ObjectiveStage stage)
    {
        switch (stage)
        {
            case ObjectiveStage.None:
                hudObjText.text = "<b>Complete the Tutorial</b>";
                objWindowText.text = "<size=75%>Proceed through the tutorial and learn to play the game!\n\n";
                break;
            case ObjectiveStage.HarvestMinerals:
                hudObjText.text = "<b>Repair the Hull</b>";
                objWindowText.text = "<size=75%>Gather enough mineral resources to repair your ship's hull.\n\n" +
                    $"Target: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))} / {ObjectiveController.Instance.MineralTarget} <size=90%><sprite=\"all_icons\" index=2>";
                break;
            case ObjectiveStage.RecoverPart:
                string nf = "Not Collected";
                string f = "Collected";
                hudObjText.text = "<b>Recover the Thruster</b>";
                objWindowText.text = "<size=75%>Push your way through the fog to find the missing thruster from your ship.\n\n" +
                    "Thruster: " + (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected ? f : nf);
                break;
            case ObjectiveStage.Upgrades:
                hudObjText.text = "<b>Upgrades</b>";
                objWindowText.text = "<size=75%>Learn how to use the upgrades system.\n\n" +
                    "Click on the ship to get started.";
                break;
            case ObjectiveStage.SurvivalStage:
                hudObjText.text = "<b>Leave the Planet</b>";
                objWindowText.text = "<size=75%>Your ship is undergoing repairs. Protect yourself from the fog until you are ready to leave, then blast off this wretched planet!\n\n" +
                    $"Target: Wait for the ship to finish repairing";
                break;
        }
    }

    public void UpdateObjectiveText(TutorialStage stage)
    {
        switch (stage)
        {
            case TutorialStage.None:
            case TutorialStage.ExplainSituation:
            case TutorialStage.WaitingForPowerDrop:
            case TutorialStage.SonarActivated:
                hudObjText.text = "<b>Complete the Tutorial</b>";
                objWindowText.text = "<size=75%>Complete the tutorial and learn to play the game!\n\n";
                break;
            case TutorialStage.CameraControls:
                hudObjText.text = "<b>Move Nex</b>";
                objWindowText.text = "<size=75%>Learn how to move your Nexus Drone.\n\n";
                break;
            case TutorialStage.BuildHarvesters:
                hudObjText.text = "<b>Build Harvesters</b>";
                objWindowText.text = $"<size=75%>Build {TutorialController.Instance.BuiltHarvestersGoal} mineral Harvesters to collect building materials.\n\n" +
                    $"Target: {ResourceController.Instance.Harvesters.Count} / {TutorialController.Instance.BuiltHarvestersGoal} Harvesters";
                break;
            case TutorialStage.BuildHarvestersExtended:
                hudObjText.text = "<b>Build Harvesters</b>";
                objWindowText.text = $"<size=75%>Build {TutorialController.Instance.BuiltHarvestersExtendedGoal} mineral Harvesters to collect building materials.\n\n" +
                    $"Target: {ResourceController.Instance.Harvesters.Count} / {TutorialController.Instance.BuiltHarvestersExtendedGoal} Harvesters";
                break;
            case TutorialStage.BuildExtender:
                hudObjText.text = "<b>Build Power Extender</b>";
                objWindowText.text = "<size=75%>Build a Power Extender to reach additional mineral nodes.\n\n";
                break;
            case TutorialStage.MouseOverPowerDiagram:
                hudObjText.text = "<b>Look at Power Diagram</b>";
                objWindowText.text = "<size=75%>Move the mouse over the power icon to view the diagram explaining how power works.\n\n";
                break;
            case TutorialStage.BuildGenerator:
                hudObjText.text = "<b>Build Power Supply</b>";
                objWindowText.text = "<size=75%>Build a Power Supply to increase your available power generation.\n\n";
                break;
            case TutorialStage.BuildMoreGenerators:
                hudObjText.text = "<b>Build Power Supplies</b>";
                objWindowText.text = $"<size=75%>Build {TutorialController.Instance.BuiltGeneratorsGoal} Supplies to increase your available power generation.\n\n" +
                     $"Target: {ResourceController.Instance.Generators.Count} / {TutorialController.Instance.BuiltGeneratorsGoal} Supplies";
                break;
            case TutorialStage.CollectMinerals:
                hudObjText.text = "<b>Repair the Hull</b>";
                objWindowText.text = "<size=75%>Gather enough mineral resources to repair your ship's hull.\n\n" +
                    $"Target: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))} / {TutorialController.Instance.CollectedMineralsGoal} <size=90%><sprite=\"all_icons\" index=2>";
                break;
            case TutorialStage.CollectSonar:
                hudObjText.text = "<b>Recover Canister</b>";
                objWindowText.text = "<size=75%>Retrieve that canister lying near the ship.\n\n";
                break;
            case TutorialStage.ActivateSonar:
                hudObjText.text = "<b>Activate Sonar</b>";
                objWindowText.text = "<size=75%>Activate the Sonar and find the ship's remaining missing parts.\n\n";
                break;
            case TutorialStage.BuildExtenderInFog:
                hudObjText.text = "<b>Build Power Extender</b>";
                objWindowText.text = "<size=75%>Build a Power Extender in the fog to search for ship parts.\n\n";
                break;
            case TutorialStage.BuildMortar:
                hudObjText.text = "<b>Build Mortar</b>";
                objWindowText.text = "<size=75%>Build a Mortar to clear the fog away.\n\n";
                break;
            case TutorialStage.BuildPulseDefence:
                hudObjText.text = "<b>Build Pulse Defence</b>";
                objWindowText.text = "<size=75%>Build a Pulse Defence to clear the fog away.\n\n";
                break;
            case TutorialStage.DefenceActivation:
                hudObjText.text = "<b>Activate Defences</b>";
                objWindowText.text = "<size=75%>Activate the defences to clear the fog away. You may like to build more before doing so, however.\n\n";
                break;
            //case TutorialStage.CollectMineralsForUpgrades:
            //    hudObjText.text = "<b>Collect Minerals</b>";
            //    objWindowText.text = "<size=75%>Gather enough mineral resources to upgrade your ship.\n\n" +
            //        $"Target: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))} / {TutorialController.Instance.MineralsForUpgradesGoal} <size=90%><sprite=\"all_icons\" index=2>";
            //    break;
            //case TutorialStage.Upgrades:
            //    hudObjText.text = "<b>Upgrades</b>";
            //    objWindowText.text = "<size=75%>Learn how to use the upgrades system.\n\n";
            //    break;
            case TutorialStage.BuildDefencesInRange:
                hudObjText.text = "<b>Build Defences In Range</b>";
                objWindowText.text = "<size=75%>Build defences within striking range of the fog.\n\n";
                break;
        }
    }
}
