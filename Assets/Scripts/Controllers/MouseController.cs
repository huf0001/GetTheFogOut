using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    //Serialized fields
    [SerializeField] private int generatorCount = 0;
    [SerializeField] private int generatorInterval = 5;

    //Non-serialized fields
    private Hub hub = null;
    private ResourceController resourceController = null;
    TowerManager towerManager;
    FloatingTextController floatingTextController;

    private TutorialController tutorialController;

    private GameObject PointAtObj;
    List<GameObject> collisionList = new List<GameObject>();

    public Hub Hub { get => hub; set => hub = value; }

    // Test for game pause/over mouse to not build/destroy buildings
    // private bool isStopped = false;

    // Start is called before the first frame update
    void Start()
    {
        resourceController = WorldController.Instance.ResourceController;
        hub = WorldController.Instance.Hub;
        towerManager = FindObjectOfType<TowerManager>();
        floatingTextController = GetComponent<FloatingTextController>();
        tutorialController = GetComponent<TutorialController>();
    }

    // Test for game pause/over mouse to not build/destroy buildings
    // public void GamePlayStop()
    // {
    //     isStopped = !isStopped;
    // }

    // Update is called once per frame
    void Update()
    {
        if (towerManager.IsinBuild())
        {
            UpdatePlacing();
        }

        // if (!isStopped)
        // {
        //     UpdatePlacing();
        // }
    }

    void RemoveBuilding()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Xbox_A") )
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.tag == "Building")
                {
                    if (hit.transform.gameObject.GetComponent<Building>().BuildingType != BuildingType.Hub)
                    {
                        PointAtObj = hit.transform.gameObject;
                        hit.transform.gameObject.GetComponent<Building>().DismantleBuilding();
                    }
                }
            }
        }
    }

    void UpdatePlacing()
    {
        // code based somewhat off:
        //"https://forum.unity.com/threads/click-object-behind-other-object.480815/"

        if (Time.timeScale == 1.0f && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Xbox_A")) )
        {
            TileData tile;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (WorldController.Instance.Ground.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                //Check if a valid tile was clicked
                if (WorldController.Instance.TileExistsAt(hit.point))
                {
                    tile = WorldController.Instance.GetTileAt(hit.point);

                    if (CheckIfTileOkay(tile, towerManager.GetBuildingType()))
                    {
                        //If tile has power, place building. Otherwise, don't place building.
                        if (tile.PowerSource != null)
                        {
                            GameObject toBuild = towerManager.GetTower("build");

                            // If there is a building, delete it if deletion is selected. Otherwise, place one if the tile is empty.
                            if (toBuild.name != "Empty")
                            {
                                //Debug.Log(toBuild.name);
                                if (toBuild.GetComponent<Building>() != null)
                                {
                                    Building b = toBuild.GetComponent<Building>();
                                }
                                else
                                {
                                    Debug.Log("toBuild's building component is non-existant");
                                }

                                if (tile.Building == null && (tile.Resource == null || towerManager.GetBuildingType() == BuildingType.Harvester))
                                {

                                    if (towerManager.GetBuildingType() == BuildingType.Generator)
                                    {
                                        if (FindObjectsOfType<Generator>().Length >= (WorldController.Instance.GetRecoveredComponentCount() + 1) * generatorInterval + 1) // the +1 accounts for the fact that the generator hologram, which has the buildingtype generator, will be on the board with the actual generators
                                        {
                                            Debug.Log("If you want to build more generators, collect more ship components first.");
                                            return;
                                        }
                                    }

                                    tile.Placedtower = toBuild;
                                    Build(tile.Placedtower, tile, 0f);
                                }
                            }
                            else
                            {
                                RemoveBuilding();

                                if(PointAtObj != null)
                                {
                                    ReturnCost(PointAtObj);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void ReturnCost(GameObject buildtodestroy)
    {
        //Hub hub = WorldController.Instance.Hub;
        Building building = buildtodestroy.GetComponentInChildren<Building>();

        // add required resources
        resourceController.StoredPower += building.PowerCost;
        resourceController.StoredMineral += building.MineralCost;
        resourceController.StoredOrganic += building.OrganicCost;
        resourceController.StoredFuel += building.FuelCost;

        StartCoroutine(FloatText(building.transform, building.MineralCost));
    }

    private bool CheckIfTileOkay(TileData tile, BuildingType building)
    {
        if (tutorialController.TutorialStage == TutorialStage.Finished || (tile == tutorialController.CurrentTile && building == tutorialController.CurrentlyBuilding))
        {
            return true;
        }

        return false;
    }

    private void Build(GameObject toBuild, TileData tile, float height)
    {
        Hub hub = WorldController.Instance.Hub;
        Building building = toBuild.GetComponentInChildren<Building>();
        BuildingType buildType = toBuild.GetComponentInChildren<Building>().BuildingType;

        // Check if required resources are avaliable
        if (building != null)
        {
            int pcost = building.PowerCost;
            int mcost = building.MineralCost;
            int ocost = building.OrganicCost;
            int fcost = building.FuelCost;

            if (pcost <= resourceController.StoredPower &&
                mcost <= resourceController.StoredMineral &&
                ocost <= resourceController.StoredOrganic &&
                fcost <= resourceController.StoredFuel)
            {
                // Remove required resources
                resourceController.StoredPower -= pcost;
                resourceController.StoredMineral -= mcost;
                resourceController.StoredOrganic -= ocost;
                resourceController.StoredFuel -= fcost;

                // Place new building
                Vector3 PosToInst = new Vector3(tile.X, height, tile.Z);
                Debug.Log(tile.X + " " + tile.Z);
                GameObject buildingGo = Instantiate(toBuild, PosToInst, Quaternion.Euler(0f, 0f, 0f));
                buildingGo.transform.SetParent(WorldController.Instance.Ground.transform);
                tile.Building = building;
                building.Location = tile;

                if (resourceController == null)
                {
                    Debug.Log("resourceController is null in MouseController");
                }
                else
                {
                    building.ResourceController = resourceController;
                }

                building.Animator = buildingGo.GetComponentInChildren<Animator>();
                building.Animator.SetBool("Built", true);
                building.Place();
                StartCoroutine(FloatText(buildingGo.transform, -building.MineralCost));
            }
            else
            {
                Debug.Log("Can't build, do not have the required resources.");
            }
        }
    }

    /// <summary>
    /// Coroutine to create floating text to display costs of building
    /// </summary>
    /// <param name="building">The location of the building being built</param>
    /// <param name="cost">Cost of building or refunding building</param>
    private IEnumerator FloatText(Transform building, int cost)
    {
        //floatingTextController.CreateFloatingText($"<sprite=\"all_icons\" index=0> -{hub.BuildingsCosts[buildType]["power"]}", buildingGo.transform);
        if (cost < 0)
        {
            floatingTextController.CreateFloatingText($"<sprite=\"all_icons\" index=3> <color=\"red\">{cost}", building);
        }
        else if (cost > 0)
        {
            floatingTextController.CreateFloatingText($"<sprite=\"all_icons\" index=3> <color=#009900>+{cost}", building);
        }
        yield return new WaitForSeconds(0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        collisionList.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        collisionList.Remove(other.gameObject);
    }

}
