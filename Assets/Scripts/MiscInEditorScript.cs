using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MiscInEditorScript : MonoBehaviour
{
    [SerializeField] private bool childBuildingToTile = false;
    [SerializeField] private bool renameTile = false;

    private Building b = null;

    // Update is called once per frame
    void Update()
    {
        if (childBuildingToTile)
        {
            ChildBuildingToTile();
        }

        if (renameTile)
        {
            RenameTile();
        }
    }
    private void RenameTile()
    {
        gameObject.name = "Tile(" + transform.position.x + "," + transform.position.z + ")";
    }

    private void ChildBuildingToTile()
    {
        if (gameObject.GetComponentInChildren<Building>() != null)
        {
            b = gameObject.GetComponentInChildren<Building>();
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
