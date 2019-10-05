using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    // Fields -------------------------------------------------------------------

    // Serialized fields
    //[SerializeField] private int generatorCount = 0;
    //[SerializeField] private int generatorInterval = 5;
    //[SerializeField] private WarningScript warningScript;

    [SerializeField] private TileData currentTile;

    // Non-serialized fields
    private Hub hub = null;
    private ResourceController resourceController = null;
    private TowerManager towerManager;
    private FloatingTextController floatingTextController;
    private TutorialController tutorialController;
    private GameObject PointAtObj;
    List<GameObject> collisionList = new List<GameObject>();
    private bool reportTutorialClick = false;
    public TileData hoveredTile;
    public bool isBuildAvaliable = true;
    private bool hovertoggle = true;
    private Camera cam;
    private Keyboard kb;
    private Color curr;
    // Test for game pause/over mouse to not build/destroy buildings
    // private bool isStopped = false;

    // Public Properties
    public static MouseController Instance { get; protected set; }
    public Hub Hub { get => hub; set => hub = value; }
    public bool ReportTutorialClick { get => reportTutorialClick; set => reportTutorialClick = value; }
    public TileData CurrentTile { get => currentTile; }

    // Start / Update Unity Methods ------------------------------------------------------

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should not be more than one MouseController");
        }

        Instance = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        WorldController.Instance.Inputs.InputMap.Build.performed += ctx => UpdatePlacingAlt();
        resourceController = ResourceController.Instance;
        hub = WorldController.Instance.Hub;
        towerManager = GetComponent<TowerManager>();
        floatingTextController = GetComponent<FloatingTextController>();
        tutorialController = GetComponent<TutorialController>();
        cam = Camera.main;
        kb = InputSystem.GetDevice<Keyboard>();
        curr = WorldController.Instance.normalTile.GetColor("_BaseColor");
    }

    // Update is called once per frame
    void Update()
    {
        selectedTile();
        checkforDeleteKey();
    }

    void checkforDeleteKey()
    {
        if (UIController.instance.buildingInfo.Visible)
        {
            if (kb.deleteKey.wasPressedThisFrame)
            {
                Building removeBuilding = ReturnCost(currentTile);
                RemoveBulding(removeBuilding);
            }
        }
    }

    void UpdateHoveredTile(bool toggle)
    {
        if (toggle)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!WorldController.Instance.IsPointerOverGameObject() && Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("GridPlanes")))
            {
                if (WorldController.Instance.TileExistsAt(hit.point))
                {
                    TileData t = WorldController.Instance.GetTileAt(hit.point);
                    //      Color test = t.plane.GetComponent<Renderer>().material.GetColor("_BaseColor");
                    if (hovertoggle)
                    {
                     //   hovertoggle = false;
                        //    changeTileMaterial(WorldController.Instance.normalTile);
                        changeTileMaterial(false);

                        if (!t.buildingChecks.collectable)
                        {
                            hoveredTile = t;
                       //     changeTileMaterial(WorldController.Instance.hoverTile);
                            changeTileMaterial(true);
                        }
                        else
                            //  changeTileMaterial(WorldController.Instance.normalTile);
                            changeTileMaterial(false);
                    }
                    else
                    {
                        if (!curr.Equals(UIController.instance.powerCurrent))
                        {
                            hovertoggle = true;
                            //   changeTileMaterial(WorldController.Instance.normalTile);
                            changeTileMaterial(false);
                            curr = UIController.instance.powerCurrent;

                        }else if (!(hoveredTile.Equals(t)))
                            {
                                hovertoggle = true;
                                /*
                                if (hoveredTile.plane == null)
                                {
                                    Debug.Log("tst");
                                    hoveredTile = WorldController.Instance.GetTileAt(hit.point);
                                    changeTileMaterial(WorldController.Instance.hoverTile);
                                }
                                else*/
                            
                                if (!t.buildingChecks.collectable)
                                {
                                //    changeTileMaterial(WorldController.Instance.normalTile);
                                changeTileMaterial(false);
                                }
                        }
                    }
                }
            }
            else
            {
                //   changeTileMaterial(WorldController.Instance.normalTile);
                changeTileMaterial(false);
            }
        }
        else
        {
            if (currentTile != null)
            {
                if (currentTile.plane)
                {
                    //    changeTileMaterial(WorldController.Instance.normalTile);
                    changeTileMaterial(false);
                    if ((!currentTile.plane.GetComponent<Renderer>().material.Equals(WorldController.Instance.hoverTile)))
                    {
                        currentTile.plane.GetComponent<Renderer>().material.SetColor("_BaseColor", getHoveredTileColor());
                    }
                    hoveredTile = currentTile;
                }
            }
            else
            {
              //  currentTile = hoveredTile;
            }
        }
    }
    public void changeTileMaterial(bool ison)
    {
        if (hoveredTile != null)
        {
            if (hoveredTile.plane != null)
            {
                if(ison)
                {
                    hoveredTile.plane.GetComponent<Renderer>().material.SetColor("_BaseColor", getHoveredTileColor());
                }else
                {
                    hoveredTile.plane.GetComponent<Renderer>().material = WorldController.Instance.normalTile;
                }
            }
        }
    }

    public Color getHoveredTileColor()
    {
        Color current = UIController.instance.powerCurrent;
        current.a = 0.8f;
        return current;
    }
    /*
    public void changeTileMaterial(Material mat)
    {
        if (hoveredTile != null)
        {
            if (hoveredTile.plane != null)
            {
                if (mat.Equals(WorldController.Instance.hoverTile))
                {
                    Color newColor = hoveredTile.plane.GetComponent<Renderer>().material.GetColor("_BaseColor");
                    newColor.a = 0.8f;
                    hoveredTile.plane.GetComponent<Renderer>().material.SetColor("_BaseColor", newColor);
                }
                else
                    hoveredTile.plane.GetComponent<Renderer>().material = mat;
            }
        }
    }
    */
    void selectedTile()
    {
        if (UIController.instance.buildingSelector.Visible || UIController.instance.buildingInfo.Visible)
        {
            UpdateHoveredTile(false);
            if (WorldController.Instance.Inputs.InputMap.CancelBuildingMenu.triggered && UIController.instance.buildingSelector.Visible)
            {
                UIController.instance.buildingSelector.ToggleVisibility();
            }
        }
        else
        {
            UpdateHoveredTile(true);
        }
    }
    // Mouse Building Placement -----------------------------------------------------------------

    void UpdatePlacingAlt()
    {
        // code based somewhat off:
        //"https://forum.unity.com/threads/click-object-behind-other-object.480815/"

        UIController.instance.buildingSelector.freezeCam();

        if (Time.timeScale == 1f && !WorldController.Instance.IsPointerOverGameObject() && isBuildAvaliable == true)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(new string[] { "Tiles", "Collectables", "Buildings" })) && WorldController.Instance.TileExistsAt(hit.point))
            {
                if (UIController.instance.buildingInfo.Visible)
                {
                    //   changeTileMaterial(WorldController.Instance.normalTile);
                    //   changeTileMaterial(false);
                    towerManager.CancelBuild();
                }
                else if (hit.collider.gameObject.layer == 15)
                {
                    hit.collider.GetComponent<Collectable>()?.CollectAbility();
                    hit.collider.GetComponent<ShipComponent>()?.Collect();
                }
                else if (hit.collider.tag == "Building")
                {
                    if (UIController.instance.buildingSelector.Visible)
                    {
                        UIController.instance.buildingSelector.ToggleVisibility();
                    }
                    currentTile = WorldController.Instance.GetTileAt(hit.point);
                    if (!UIController.instance.buildingInfo.Visible)
                    {
                        Building b = hit.collider.gameObject.GetComponentInChildren<Building>();
                        UIController.instance.buildingInfo.ShowInfo(b);
                    }
                    else
                    {
                        UIController.instance.buildingInfo.HideInfo();
                    }
                }
                else if (hit.collider.name == "pCube31" || hit.collider.name == "Ship:pCube5" || hit.collider.name == "polySurface22") // check for hub
                {
                    currentTile = WorldController.Instance.GetTileAt(36, 34);
                    if (UIController.instance.buildingSelector.Visible)
                    {
                        changeTileMaterial(false);
                        UIController.instance.buildingSelector.ToggleVisibility();
                    }

                    if (!UIController.instance.buildingInfo.Visible)
                    {
                        Building b = Hub.Instance;
                        UIController.instance.buildingInfo.ShowInfo(b);
                    }
                    else
                    {
                        UIController.instance.buildingInfo.HideInfo();
                    }

                }
                else
                {
                    TileData tile = WorldController.Instance.GetTileAt(hit.point);

                    if (tile.isBuildable)
                    {
                        if (WorldController.Instance.ActiveTiles.Contains(tile) && TutorialController.Instance.TileAllowed(tile/*, !UIController.instance.buildingSelector.Visible*/))
                        {
                            if (!AbilityController.Instance.checkTrigger())
                            {
                                if (!UIController.instance.buildingSelector.Visible)
                                {
                                    WorldController.Instance.CheckTileContents(tile);
                                }
                                else if (tile != currentTile)
                                {
                                    UIController.instance.buildingSelector.QuickCloseMenu();
                                    WorldController.Instance.CheckTileContents(tile);
                                }
                                else
                                {
                                    towerManager.CancelBuild();
                                }
                            }                            

                            if (reportTutorialClick)
                            {
                                TutorialController.Instance.RegisterMouseClicked();
                            }
                        }
                        else if (UIController.instance.buildingSelector.Visible)
                        {
                            towerManager.CancelBuild();
                        }

                        currentTile = tile;
                    }
                }
            }
        }
    }

    // Refunds the cost for a building that is going to be destroyed. 
    // Returns the building that should be destroyed.
    public Building ReturnCost(TileData tile)
    {
        int returnCost;
        if (tile.Building != null)
        {
            Building building = tile.Building;
            if (building.BuildingType != BuildingType.Hub)
            {
                returnCost = building.MineralCost;
                if (building.BuildingType != BuildingType.Extender)
                {
                    if (building.Health != building.MaxHealth)
                    {
                        if (building.Health < 40 && building.Health > 25)
                        {
                            returnCost = Mathf.RoundToInt(returnCost / 1.4f);
                        }

                        if (building.Health < 20 && building.Health > 5)
                        {
                            returnCost = Mathf.RoundToInt(returnCost * 0.5f);
                        }
                    }
                }

                // add required resources
                resourceController.StoredPower += building.PowerCost;
                resourceController.StoredMineral += returnCost;
                StartCoroutine(FloatText(building.transform, returnCost));
                return building;
            }
        }
        return null;
    }

    // Remove the passed building, should be called with the building returned from ReturnCost()
    public void RemoveBulding(Building building)
    {
        if (building == null)
        {
            return;
        }
        else if (building.BuildingType != BuildingType.Hub)
        {
            //PointAtObj = hit.transform.gameObject;
            building.DismantleBuilding();
        }
    }
	
	    public void checkUpgrade(BuildingType building, GameObject b)
    {
        if (building == BuildingType.AirCannon)
        {
            if (WorldController.Instance.mortarUpgradeLevel)
            {
                upgradeMaterial(WorldController.Instance.mortarUpgradeLevel.upgradeNum, b,true);
            }
        }
        else if (building == BuildingType.FogRepeller)
        {
            if (WorldController.Instance.pulseDefUpgradeLevel)
            {
                upgradeMaterial(WorldController.Instance.pulseDefUpgradeLevel.upgradeNum, b,false);
            }
        }
        else if (building == BuildingType.Harvester)
        {
            if (WorldController.Instance.hvstUpgradeLevel)
            {
                upgradeMaterial(WorldController.Instance.hvstUpgradeLevel.upgradeNum,b,false);
            }
        }
    }
    public void upgradeMaterial(int level, GameObject b,bool air)
    {
        Material mr = b.GetComponentInChildren<MeshRenderer>().material;
        Color lvl = mr.GetColor("_UpgradeColour");
        if (level == 1)
        {
            lvl = new Color(0f, 174f, 191f);
        }
        else if (level == 2)
        {
            lvl = new Color(107f, 0f, 191f);
        }
        if (air) //b.GetComponent<Building>().BuildingType == BuildingType.AirCannon
        {
            MeshRenderer[] meshs = b.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in meshs)
            {
                Material k = m.material;
                k.SetColor("_UpgradeColour", lvl);
                k.SetFloat("_FresnelPower", 10f);
                k.SetFloat("_Upgraded", 1);
            }
        }
        else
        {
            mr = b.GetComponentInChildren<MeshRenderer>().material;
            mr.SetColor("_UpgradeColour", lvl);
            mr.SetFloat("_Upgraded", 1);
            mr.SetFloat("_FresnelPower", 10f);
        }
    }

    // Builds the building given on the tile given.
    public void Build(GameObject toBuild, TileData tile, float height)
    {
        Hub hub = WorldController.Instance.Hub;
        Building building = toBuild.GetComponentInChildren<Building>();
        BuildingType buildType = toBuild.GetComponentInChildren<Building>().BuildingType;

        // Check if required resources are avaliable
        if (building != null)
        {
            int pcost = building.PowerCost;
            int mcost = building.MineralCost;

            if (pcost <= resourceController.StoredPower &&
                mcost <= resourceController.StoredMineral)
            {
                // Remove required resources
                resourceController.StoredPower -= pcost;
                resourceController.StoredMineral -= mcost;

                // Place new building
                Vector3 PosToInst = new Vector3(tile.X, height, tile.Z);
                GameObject buildingGo = Instantiate(toBuild, PosToInst, Quaternion.Euler(0f, 0f, 0f));
                building = buildingGo.GetComponentInChildren<Building>();
                buildingGo.transform.SetParent(WorldController.Instance.Ground.transform);
                tile.Building = building;
                building.Location = tile;

                // Set and play animation
                building.Animator = buildingGo.GetComponentInChildren<Animator>();
                if (building.Animator)
                {
                    building.Animator.SetBool("Built", true);
                }

                checkUpgrade(buildType,buildingGo);
                //Tell the building to do things it should do when placed
                building.Place();
                //    changeTileMaterial(WorldController.Instance.normalTile);
                changeTileMaterial(false);
                UIController.instance.buildingSelector.ToggleVisibility();
                StartCoroutine(FloatText(buildingGo.transform, -building.MineralCost));
            }
            else
            {
                //warningScript.ShowMessage(WarningScript.WarningLevel.Warning, warningScript.Warning + "Not enough minerals to build!");
                Debug.Log("Can't build, do not have the required resources.");
            }
        }
    }

    // Floating text functions -----------------------------------------------------------------

    /// <summary>
    /// Coroutine to create floating text to display costs of building
    /// </summary>
    /// <param name="building">The location of the building being built</param>
    /// <param name="cost">Cost of building or refunding building</param>
    private IEnumerator FloatText(Transform building, int cost)
    {
        if (cost < 0)
        {
            floatingTextController.CreateFloatingText($"<sprite=\"all_icons\" index=2> <color=\"red\">{cost}", building);
        }
        else if (cost > 0)
        {
            floatingTextController.CreateFloatingText($"<sprite=\"all_icons\" index=2> <color=#009900>+{cost}", building);
        }
        yield return new WaitForSeconds(0.01f);
    }

    private void OnTriggerEnter(Collider other)
    {
        collisionList.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        collisionList.Remove(other.gameObject);
    }

    // Utility Functions -----------------------------------------------------------------

    // Returns if a tile is OK to be built on.
    private bool CheckIfTileOkay(TileData tile, BuildingType building)
    {
        if (tutorialController.Stage == TutorialStage.Finished || (tile == tutorialController.TargetTile && building == tutorialController.CurrentlyBuilding))
        {
            return true;
        }

        return false;
    }

    // Disabled functions -----------------------------------------------------------------

    // Test for game pause/over mouse to not build/destroy buildings
    // public void GamePlayStop()
    // {
    //     isStopped = !isStopped;
    // }
}
