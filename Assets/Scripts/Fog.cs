using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    private List<GameObject> units = new List<GameObject>();
    private WorldController world;
    [SerializeField] private GameObject fogUnitPrefab;

    private List<Tile> fogTiles = new List<Tile>();

    private int xMax = 0;
    private int zMax = 0;
    private float fogHealthLimit = 5f;
    private float tick = 0;

    // Start is called before the first frame update
    public void InstantiateFog()
    {
        world = GameObject.Find("GameManager").GetComponent<WorldController>();
        xMax = world.Length;
        zMax = world.Width;

        for (int i = 0; i < zMax; i++)
        {
            CreateNewUnit(i, zMax - 1);
        }
    }

    private void CreateNewUnit(int x, int z)
    {
        GameObject unit = Instantiate(fogUnitPrefab);
        Tile tile = world.GetTileAt(x, z).GetComponent<Tile>();
        unit.transform.position = new Vector3(x, tile.transform.position.y + 0.3f, z);
        unit.transform.SetParent(this.transform, true);
        unit.name = "FogUnit_" + x + "_" + z;
        unit.GetComponent<FogUnit>().Location = tile;
        unit.GetComponent<FogUnit>().HealthLimit = fogHealthLimit;
        units.Add(unit);
        fogTiles.Add(tile);
    }

    // Update is called once per frame
    void Update()
    {
        if (units.Count == 0)
        {
            InstantiateFog();
        }

        tick += Time.deltaTime;

        foreach (GameObject f in units)
        {
            f.GetComponent<FogUnit>().Health += Time.deltaTime;
        }

        if (tick >= 1)
        {
            //Debug.Log("Fog: 'Tick!'");
            tick = 0;
            SpillTilesOver();
        }
    }

    private void SpillTilesOver()
    {
        FogUnit f = null;
        Tile t = null;
        List<Tile> newTiles = new List<Tile>();

        //Debug.Log("units length is " + units.Count);

        foreach (GameObject g in units)
        {
            f = g.GetComponent<FogUnit>();
            t = f.Location;

            //Debug.Log("f.Health: " + f.Health + ". fogHealthLimit: " + fogHealthLimit + ".");

            if (f.Health == fogHealthLimit)
            {
                for (int i = t.X - 1; i <= t.X + 1; i++)
                {
                    if (i >= 0 && i < xMax)
                    {
                        for (int j = t.Z - 1; j <= t.Z + 1; j++)
                        {
                            if (j >= 0 && j < zMax)
                            {
                                t = world.GetTileAt(i, j).GetComponent<Tile>();

                                if (fogTiles.Contains(t) == false)
                                {
                                    newTiles.Add(t);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (newTiles.Count > 0)
        {
            foreach (Tile newTile in newTiles)
            {
                CreateNewUnit(newTile.X, newTile.Z);        //CreateNewUnit adds the tile spawned on to the list fogTiles
            }
        }
    }
}
