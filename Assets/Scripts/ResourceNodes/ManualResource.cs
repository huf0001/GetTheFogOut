using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualResource : MonoBehaviour
{
    [SerializeField] GameObject ResourcePrefab;

    private bool inResource;
    // Start is called before the first frame update
    void Start()
    {
        inResource = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (inResource)
        {
            placeResource();
        }
    }

    public void placeResource()
    {
        RenderResource();
    }

    private void RenderResource()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        TileData tile;
        if (WorldController.Instance.Ground.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
        {
            tile = WorldController.Instance.GetTileAt(hit.point);
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 PosToInst = new Vector3(tile.X, 0.25f, tile.Z);
                GameObject ToSpawn = Instantiate(ResourcePrefab, PosToInst, Quaternion.identity);
                inResource = false;
            }
        }
    }

    public void InResource()
    {
        inResource = !inResource;
    }

}
