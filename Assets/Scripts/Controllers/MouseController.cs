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
    // Test for game pause/over mouse to not build/destroy buildings
    // private bool isStopped = false;

    // Public Properties
    public static MouseController Instance { get; protected set; }
    public Hub Hub { get => hub; set => hub = value; }
    public bool ReportTutorialClick { get => reportTutorialClick; set => reportTutorialClick = value; }

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
        towerManager = FindObjectOfType<TowerManager>();
        floatingTextController = GetComponent<FloatingTextController>();
        tutorialController = GetComponent<TutorialController>();
        cam = Camera.main;
        kb = InputSystem.GetDevice<Keyboard>();
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
                Building removeBuilding = ReturnCost(towerManager.CurrentTile);
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
                    if (hovertoggle)
                    {
                        hovertoggle = false;
                        changeTileMaterial(WorldController.Instance.normalTile);

                        if (!t.buildingChecks.collectable)
                        {
                            hoveredTile = t;
                            changeTileMaterial(WorldController.Instance.hoverTile);
                        }
                        else
                            changeTileMaterial(WorldController.Instance.normalTile);

                    }
                    else
                    {
                        if (!(hoveredTile.Equals(t)))
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
                                changeTileMaterial(WorldController.Instance.normalTile);
                            }

                        }
                    }
                }
            }
            else
            {
                changeTileMaterial(WorldController.Instance.normalTile);
            }
        }
        else
        {
            if (towerManager.CurrentTile != null)
            {
                if (towerManager.CurrentTile.plane)
                {
                    changeTileMaterial(WorldController.Instance.normalTile);

                    if ((!towerManager.CurrentTile.plane.GetComponent<Renderer>().material.Equals(WorldController.Instance.hoverTile)))
                    {
                        Color newColor = towerManager.CurrentTile.plane.GetComponent<Renderer>().material.GetColor("_BaseColor");
                        newColor.a = 0.8f;
                        towerManager.CurrentTile.plane.GetComponent<Renderer>().material.SetColor("_BaseColor", newColor);
                    }
                    hoveredTile = towerManager.CurrentTile;
                }
            }
            else
            {
                towerManager.CurrentTile = hoveredTile;
            }
        }
    }

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

    void selectedTile()
    {
        if (UIController.instance.buildingSelector.Visible || UIController.instance.buildingInfo.Visible)
        {
            UpdateHoveredTile(false);
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
            TileData tile;
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            /*
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Buildings")))
            {
                if (!UIController.instance.buildingInfo.Visible)
                {
                    Building b;
                    if (hit.collider.name == "pCube31")
                    {
                        b = Hub.Instance;
                        UIController.instance.buildingInfo.ShowInfo(b);
                    }
                }
                else
                    UIController.instance.buildingInfo.HideInfo();

            }else
            */
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(new string[] { "Tiles", "Collectables", "Buildings" })) && WorldController.Instance.TileExistsAt(hit.point))
            {
                if (UIController.instance.buildingSelector.Visible || UIController.instance.buildingInfo.Visible)
                {
                    changeTileMaterial(WorldController.Instance.normalTile);
                    towerManager.CancelBuild();
                } else if (hit.collider.gameObject.layer == 15)
                {
                    hit.collider.GetComponent<Collectable>()?.CollectAbility();
                    hit.collider.GetComponent<ShipComponent>()?.Collect();
                }
                else if (hit.collider.name == "pCube31")
                {
                    if (!UIController.instance.buildingInfo.Visible)
                    {
                        Building b = Hub.Instance;
                        UIController.instance.buildingInfo.ShowInfo(b);
                    }
                    else
                        UIController.instance.buildingInfo.HideInfo();

                    TileData t = WorldController.Instance.GetTileAt(36,34);
                    towerManager.CurrentTile = t;
                }
                else
                {
                    tile = WorldController.Instance.GetTileAt(hit.point);
                    if (tile.isBuildable)
                    {
                        if (WorldController.Instance.ActiveTiles.Contains((tile)) && TutorialController.Instance.TileAllowed(tile))
                        {
                            if (!UIController.instance.buildingSelector.Visible)
                            {
                                WorldController.Instance.CheckTileContents(tile);
                            }

                            if (reportTutorialClick)
                            {
                                TutorialController.Instance.RegisterMouseClicked();
                            }

                            towerManager.CurrentTile = tile;
                        }
                    } else
                        towerManager.CurrentTile = tile;
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
                            Debug.Log(building.Health + " NNNNNN" + returnCost);
                        }

                        if (building.Health < 20 && building.Health > 5)
                        {
                            returnCost = Mathf.RoundToInt(returnCost / 2f);
                            Debug.Log(building.Health + " YYY" + returnCost);
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

                //Tell the building to do things it should do when placed
                building.Place();
                changeTileMaterial(WorldController.Instance.normalTile);
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
        if (tutorialController.Stage == TutorialStage.Finished || (tile == tutorialController.CurrentTile && building == tutorialController.CurrentlyBuilding))
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
