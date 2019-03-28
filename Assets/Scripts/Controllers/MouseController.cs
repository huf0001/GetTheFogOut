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
                        TowerManager tm = FindObjectOfType<TowerManager>();
                        tile.Placedtower = tm.GetTower();

                        // If there is a building, delete it. If not, place one.
                        if (tile.Building != null)
                        {
                            Destroy(tile.Building.transform.gameObject);
                        }
                        else
                        {

                                Vector3 PosToInst = new Vector3(tile.transform.position.x, hit.point.y, tile.transform.position.z);
                                GameObject buildingGo = Instantiate(tile.Placedtower,
    PosToInst, tile.transform.rotation);
                                    buildingGo.transform.SetParent(tile.transform);
                                Building building = buildingGo.AddComponent<Harvester>() as Building;
                                tile.Building = building;
                            
                            tm.SelectedTower = null;
                        }

                        return;
                    } 
                }
            }
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
