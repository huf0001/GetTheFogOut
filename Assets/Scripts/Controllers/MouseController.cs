using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    List<GameObject> collisionList = new List<GameObject>();
    TowerManager towerManager;

    // Start is called before the first frame update
    void Start()
    {
        towerManager = FindObjectOfType<TowerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacing();
    }

    void UpdatePlacing()
    {
        // code based somewhat off:
        //"https://forum.unity.com/threads/click-object-behind-other-object.480815/"

        if (Input.GetMouseButtonDown(0))
        {
            //RaycastHit[] hits;
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //hits = Physics.RaycastAll(ray, 100.0f);
            //for (int i = 0; i < hits.Length; i++)
            //{
            //    RaycastHit hit = hits[i];
            //    if (hit.transform.gameObject.tag == "Tile")
            //    {
            //        Debug.Log("hit");
            //     //   tiletest = hit.transform.gameObject;
            //     //   WC.MeshRendererTile(tiletest,true);

            //        if (!EventSystem.current.IsPointerOverGameObject())
            //        {
            //            //Tile tile = hit.transform.gameObject.GetComponent<Tile>();
            TileData tile;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (WorldController.Instance.Ground.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                tile = WorldController.Instance.GetTileAt(hit.point);

                //If tile has power, place building. Otherwise, don't place building.
                if (tile.PowerSource != null)
                {
                    tile.Placedtower = towerManager.GetTower();
                    // If there is a building, delete it. If not, place one.
                    if ((tile.Building != null) && (tile.Building.gameObject.GetComponent<Hub>() == null))
                    {
                        Destroy(tile.Building.transform.gameObject);
                    }
                    else
                    {
                        Build(tile.Placedtower, tile, 0f);
 
                        //tm.SelectedTower = null;      //If selected tower is reverted to null after the building is created, this will create user problems atm as they won't know that they can't just click
                        //another space and make another building of the same type there.
                    }
                }   
                return;
            }
        }
    }
    //}
        //}

    //}


    private void Build(GameObject toBuild, TileData tile, float height)
    {
        Hub hub = WorldController.Instance.Hub;
        BuildingType buildType = toBuild.GetComponentInChildren<Building>().BuildingType;

        // Check if required resources are avaliable
        if (hub.BuildingsCosts[buildType]["power"] <= hub.StoredPower &&
            hub.BuildingsCosts[buildType]["mineral"] <= hub.StoredMineral &&
            hub.BuildingsCosts[buildType]["organic"] <= hub.StoredOrganic &&
            hub.BuildingsCosts[buildType]["fuel"] <= hub.StoredFuel)
        {
            // Remove required resources
            hub.StoredPower -= hub.BuildingsCosts[buildType]["power"];
            hub.StoredMineral -= hub.BuildingsCosts[buildType]["mineral"];
            hub.StoredOrganic -= hub.BuildingsCosts[buildType]["organic"];
            hub.StoredFuel -= hub.BuildingsCosts[buildType]["fuel"];

            // Place new building
            Vector3 PosToInst = new Vector3(tile.X, height, tile.Z);
            Debug.Log(tile.X + " " + tile.Z);
            GameObject buildingGo = Instantiate(toBuild, PosToInst, Quaternion.Euler(0f, 0f, 0f));
            buildingGo.transform.SetParent(WorldController.Instance.Ground.transform);
            Building building = buildingGo.GetComponentInChildren<Building>();
            tile.Building = building;
            building.Location = tile;
            building.Animator = buildingGo.GetComponentInChildren<Animator>();
            building.Animator.SetBool("Built", true);
            building.Place();
        }
        else
        {
            Debug.Log("Can't build, do not have the required resources.");
        }

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
