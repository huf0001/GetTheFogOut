using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterFan : Defence
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        InvokeRepeating("Fire", 0.5f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CancelInvoke();
    }

    void Fire()
    {
        Tile target = GetTarget();
        if (target != null)
        {
            target.FogUnit.Health -= 30;
            foreach (Tile tile in target.AdjacentTiles)
            {
                if (tile.FogUnit != null)
                {
                tile.FogUnit.Health -= 10;
                }
            }
        }
    }

    Tile GetTarget()
    {
        // Get the tile with the highest fog concentration.
        List<Tile> tiles = new List<Tile>();
        location.CollectFogTilesInRange(tiles, (int)visibilityRange);
        tiles.Sort((t1, t2) => t1.FogUnit.Health.CompareTo(t2.FogUnit.Health));
        foreach (Tile tile in tiles) tile.Visited = false;
        if (tiles != null)
        {
            Tile target = tiles[0];
            return target;
        } else
        {
            return null;
        }
    }
}
