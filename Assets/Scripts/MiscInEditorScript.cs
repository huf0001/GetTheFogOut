using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MiscInEditorScript : MonoBehaviour
{
    private Building b = null;

    // Update is called once per frame
    void Update()
    {
        //ChildBuildingToTile();
        //RenameTile();
    }
    private void RenameTile()
    {
        gameObject.name = "Tile(" + transform.position.x + "," + transform.position.z + ")";
    }

    private void ChildBuildingToTile()
    {
        if (gameObject.GetComponent<Building>() != null)
        {
            b = gameObject.GetComponent<Building>();
        }

        if (b != null)
        {
            if (b.Location != null)
            {
                transform.SetParent(b.Location.transform);
            }
        }
    }
}
