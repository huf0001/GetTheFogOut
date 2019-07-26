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
    [SerializeField] private Image powerThresholdsBg;
    [SerializeField] private Image powerThresholds;
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

    ResourceController resourceController = null;
    private int index, temp;
    private MeshRenderer tile;

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

        //cursor = GameObject.Find("Cursor");
        //cursor.SetActive(false);
        //Invoke("FindTile", 5);
        //Tweens in the UI for a smooth bounce in from outside the canvas
        //hudBar = GameObject.Find("HUD");// "HudBar");
        //hudBar.GetComponent<RectTransform>().DOAnchorPosY(200f, 1.5f).From(true).SetEase(Ease.OutBounce);
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
        Sequence sequence = DOTween.Sequence();
        sequence.Insert(0, powerThresholdsBg.DOFillAmount(1, 0.3f));
        sequence.Insert(0.05f, powerThresholds.DOFillAmount(1, 0.25f));
    }

    public void HidePowerThresholds()
    {
        powerThresholdsBg.DOFillAmount(0, 0.3f);
        powerThresholds.DOFillAmount(0, 0.2f);
    }

    // Functions dealing with the drop down objective button
    public void ShowRepairButton(string controller)
    {
        objectiveProceedCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[0];

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(objectiveButtonBG.DOFillAmount(1, 1))
            .Append(launchButtonImage.DOFade(1, 0.5f))
            .OnComplete(
            delegate
            {
                objectiveButton.enabled = true;
                buttonClosed = false;
                objectiveButton.onClick.AddListener(
                    delegate
                    {
                        if (ResourceController.Instance.StoredMineral >= TutorialController.Instance.CollectedMineralsGoal)
                        {
                            ResourceController.Instance.StoredMineral -= TutorialController.Instance.CollectedMineralsGoal;
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
        showLaunch.Append(objectiveButtonBG.DOFillAmount(1, 1))
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
        sequence.Insert(1.5f, countdownSliderBG.DOFillAmount(1, 1))
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
        //Sequence sequence = DOTween.Sequence();
        //sequence.Append()
        countdownSliderCG.alpha = 0;
        countdownSliderBG.fillAmount = 0;
    }

    public void ShowLaunchButton()
    {
        objectiveProceedCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[2];

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(objectiveButtonBG.DOFillAmount(1, 1))
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
        showLaunch.Append(objectiveButtonBG.DOFillAmount(1, 1))
            .Append(launchButtonImage.DOFade(1, 0.5f))
            .OnComplete(
                delegate
                {
                    objectiveButton.enabled = true;
                    buttonClosed = false;
                    objectiveButton.onClick.AddListener(
                        delegate {
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
        .Append(objectiveButtonBG.DOFillAmount(0, 1))
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
            powerText.text = Mathf.Round(Mathf.Lerp(powerVal, power, powerTime)) + "%" + "\n<size=80%><color=" + colour + powerChange + " %/s</color>";

            float powerCheck = float.Parse(powerText.text.Split('%')[0]) * 0.01f;/// resourceController.MaxPower;

            if (powerCheck > 0 && powerCheck <= .25f && powerImg.sprite != powerLevelSprites[1])
            {
                powerImg.sprite = powerLevelSprites[1];
                index = 1;
            }
            else if (powerCheck > .25f && powerCheck <= .50f && powerImg.sprite != powerLevelSprites[2])
            {
                powerImg.sprite = powerLevelSprites[2];
                index = 2;
            }
            else if (powerCheck > .50f && powerCheck <= .75f && powerImg.sprite != powerLevelSprites[3])
            {
                powerImg.sprite = powerLevelSprites[3];
                index = 0;
            }
            else if (powerCheck > .75f && powerImg.sprite != powerLevelSprites[4])
            {
                powerImg.sprite = powerLevelSprites[4];
                index = 3;
            }
            else if (powerCheck == 0 && powerImg.sprite != powerLevelSprites[0])
            {
                powerImg.sprite = powerLevelSprites[0];
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

            mineralText.text = Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime)) + " units\n<color=" + colour + mineralChange + "</color>";
        }
    }

    public void UpdateObjectiveText(ObjectiveStage stage)
    {
        switch (stage)
        {
            case ObjectiveStage.None:
                hudObjText.text = "Objective: Complete the Tutorial";
                objWindowText.text = "<b>Complete the Tutorial</b>\n\n" +
                    "<size=75%>Proceed through the tutorial and learn to play the game!\n\n";
                break;
            case ObjectiveStage.HarvestMinerals:
                hudObjText.text = "Objective: Repair the Hull";
                objWindowText.text = "<b>Repair the Hull</b>\n\n" +
                    "<size=75%>Gather enough mineral resources to repair your ship's hull.\n\n" +
                    $"Target: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))} / {ObjectiveController.Instance.MineralTarget} <size=90%><sprite=\"all_icons\" index=2>";
                break;
            case ObjectiveStage.RecoverPart:
                string nf = "Not Found";
                string f = "Found";
                hudObjText.text = "Objective: Recover the Thruster";
                objWindowText.text = "<b>Recover the Thruster</b>\n\n" +
                    "<size=75%>Push your way through the fog to find the missing thruster from your ship.\n\n" +
                    "Thrusters: " + (WorldController.Instance.GetShipComponent(ShipComponentsEnum.Thrusters).Collected ? f : nf);
                break;
            case ObjectiveStage.SurvivalStage:
                hudObjText.text = "Objective: Leave the Planet";
                objWindowText.text = "<b>Leave the Planet</b>\n\n" +
                    "<size=75%>Your ship is undergoing repairs. Protect yourself from the fog until you are ready to leave, then blast off this wretched planet!\n\n" +
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
                hudObjText.text = "Objective: Complete the Tutorial";
                objWindowText.text = "<b>Complete the Tutorial</b>\n\n" +
                    "<size=75%>Complete the tutorial and learn to play the game!\n\n";
                break;
            case TutorialStage.CameraControls:
                hudObjText.text = "Objective: Move Nex";
                objWindowText.text = "<b>Move Nex</b>\n\n" +
                    "<size=75%>Learn how to move your Nexus Drone.\n\n";
                break;
            case TutorialStage.BuildHarvesters:
            case TutorialStage.BuildHarvestersExtended:
                hudObjText.text = "Objective: Build Harvesters";
                objWindowText.text = "<b>Build Harvesters</b>\n\n" +
                    $"<size=75%>Build {TutorialController.Instance.BuiltHarvestersExtendedGoal} mineral Harvesters to collect building materials.\n\n" +
                    $"Target: {ResourceController.Instance.Harvesters.Count} / {TutorialController.Instance.BuiltHarvestersExtendedGoal} Harvesters";
                break;
            case TutorialStage.BuildExtender:
                hudObjText.text = "Objective: Build Power Extender";
                objWindowText.text = "<b>Build Power Extender</b>\n\n" +
                    "<size=75%>Build a Power Extender to reach additional mineral nodes.\n\n";
                break;
            case TutorialStage.MouseOverPowerDiagram:
                hudObjText.text = "Objective: Look at Power Diagram";
                objWindowText.text = "<b>Look at Power Diagram</b>\n\n" +
                                     "<size=75%>Move the mouse over the power icon to view the diagram explaining how power works.\n\n";
                break;
            case TutorialStage.BuildGenerator:
                hudObjText.text = "Objective: Build Generator";
                objWindowText.text = "<b>Build Generator</b>\n\n" +
                    "<size=75%>Build a Power Generator to increase your available power generation.\n\n";
                break;
            case TutorialStage.BuildMoreGenerators:
                hudObjText.text = "Objective: Build Generators";
                objWindowText.text = "<b>Build Generators</b>\n\n" +
                     $"<size=75%>Build {TutorialController.Instance.BuiltGeneratorsGoal} Generators to increase your available power generation.\n\n" +
                     $"Target: {ResourceController.Instance.Generators.Count} / {TutorialController.Instance.BuiltGeneratorsGoal} Generators";
                break;
            case TutorialStage.CollectMinerals:
                hudObjText.text = "Objective: Repair the Hull";
                objWindowText.text = "<b>Repair the Hull</b>\n\n" +
                                     "<size=75%>Gather enough mineral resources to repair your ship's hull.\n\n" +
                                     $"Target: {Mathf.Round(Mathf.Lerp(mineralVal, mineral, mineralTime))} / {TutorialController.Instance.CollectedMineralsGoal} <size=90%><sprite=\"all_icons\" index=2>";
                break;
            case TutorialStage.CollectSonar:
                hudObjText.text = "Objective: Recover Canister";
                objWindowText.text = "<b>Recover Canister</b>\n\n" +
                                     "<size=75%>Retrieve that canister lying near the ship.<size=90%><sprite=\"all_icons\" index=2>";
                break;
            case TutorialStage.ActivateSonar:
                hudObjText.text = "Objective: Activate Sonar";
                objWindowText.text = "<b>Activate Sonar</b>\n\n" +
                                     "<size=75%>Activate the Sonar and find the ship's remaining missing parts.<size=90%><sprite=\"all_icons\" index=2>";
                break;
            case TutorialStage.BuildExtenderInFog:
                hudObjText.text = "Objective: Build Power Extender";
                objWindowText.text = "<b>Build Power Extender</b>\n\n" +
                                     "<size=75%>Build a Power Extender in the fog to search for ship parts.\n\n";
                break;
            case TutorialStage.BuildMortar:
                hudObjText.text = "Objective: Build Mortar";
                objWindowText.text = "<b>Build Mortar</b>\n\n" +
                    "<size=75%>Build a Mortar to clear the fog away.\n\n";
                break;
            case TutorialStage.BuildPulseDefence:
                hudObjText.text = "Objective: Build Pulse Defence";
                objWindowText.text = "<b>Build Pulse Defence</b>\n\n" +
                    "<size=75%>Build a Pulse Defence to clear the fog away.\n\n";
                break;
            case TutorialStage.DefenceActivation:
                hudObjText.text = "Objective: Activate Defences";
                objWindowText.text = "<b>Activate Defences</b>\n\n" +
                    "<size=75%>Activate the defences to clear the fog away. You may like to build more before doing so, however.\n\n";
                break;
            case TutorialStage.BuildDefencesInRange:
                hudObjText.text = "Objective: Build Defences In Range";
                objWindowText.text = "<b>Build Defences In Range</b>\n\n" +
                    "<size=75%>Build defences within striking range of the fog.\n\n";
                break;
        }
    }

    /*  no use, tween value issues
public IEnumerator FadeTo(float aValue,float aTime)
{
    bool toggle = true;
    while (true)
    {
        Color c = powerLow;
        Color newColor;
        float alpha = c.a;
        if (toggle)
        {
            for (float t = 1.0f; t > 0.0f; t -= Time.deltaTime / aTime)
            {
                newColor = new Color(c.r, c.g, c.b, Mathf.Lerp(aValue, alpha, t));
                changeColor(newColor,true);
                yield return null;
            }
            toggle = !toggle;
            yield return new WaitForSeconds(2);
        }
        else
        {
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
            {
                newColor = new Color(c.r, c.g, c.b, Mathf.Lerp(aValue, 0.6f, t));
                changeColor(newColor,true);
                yield return null;
            }
            toggle = !toggle;
            yield return new WaitForSeconds(2);
        }
    }
}
*/
}
