using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    private List<FogUnit> units = new List<FogUnit>();
    private WorldController world;

    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.Find("GameManager").GetComponent<WorldController>();


        
    }

    private void CreateNewUnit(WorldController world, int x, int y)
    {
        FogUnit unit = new FogUnit();
        unit.Location = world.GetTileAt(x, y).GetComponent<Tile>();

        units.Add(unit);
    }

    // Update is called once per frame
    void Update()
    {
        if (units.Count == 0)
        {
            // game over, the player has won.
        }
    }


}
