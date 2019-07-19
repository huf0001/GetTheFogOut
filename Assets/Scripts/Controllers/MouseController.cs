using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    // Fields -------------------------------------------------------------------

    // Serialized fields
    [SerializeField] private int generatorCount = 0;
    [SerializeField] private int generatorInterval = 5;
    [SerializeField] private WarningScript warningScript;

    // Non-serialized fields
    private Hub hub = null;
    private ResourceController resourceController = null;
    private TowerManager towerManager;
    private FloatingTextController floatingTextController;
    private TutorialController tutorialController;
    private GameObject PointAtObj;
    List<GameObject> collisionList = new List<GameObject>();
    private bool reportTutorialClick = false;
    private TileData hoveredTile;
    public bool isBuildAvaliable = true;
    private bool hovertoggle = true;
    // Test for game pause/over mouse to not build/destroy buildings
    // private bool isStopped = false;

    // Public Properties
    public static MouseController Instance { get; protected set; }
    public Hub Hub { get => hub; set => hub = value; }
    public bool ReportTutorialClick { get => reportTutorialClick; set => reportTutorialClick = value; }
    public WarningScript WarningScript { get => warningScript; }

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
        resourceController = ResourceController.Instance;
        hub = WorldController.Instance.Hub;
        towerManager = FindObjectOfType<TowerManager>();
        floatingTextController = GetComponent<FloatingTextController>();
        tutorialController = GetComponent<TutorialController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacingAlt();
        UpdateHoveredTile();
    }

    void UpdateTileAppearance()
    {
        List<TileData> innerTiles = new List<TileData>();
        List<TileData> middleTiles = new List<TileData>();
        List<TileData> outerTiles = new List<TileData>();

        Vector3 pos = new Vector3(hoveredTile.X, 0f, hoveredTile.Z);

        Collider[] hits = Physics.OverlapSphere(pos, 3, LayerMask.GetMask("Quads"));
        foreach (Collider hit in hits)
        {
            innerTiles.Add(WorldController.Instance.GetTileAt(hit.transform.position));
        }

        hits = Physics.OverlapSphere(pos, 4, LayerMask.GetMask("Quads"));
        foreach (Collider hit in hits)
        {
            if (!innerTiles.Contains(WorldController.Instance.GetTileAt(hit.transform.position)))
            {
                middleTiles.Add(WorldController.Instance.GetTileAt(hit.transform.position));
            }
        }

        hits = Physics.OverlapSphere(pos, 5, LayerMask.GetMask("Quads"));
        foreach (Collider hit in hits)
        {
            if (!middleTiles.Contains(WorldController.Instance.GetTileAt(hit.transform.position)))
            {
                outerTiles.Add(WorldController.Instance.GetTileAt(hit.transform.position));
            }
        }
    }

    void UpdateHoveredTile()
    {
        RaycastHit hit;
        TileData temp;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("GridPlanes")))
        {
            if (WorldController.Instance.TileExistsAt(hit.point))
            {
                if (hovertoggle)
                {
                    hoveredTile = WorldController.Instance.GetTileAt(hit.point);
                    hoveredTile.plane.GetComponent<Renderer>().material = WorldController.Instance.hoverTile;
                    hovertoggle = false;
                }
                else
                {
                    if (!hoveredTile.Equals(WorldController.Instance.GetTileAt(hit.point)))
                    {
                        hoveredTile.plane.GetComponent<Renderer>().material = WorldController.Instance.normalTile;
                        hovertoggle = true;
                    }
                }
            }
        }
        else
        {
            if (hoveredTile != null)
            {
                hoveredTile.plane.GetComponent<Renderer>().material = WorldController.Instance.normalTile;
            }
        }

    }

    // Mouse Building Placement -----------------------------------------------------------------

    void UpdatePlacingAlt()
    {
        // code based somewhat off:
        //"https://forum.unity.com/threads/click-object-behind-other-object.480815/"

        UIController.instance.buildingSelector.freezeCam();
        
        if (Time.timeScale == 1.0f && Input.GetButtonDown("Submit") && !EventSystem.current.IsPointerOverGameObject() && isBuildAvaliable == true)
        {
            TileData tile;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Check if a valid tile was clicked
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Tiles")) && WorldController.Instance.TileExistsAt(hit.point))
            {
                if (UIController.instance.buildingSelector.Visible || UIController.instance.buildingInfo.Visible)
                {
                    towerManager.CancelBuild();
                }
                else
                {
                    tile = WorldController.Instance.GetTileAt(hit.point);

                    if (tile.PowerSource != null && TutorialController.Instance.TileAllowed(tile))
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
                }
            }
        }
    }

    // Refunds the cost for a building that is going to be destroyed. 
    // Returns the building that should be destroyed.
    public Building ReturnCost(TileData tile)
    {
        if (tile.Building != null)
        {
            Building building = tile.Building;
            if (building.BuildingType != BuildingType.Hub)
            {
                // add required resources
                resourceController.StoredPower += building.PowerCost;
                resourceController.StoredMineral += building.MineralCost;

                StartCoroutine(FloatText(building.transform, building.MineralCost));
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
                //Debug.Log(tile.X + " " + tile.Z);
                GameObject buildingGo = Instantiate(toBuild, PosToInst, Quaternion.Euler(0f, 0f, 0f));
                building = buildingGo.GetComponentInChildren<Building>();
                buildingGo.transform.SetParent(WorldController.Instance.Ground.transform);
                tile.Building = building;
                building.Location = tile;

                // Give the building a copy of RecourceController   //ResourceController is now a public static class; Building calls it directly to get a reference to it.
                //if (resourceController == null)
                //{
                //    Debug.Log("resourceController is null in MouseController");
                //}
                //else
                //{
                //    building.ResourceController = resourceController;
                //}

                // Set and play animation
                building.Animator = buildingGo.GetComponentInChildren<Animator>();
                if (building.Animator)
                {
                    building.Animator.SetBool("Built", true);
                }

                //Tell the building to do things it should do when placed
                building.Place();

                StartCoroutine(FloatText(buildingGo.transform, -building.MineralCost));
            }
            else
            {
                warningScript.ShowMessage(WarningScript.WarningLevel.Warning, warningScript.Warning + "Not enough minerals to build!");
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
        if (tutorialController.TutorialStage == TutorialStage.Finished || (tile == tutorialController.CurrentTile && building == tutorialController.CurrentlyBuilding))
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
