using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//TODO: replace a TILE object with an invinsible box and only show upon click on the button(tower)
//TODO: add Null ref / exception :(
public class Tile : MonoBehaviour
{
    private GameObject placedtower;

    void OnMouseUp()
    {
            TowerManager tm = FindObjectOfType<TowerManager>();
            placedtower = tm.GetTower();
        //TODO: check the condition if the player has enough currency to build on this tile
            if (!EventSystem.current.IsPointerOverGameObject() && placedtower != null)
            {
                placebuilding();
        //DESC: reset the button, so you can't spam/accidentally building
                tm.SelectedTower = null;
            }
    }

    void placebuilding()
    { 
        //TODO: try to fix the position if replaced by 3d game object
        //DESC: will replace a prefab on top of the tile(parent) position/rotation.
        Instantiate(placedtower, transform.parent.position, transform.parent.rotation);

        //DESC: destroy tile upon replacing new object
        Destroy(transform.parent.gameObject);

    }
}
