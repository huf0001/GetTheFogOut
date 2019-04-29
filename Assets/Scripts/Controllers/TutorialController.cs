using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.UI;

public enum TutorialStage
{
    None,
    CrashLanding,
    ShipPartsCrashing,
    ZoomBackToShip,
    ExplainSituation,
    ExplainBuildingPlacement,
    BuildHarvester,
    BuildGenerator,
    BuildRelay,
    FogIsHazard,
    BuildClusterFan,
    Finished
}

public enum ButtonType
{
    None,
    BuildSelect,
    Battery,
    ClusterFan,
    Generator,
    Harvester,
    Relay,
    Destroy
}

[Serializable]
public class KeyDialoguePair
{
    //Serialized Fields
    [SerializeField] private string key;
    [SerializeField] private List<string> dialogue;

    //Public Properties
    public string Key { get => key; }
    public List<string> Dialogue { get => dialogue; }

    public KeyDialoguePair(string k, List<string> d)
    {
        key = k;
        dialogue = d;
    }
}

public class TutorialController : DialogueBoxController
{
    //Fields---------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private bool skipTutorial = true;

    [SerializeField] private DialogueBox aiText;
    [SerializeField] private ResourceNode harvesterResource;
    [SerializeField] private Landmark generatorLandmark;
    [SerializeField] private Landmark relayLandmark;
    [SerializeField] private Landmark clusterFanLandmark;
    [SerializeField] private Locatable buildingTarget;

    [SerializeField] private btnTutorial btnBuildSelect;
    [SerializeField] private btnTutorial btnBuildHarvester;
    [SerializeField] private btnTutorial btnBuildGenerator;
    [SerializeField] private btnTutorial btnBuildRelay;
    [SerializeField] private btnTutorial btnBuildClusterFan;

    [SerializeField] private Color uiNormalColour;
    [SerializeField] private Color uiHighlightColour;

    [SerializeField] private List<KeyDialoguePair> dialogue;

    //Non-Serialized Fields
    private TutorialStage tutorialStage = TutorialStage.CrashLanding;
    private int subStage = 1;
    private BuildingType currentlyBuilding = BuildingType.None;

    private TileData currentTile = null;
    private int currentTileX = 0;
    private int currentTileZ = 0;

    private bool buttonClicked = false;
    private btnTutorial btnCurrent;
    private ButtonType currentlyLerping;

    private DecalProjectorComponent targetDecal = null;
    private float decalMin = 1.5f;
    private float decalMax = 3f;
    private float lerpMultiplier = 1f;
    private float lerpProgress = 0f;
    private bool lerpForward = true;

    private Dictionary<string, List<string>> dialogueDictionary = new Dictionary<string, List<string>>();
    private bool dialogueSent = false;

    //Public Properties
    public TutorialStage TutorialStage { get => tutorialStage; }
    public TileData CurrentTile { get => currentTile; }
    public BuildingType CurrentlyBuilding { get => currentlyBuilding; }
    public ButtonType CurrentlyLerping { get => currentlyLerping; }
    public Color UINormalColour { get => uiNormalColour; }
    public Color UIHighlightColour { get => uiHighlightColour; }

    //Start-Up Methods-----------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        if (skipTutorial)
        {
            tutorialStage = TutorialStage.Finished;
        }
        else
        {
            targetDecal = buildingTarget.GetComponent<DecalProjectorComponent>();
            SetupDialogue();
        }
    }

    //private void SetupDialogue()
    //{
    //    dialogueDictionary["explain situation"] = new List<string>();
    //    dialogueDictionary["explain situation"].Add("Well then! That landing was a bit rougher than I had hoped. I will scan the ship and our current surroundings. (Click to Continue)");
    //    dialogueDictionary["explain situation"].Add("Scanning . . .");
    //    dialogueDictionary["explain situation"].Add("Scan complete.");
    //    dialogueDictionary["explain situation"].Add("Bad news. The ship has sustained substantial damaged and three core components are missing.");
    //    dialogueDictionary["explain situation"].Add("Good news, the planet is only inhabited by flora. Fauna is not present. You are currently the only living animal to set foot on this planet.");

    //    dialogueDictionary["explain building placement"] = new List<string>();
    //    dialogueDictionary["explain building placement"].Add("You are going to need to find those pieces off your ship if we are going to get home. Let me start the back-up generator.");
    //    dialogueDictionary["explain building placement"].Add("Done. But, it does not look like that will be enough to get us by. You are going to need to build your way out of here.");
    //    dialogueDictionary["explain building placement"].Add("You see that shiny lump of minerals over there? It is a common resource in our universe, and it is used in most buildings back home.");

    //    dialogueDictionary["build cluster fan 1"] = new List<string>();
    //    dialogueDictionary["build cluster fan 1"].Add("To help you in your journey, I am going to download important building blueprints.");
    //    dialogueDictionary["build cluster fan 1"].Add("Downloading . . .");
    //    dialogueDictionary["build cluster fan 1"].Add("Download complete.");
    //    dialogueDictionary["build cluster fan 1"].Add("The blueprints have been implemented. Click on the icon to open the building menu.");

    //    dialogueDictionary["build cluster fan 2"] = new List<string>();
    //    dialogueDictionary["build cluster fan 2"].Add("The first blueprint to be used will be the harvester.");
    //    dialogueDictionary["build cluster fan 2"].Add("Use the mouse to select the harvester. Other buildings will become available to you once you have enough materials.");

    //    dialogueDictionary["build cluster fan 3"] = new List<string>();
    //    dialogueDictionary["build cluster fan 3"].Add("Now, place the harvester on top of the mineral by clicking it.");

    //    dialogueDictionary["build generator"] = new List<string>();
    //    dialogueDictionary["build generator"].Add("Great! Seems the bump on your head from the landing did not do as much damage as I thought. Now that you have some minerals, you can build another power generator.");

    //    dialogueDictionary["build harvester"] = new List<string>();
    //    dialogueDictionary["build harvester"].Add("Build Harvester (Click to Continue)");

    //    dialogueDictionary["build relay"] = new List<string>();
    //    dialogueDictionary["build relay"].Add("Build Relay (Click to Continue)");
    //}

    //Tutorial Stage Management Methods------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        if (tutorialStage != TutorialStage.Finished)
        {
            if (dialogueRead)
            {
                dialogueSent = false;
                subStage += 1;
                ResetDialogueRead();
            }

            if (buttonClicked)
            {
                buttonClicked = false;
                btnCurrent.ReportClick = false;
                subStage += 1;
            }

            if (targetDecal.enabled)
            {
                LerpDecal();
            }
        }

        CheckTutorialStage();
    }

    private void CheckTutorialStage()
    {
        switch (tutorialStage)
        {
            case TutorialStage.CrashLanding:
                CrashLanding();
                break;
            case TutorialStage.ShipPartsCrashing:
                ShipPartsCrashing();
                break;
            case TutorialStage.ZoomBackToShip:
                ZoomBackToShip();
                break;
            case TutorialStage.ExplainSituation:
                ExplainSituation();
                break;
            case TutorialStage.ExplainBuildingPlacement:
                ExplainBuildingPlacement();
                break;
            case TutorialStage.BuildHarvester:
                BuildHarvester();
                break;
            case TutorialStage.BuildGenerator:
                BuildGenerator();
                break;
            case TutorialStage.BuildRelay:
                BuildRelay();
                break;
            case TutorialStage.FogIsHazard:
                FogIsHazard();
                break;
            case TutorialStage.BuildClusterFan:
                BuildClusterFan();
                break;
            case TutorialStage.Finished:
                //End tutorial, game is fully responsive to player's input.
                break;
        }
    }

    //Tutorial Stage 1: Start Scene Animation
    //Ship crash lands, intro scene
    private void CrashLanding()
    {
        //Run animation for the ship crash landing

        tutorialStage = TutorialStage.ShipPartsCrashing;
    }

    //Ship parts crash, x3 different scenes
    private void ShipPartsCrashing()
    {
        //Run animation / camera movement for showing the different ship parts crashing

        tutorialStage = TutorialStage.ZoomBackToShip;
    }

    //Zoom back to ship
    private void ZoomBackToShip()
    {
        //Run camera movement to move camera back to the hub

        tutorialStage = TutorialStage.ExplainSituation;
    }

    //Tutorial Stage 2: AI Explains Basic Building Placement
    //AI explains player's situation
    private void ExplainSituation()
    {
        if (subStage == 1)
        {
            SendDialogue("explain situation", 2);
        }
        else if (subStage == 2)
        {
            tutorialStage = TutorialStage.BuildHarvester;
            currentlyBuilding = BuildingType.Harvester;
            subStage = 1;
        }
    }

    private void ExplainBuildingPlacement()
    {
        if (subStage == 1)
        {
            SendDialogue("explain building placement", 1);
        }
        else if (subStage == 2)
        {
            tutorialStage = TutorialStage.BuildHarvester;
            currentlyBuilding = BuildingType.Harvester;
            subStage = 1;
        }
    }

    //AI explains how to build a harvester and how they work
    private void BuildHarvester()
    {
        if (subStage == 1)
        {
            SendDialogue("build harvester 1", 1);
        }
        else if (subStage == 2)
        {
            //Display UI element prompting player to click the building selector button
            //lerpUI = true;
            btnCurrent = btnBuildSelect;
            btnCurrent.ReportClick = true;
            currentlyLerping = ButtonType.BuildSelect;
        }
        else if (subStage == 3)
        {
            //Reset UI lerping
            btnCurrent.ReportClick = false;

            //Display UI element prompting player to select the harvester
            btnCurrent = btnBuildHarvester;
            btnCurrent.ReportClick = true;
            currentlyLerping = ButtonType.Harvester;
        }
        else if (subStage == 4)
        {
            //Turn off UI lerping
            btnCurrent.ReportClick = false;
            currentlyLerping = ButtonType.None;

            //Get location of resource node
            GetLocationOf(harvesterResource);

            //Display UI element prompting player to build a harvester on this resource node
            ActivateTarget(harvesterResource);

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 5)
        {
            //Check if the player has built the harvester
            if (BuiltCurrentlyBuilding())
            {
                //Turn off UI element prompting player to build a harvester on the resource node
                tutorialStage = TutorialStage.BuildGenerator;
                currentlyBuilding = BuildingType.Generator;
                subStage = 1;
                DeactivateTarget();
            }
        }
    }

    //AI helps player build a power generator and explains how they work
    private void BuildGenerator()
    {
        if (subStage == 1)
        {
            SendDialogue("build generator", 2);
        }
        else if (subStage == 2)
        {
            //Get tile
            GetLocationOf(generatorLandmark);

            //Display UI element prompting player to build a generator on this tile
            ActivateTarget(generatorLandmark);

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 3)
        {
            if (BuiltCurrentlyBuilding())
            {
                tutorialStage = TutorialStage.BuildRelay;
                currentlyBuilding = BuildingType.Relay;
                subStage = 1;
                DeactivateTarget();
            }
        }
    }

    //AI helps player build a relay and explains how they work
    private void BuildRelay()
    {
        if (subStage == 1)
        {
            SendDialogue("build relay", 2);
        }
        else if (subStage == 2)
        {
            //Get tile
            GetLocationOf(relayLandmark);

            //Display UI element prompting player to build a relay on this tile
            ActivateTarget(relayLandmark);

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 3)
        {
            if (BuiltCurrentlyBuilding())
            {
                tutorialStage = TutorialStage.FogIsHazard;
                currentlyBuilding = BuildingType.None;
                subStage = 1;
                DeactivateTarget();
            }
        }
    }

    //Tutorial Stage 3: AI Explains The Fog
    //AI realises the fog is a hazard
    private void FogIsHazard()
    {
        //Spawn fog units around hub

        //Get AI dialogue
        //Activate DialogueBox, passing dialogue to it

        tutorialStage = TutorialStage.BuildClusterFan;
        currentlyBuilding = BuildingType.Defence;
    }

    //AI tells player to build a cluster fan and explains how they work
    private void BuildClusterFan()
    {
        if (subStage == 1)
        {
            SendDialogue("build cluster fan", 1);
        }
        else if (subStage == 2)
        {
            //Get tile
            GetLocationOf(clusterFanLandmark);

            //Display UI element prompting player to build a cluster fan on this tile
            ActivateTarget(clusterFanLandmark);

            //Progress to next SubStage
            subStage += 1;
        }
        else if (subStage == 3)
        {
            if (BuiltCurrentlyBuilding())
            {
                tutorialStage = TutorialStage.Finished;
                currentlyBuilding = BuildingType.None;
                subStage = 1;
                DeactivateTarget();
            }
        }
    }

    //Utility Methods------------------------------------------------------------------------------

    private void SendDialogue(string dialogueKey, float invokeDelay)
    {
        if (!dialogueSent)
        {
            //Activate DialogueBox, passing dialogue to it
            aiText.ActivateDialogueBox(GetDialogue(dialogueKey), invokeDelay);

            //Set dialogueSent to true so that the dialogue box isn't being repeatedly activated
            dialogueSent = true;
        }
    }

    private List<string> GetDialogue(string key)
    {
        foreach (KeyDialoguePair p in dialogue)
        {
            if (p.Key == key)
            {
                return p.Dialogue;
            }
        }

        Debug.Log("Dialogue key '" + key + "' is invalid.");
        return null;
    }

    public void RegisterButtonClicked()
    {
        buttonClicked = true;
    }

    private void GetLocationOf(Locatable l)
    {
        currentTile = l.Location;

        if (currentTile == null)
        {
            Debug.Log("TutorialController.CurrentTile is null");
        }
        else
        {
            currentTileX = currentTile.X;
            currentTileZ = currentTile.Z;
        }
    }

    private void ActivateTarget(Locatable l)
    {
        buildingTarget.Location = currentTile;
        buildingTarget.transform.position = l.transform.position;
        targetDecal.enabled = true;

        lerpProgress = 0f;
        lerpForward = true;
    }

    private void LerpDecal()
    {
        float lerped = Mathf.Lerp(decalMin, decalMax, lerpProgress);
        targetDecal.m_Size.Set(lerped, 1, lerped);

        UpdateLerpValues();

        //Forces the decal to show the lerping
        targetDecal.enabled = false;
        targetDecal.enabled = true;
    }

    private bool BuiltCurrentlyBuilding()
    {
        if (currentTile != null)
        {
            if (currentTile.Building != null)
            {
                if (currentTile.Building.BuildingType == currentlyBuilding)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void DeactivateTarget()
    {
        targetDecal.enabled = false;
    }

    private void UpdateLerpValues()
    {
        if (lerpForward)
        {
            lerpProgress += Time.deltaTime * lerpMultiplier;
        }
        else
        {
            lerpProgress -= Time.deltaTime * lerpMultiplier;
        }

        if (lerpProgress > 1)
        {
            lerpProgress = 1;
            lerpForward = false;
        }
        else if (lerpProgress < 0)
        {
            lerpProgress = 0;
            lerpForward = true;
        }
    }
}

