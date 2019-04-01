using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{

    List<GameObject> collisionList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
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
            RaycastHit[] hits;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray, 100.0f);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.transform.gameObject.tag == "Tile")
                {
                    Debug.Log("hit");

                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        Tile tile = hit.transform.gameObject.GetComponent<Tile>();

                        //If tile has power, place building. Otherwise, don't place building.
                        if (tile.PowerSource != null)
                        {
                            //TowerManager tm = FindObjectOfType<TowerManager>();
                            //tile.Placedtower = tm.GetTower();
                            tile.Placedtower = FindObjectOfType<TowerManager>().GetTower();

                            // If there is a building, delete it. If not, place one.
                            if ((tile.Building != null) && (tile.Building.gameObject.GetComponent<Hub>() == null))
                            {
                                Destroy(tile.Building.transform.gameObject);
                            }
                            else
                            {
                                Build(tile.Placedtower, tile, hit.point.y);
                                //tm.SelectedTower = null;      //If selected tower is reverted to null after the building is created, this will create user problems atm as they won't know that they can't just click
                                                                //another space and make another building of the same type there.
                            }

                            return;
                        }
                    } 
                }
            }
        }
    }

    private void Build(GameObject toBuild, Tile tile, float height)
    {
        Vector3 PosToInst = new Vector3(tile.transform.position.x, height, tile.transform.position.z);
        GameObject buildingGo = Instantiate(toBuild, PosToInst, tile.transform.rotation);
        buildingGo.transform.SetParent(tile.transform);
        Building building = buildingGo.GetComponentInChildren<Building>();
        tile.Building = building;
        building.Location = tile;
        building.Animator = buildingGo.GetComponentInChildren<Animator>();
        building.Animator.SetBool("Built", true);
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
