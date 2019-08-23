using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    public static UIController instance = null;

    TextMeshProUGUI powerText, organicText, mineralText, fuelText;
    public GameObject endGame, pauseGame;
    GameObject hudBar;
    public BuildingSelector buildingSelector;
    public BuildingInfo buildingInfo;
    public TextMeshProUGUI endGameText;
    public bool buttonClosed = true;

    private GameObject cursor;
    private Slider powerSlider;
    private int power = 0, powerChange = 0, mineral = 0;
    private float powerVal = 0.0f, mineralVal = 0.0f;
    private float powerTime = 0.0f, mineralTime = 0.0f;
    private bool isCursorOn = false;
    private Image launchButtonImage;
    private MusicFMOD musicFMOD;

    [SerializeField] private Image powerImg;
    [SerializeField] private Sprite[] powerLevelSprites;
    [SerializeField] private Animator powerThresholds;
    [SerializeField] private TextMeshProUGUI objWindowText;
    [SerializeField] private TextMeshProUGUI hudObjText;
    [Header("Tile Colours")]
    [SerializeField] private Color powerLow;
    [SerializeField] private Color powerMedium;
    [SerializeField] private Color powerHigh;
    [SerializeField] private Color powerMax;
    [SerializeField] private Color powerCurrent;
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
    [SerializeField] private Image abilityImage;
    [SerializeField] private Sprite[] abilitySprites;
    [SerializeField] private Button[] abilityButtons;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescText;
    [SerializeField, TextArea] private string[] abilityDescriptions;
    [Header("Upgrades")]
    [SerializeField] private GameObject upgradesCanvas;
    [SerializeField] private Transform upgradesBg;

    ResourceController resourceController = null;
    private int index, temp;
    private MeshRenderer tile;

    private float currentPowerValDisplayed = 0;

    public float CurrentPowerValDisplayed { get => currentPowerValDisplayed; }

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        index = 0;
        temp = 2;

        launchButtonImage = objectiveButton.image;
        FindSliders();
    }

    // Update is called once per frame
    void Update()
    {
        if (resourceController == null)
        {
            resourceController = ResourceController.Instance;
        }

        powerTime += Time.deltaTime;
        mineralTime += Time.deltaTime;
        UpdateResourceText();

        if (Input.GetKeyDown("c"))
        {
            isCursorOn = !isCursorOn;
            cursor.SetActive(isCursorOn);
        }
    }

    // find sliders and text
    void FindSliders()
    {
        powerText = GameObject.Find("PowerLevel").GetComponent<TextMeshProUGUI>();
        mineralText = GameObject.Find("MineralLevel").GetComponent<TextMeshProUGUI>();
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
        objectiveProceedCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[0];

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
                    countdownText.DOFade(0.3f, 0.7f).SetLoops(-1, LoopType.Yoyo);
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
        objectiveProceedCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[2];

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
                launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 1).SetLoops(-1, LoopType.Yoyo);
                buttonClosed = false;
            });
    }

    public void ShowActivateButton()
    {
        objectiveProceedCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[3];

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
                            if (TutorialController.Instance.DefencesOperable())     //If returns false, DefencesOperable() sets up the BuildDefencesInRange() stage
                            {
                                TutorialController.Instance.ActivateDefences();
                            }

                            objectiveButton.enabled = false;
                            CloseButton();
                        });
                    launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 1).SetLoops(-1, LoopType.Yoyo);
                });
    }

    public void CloseButton()
    {
        DOTween.Kill(launchButtonImage);

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(launchButtonImage.DOFade(0, 0.5f))
        .Append(objectiveButtonBG.DOFade(0, 1))
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
        });
    }

    // Ability unlock screen
    public void AbilityUnlock(Ability ability)
    {
        abilityUnlockCanvas.gameObject.SetActive(true);
        switch (ability.AbilityType)
        {
            case AbilityEnum.Artillery:
                abilityImage.sprite = abilitySprites[0];
                abilityNameText.text = "Artillery Blast";
                abilityDescText.text = abilityDescriptions[0];
                abilityButtons[0].interactable = true;
                break;
            case AbilityEnum.BuildingDefence:
                abilityImage.sprite = abilitySprites[1];
                abilityNameText.text = "Defence Mode";
                abilityDescText.text = abilityDescriptions[1];
                abilityButtons[1].interactable = true;
                break;
            case AbilityEnum.FreezeFog:
                abilityImage.sprite = abilitySprites[2];
                abilityNameText.text = "Freeze Fog";
                abilityDescText.text = abilityDescriptions[2];
                abilityButtons[2].interactable = true;
                break;
            case AbilityEnum.Overclock:
                abilityImage.sprite = abilitySprites[3];
                abilityNameText.text = "Overclock";
                abilityDescText.text = abilityDescriptions[3];
                abilityButtons[3].interactable = true;
                break;
            case AbilityEnum.Sonar:
                abilityImage.sprite = abilitySprites[4];
                abilityNameText.text = "Sonar";
                abilityDescText.text = abilityDescriptions[4];
                abilityButtons[4].interactable = true;
                break;
        }
    }

    public void ShowUpgradeWindow()
    {
        upgradesCanvas.SetActive(true);
        upgradesBg.DOScale(1, 0.3f).SetUpdate(true);
        Time.timeScale = 0.1f;
    }

    public void HideUpgradeWindow()
    {
        upgradesBg.DOScale(0.01f, 0.3f).SetUpdate(true).OnComplete(
            delegate
            {
                upgradesCanvas.SetActive(false);
            });
        Time.timeScale = 1;
    }

    public void WinGame()
    {
        DOTween.Kill(launchButtonImage);
        WorldController.Instance.GameWin = true;
        WorldController.Instance.GameOver = true;
        //musicFMOD.GameWinMusic();
    }

    // End Game Method
    public void EndGameDisplay(string text)
    {
        endGameText.text = text;
        endGame.SetActive(true);
    }

    public void ToggleCursor(bool isOn)
    {
        cursor.SetActive(isOn);
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
            powerText.text =  currentPowerValDisplayed + "%" + "\n<size=80%><color=" + colour + powerChange + " %/s</color>";

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

            int mineralChange = resourceController.MineralChange;

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
            case TutorialStage.ExplainMinerals:
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
                hudObjText.text = "<b>Build Generator</b>";
                objWindowText.text = "<size=75%>Build a Power Generator to increase your available power generation.\n\n";
                break;
            case TutorialStage.BuildMoreGenerators:
                hudObjText.text = "<b>Build Generators</b>";
                objWindowText.text = $"<size=75%>Build {TutorialController.Instance.BuiltGeneratorsGoal} Generators to increase your available power generation.\n\n" +
                     $"Target: {ResourceController.Instance.Generators.Count} / {TutorialController.Instance.BuiltGeneratorsGoal} Generators";
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
            case TutorialStage.BuildDefencesInRange:
                hudObjText.text = "<b>Build Defences In Range</b>";
                objWindowText.text = "<size=75%>Build defences within striking range of the fog.\n\n";
                break;
        }
    }
}
