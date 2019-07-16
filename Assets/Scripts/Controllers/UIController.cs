using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    private Image launchBackground;
    private MusicFMOD musicFMOD;

    [SerializeField] Image powerImg;
    [SerializeField] Sprite[] powerLevelSprites;
    [SerializeField] TextMeshProUGUI objWindowText;
    [SerializeField] TextMeshProUGUI hudObjText;
    [SerializeField] Color powerLow;
    [SerializeField] Color powerMedium;
    [SerializeField] Color powerHigh;
    [SerializeField] Color powerCurrent;
    [SerializeField] GameObject launchCanvas;
    [SerializeField] Button launchButton;
    [SerializeField] Sprite[] objectiveButtonSprites;

    ResourceController resourceController = null;
//    private MeshRenderer MeshRend;
    private int index,temp;

    //private GameObject objtest = GameObject.FindGameObjectWithTag("Tile");//.GetComponent<Material>();

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

        FindSliders();

        //cursor = GameObject.Find("Cursor");
        //cursor.SetActive(false);
        //Invoke("FindTile", 5);
        //Tweens in the UI for a smooth bounce in from outside the canvas
        //hudBar = GameObject.Find("HUD");// "HudBar");
        //hudBar.GetComponent<RectTransform>().DOAnchorPosY(200f, 1.5f).From(true).SetEase(Ease.OutBounce);
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
    /*
    void FindTile()
    {
        mr = GameObject.FindGameObjectWithTag("Tile").GetComponent<MeshRenderer>();
    }*/


    // Functions dealing with the drop down objective button
    public void ShowRepairButton()
    {
        launchCanvas.SetActive(true);
        launchBackground = launchCanvas.GetComponentInChildren<Image>();
        launchButtonImage = launchButton.image;
        launchButtonImage.sprite = objectiveButtonSprites[0];

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(launchBackground.DOFillAmount(1, 1))
            .Append(launchButtonImage.DOFade(1, 0.5f))
            .OnComplete(
            delegate
            {
                launchButton.enabled = true;
                buttonClosed = false;
                launchButton.onClick.AddListener(
                    delegate {
                        if (ResourceController.Instance.StoredMineral >= 500)
                        {
                            ResourceController.Instance.StoredMineral -= 500;
                            launchButton.enabled = false;
                            ObjectiveController.Instance.IncrementStage();
                            CloseButton();
                        }
                    });
                launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 1).SetLoops(-1, LoopType.Yoyo);
            });
    }

    public void ShowAttachButton()
    {
        launchCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[1];

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(launchBackground.DOFillAmount(1, 1))
            .Append(launchButtonImage.DOFade(1, 0.5f))
            .OnComplete(
            delegate
            {
                launchButton.enabled = true;
                buttonClosed = false;
                launchButton.onClick.AddListener(
                    delegate {
                        launchButton.enabled = false;
                        ObjectiveController.Instance.IncrementSubStage();
                        CloseButton();
                    });
                launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 1).SetLoops(-1, LoopType.Yoyo);
            });
    }

    public void ShowLaunchButton()
    {
        launchCanvas.SetActive(true);
        launchButtonImage.sprite = objectiveButtonSprites[2];

        Sequence showLaunch = DOTween.Sequence();
        showLaunch.Append(launchBackground.DOFillAmount(1, 1))
            .Append(launchButtonImage.DOFade(1, 0.5f))
            .OnComplete(
            delegate
            {
                launchButton.enabled = true;
                launchButton.onClick.AddListener(delegate {
                    if (ResourceController.Instance.StoredPower >= 500)
                    {
                        ResourceController.Instance.StoredPower -= 500;
                        WinGame();
                    }
                });
                launchButtonImage.DOColor(new Color(0.5f, 0.5f, 0.5f), 1).SetLoops(-1, LoopType.Yoyo);
                buttonClosed = false;
            });
    }

    public void CloseButton()
    {
        DOTween.Kill(launchButtonImage);

        Sequence showLaunch = DOTween.Sequence();
            showLaunch.Append(launchButtonImage.DOFade(0, 0.5f))
            .Append(launchBackground.DOFillAmount(0, 1))
            .OnComplete(
            delegate
            {
                launchButton.onClick.RemoveAllListeners();
                launchButton.enabled = false;
                launchCanvas.SetActive(false);
                buttonClosed = true;
            });
    }

    public void WinGame()
    {
        DOTween.Kill(launchButtonImage);
        WorldController.Instance.GameWin = true;
        WorldController.Instance.GameOver = true;
        musicFMOD.GameWinMusic();
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
        /*
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tile in gameObjects)
        {
            MeshRend = tile.GetComponent<MeshRenderer>();
            if (!flash)
            {
                MeshRend.material.DOColor(newColor, "_BaseColor", 1);
            }else
            {
                MeshRend.material.SetColor("_BaseColor", getAlpha(newColor, 0.1f));
                MeshRend.material.DOColor(newColor, "_BaseColor", 1).SetLoops(-1, LoopType.Yoyo).SetSpeedBased().SetId("tile");
            }
        }*/
        MeshRenderer tile = GameObject.FindGameObjectWithTag("Tile").GetComponent<MeshRenderer>();
        if (!flash)
        {
            tile.sharedMaterial.DOColor(newColor, "_BaseColor", 1);
        }
        else
        {
            tile.sharedMaterial.SetColor("_BaseColor", GetAlpha(newColor, 0.1f));
            tile.sharedMaterial.DOColor(newColor, "_BaseColor", 1).SetLoops(-1, LoopType.Yoyo).SetSpeedBased().SetId("tile");
        } 
    }

    private Color GetAlpha(Color color,float avalue)
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

                //    WorldController.Instance.changePowerTIle(Color.green);

                index = 0;

                colour = "#009900>+";
            }
            else if (powerChange < 0)
            {

                //   WorldController.Instance.changePowerTIle(Color.red);

                index = 1;

                colour = "\"red\">";
            }
            else
            {

                //  WorldController.Instance.changePowerTIle(Color.yellow);

                index = 2;

                colour = "#006273>±";
            }

            if(temp != index)
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
                }
                if (index == 1)
                {
                    ChangeColor(powerCurrent,true);
                 //   StartCoroutine(coroutine);
                }
                else
                {
                    //   StopCoroutine(coroutine);
                    //DOTween.KillAll();
                    DOTween.Kill("tile");
                    ChangeColor(powerCurrent,false);
                }
            }
            

            // update text values
            powerText.text = Mathf.Round(Mathf.Lerp(powerVal, power, powerTime)) + "%" + "\n<size=80%><color=" + colour + powerChange + " %/s</color>";

            float powerCheck = float.Parse(powerText.text.Split('%')[0]) * 0.01f;/// resourceController.MaxPower;

            if (powerCheck > 0 && powerCheck <= .25f && powerImg.sprite != powerLevelSprites[1])
            {

                //  mat.DOColor(powerHigh, 1);
                //mr = GameObject.FindGameObjectWithTag("Tile").GetComponent<MeshRenderer>();
                //mr.material.SetColor("_BaseColor",Color.white);

                // mat = GameObject.FindGameObjectWithTag("Tile").GetComponent<Material>();
                //       powerCurrent = powerHigh;
                //       Debug.Log(mr.material.name);

                powerImg.sprite = powerLevelSprites[1];
            }
            else if (powerCheck > .25f && powerCheck <= .50f && powerImg.sprite != powerLevelSprites[2])
            {

                //mr = GameObject.FindGameObjectWithTag("Tile").GetComponent<MeshRenderer>();
                //mr.material.DOColor(Color.white, 1);
                //      powerCurrent = powerMedium;

                powerImg.sprite = powerLevelSprites[2];
            }
            else if (powerCheck > .50f && powerCheck <= .75f && powerImg.sprite != powerLevelSprites[3])
            {
                powerImg.sprite = powerLevelSprites[3];
            }
            else if (powerCheck > .75f && powerImg.sprite != powerLevelSprites[4])
            {
                powerImg.sprite = powerLevelSprites[4];
            }
            else if (powerCheck == 0 && powerImg.sprite != powerLevelSprites[0])
            {
                powerImg.sprite = powerLevelSprites[0];
            }


            //       mr.material.SetColor("_BaseColor", powerCurrent); //Color.Lerp(prev, new Color(r, g, b, a), 0.5f)

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
            case ObjectiveStage.StorePower:
                hudObjText.text = "Objective: Leave the Planet";
                objWindowText.text = "<b>Leave the Planet</b>\n\n" +
                    "<size=75%>The fog is out to get you! Hurry and gather enough power to leave this wretched planet behind!\n\n" +
                    $"Target: {Mathf.Round(Mathf.Lerp(powerVal, power, powerTime))} / {ObjectiveController.Instance.PowerTarget} <size=90%><sprite=\"all_icons\" index=0>";
                break;
        }
    }

    public void UpdateObjectiveText(TutorialStage stage)
    {
        switch (stage)
        {
            case TutorialStage.None:
            case TutorialStage.CrashLanding:
            case TutorialStage.ShipPartsCrashing:
            case TutorialStage.ZoomBackToShip:
            case TutorialStage.ExplainSituation:
                hudObjText.text = "Objective: Complete the Tutorial";
                objWindowText.text = "<b>Complete the Tutorial</b>\n\n" +
                    "<size=75%>Complete the tutorial and learn to play the game!\n\n";
                break;
            case TutorialStage.MoveCamera:
                hudObjText.text = "Objective: Move Nex";
                objWindowText.text = "<b>Move Nex</b>\n\n" +
                    "<size=75%>Learn how to move your Nexus Drone.\n\n";
                break;
            case TutorialStage.BuildGenerator:
                hudObjText.text = "Objective: Build Generator";
                objWindowText.text = "<b>Build Generator</b>\n\n" +
                    "<size=75%>Build a Power Generator to increase your available power generation.\n\n";
                break;
            case TutorialStage.BuildExtender:
                hudObjText.text = "Objective: Build Power Extender";
                objWindowText.text = "<b>Build Power Extender</b>\n\n" +
                    "<size=75%>Build a Power Extender to extend your range.\n\n";
                break;
            case TutorialStage.BuildBattery:
                hudObjText.text = "Objective: Build Battery";
                objWindowText.text = "<b>Build Battery</b>\n\n" +
                    "<size=75%>Build a Battery to increase your stored power.\n\n";
                break;
            case TutorialStage.IncreasePowerGeneration:
                hudObjText.text = "Objective: More Power Generation";
                objWindowText.text = "<b>More Power Generation</b>\n\n" +
                    $"<size=75%>Build more Power Generators and increase your power output to +{TutorialController.Instance.PowerGainGoal}.\n\n" +
                    $"Target: +{powerChange} / +{TutorialController.Instance.PowerGainGoal} <size=90%><sprite=\"all_icons\" index=0>";
                break;
            case TutorialStage.BuildHarvesters:
                hudObjText.text = "Objective: Build Harvesters";
                objWindowText.text = "<b>Build Harvesters</b>\n\n" +
                    $"<size=75%>Build {TutorialController.Instance.BuiltHarvestersGoal} Mineral Harvesters to replenish your building materials.\n\n" +
                    $"Target: {ResourceController.Instance.Harvesters.Count} / {TutorialController.Instance.BuiltHarvestersGoal} Harvesters";
                break;
            case TutorialStage.BuildAirCannon:
                hudObjText.text = "Objective: Build Air Cannon";
                objWindowText.text = "<b>Build Air Cannon</b>\n\n" +
                    "<size=75%>Build an Air Cannon to protect yourself from the Fog!\n\n";
                break;
            case TutorialStage.BuildFogRepeller:
                hudObjText.text = "Objective: Build Fog Repeller";
                objWindowText.text = "<b>Build Fog Repeller</b>\n\n" +
                    "<size=75%>Build a Fog Repeller to protect yourself from the Fog!\n\n";
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
