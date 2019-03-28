using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBroken
{

    [SerializeField] private Resource resource;
    [SerializeField] private Building building = null;

    public Resource Resource
    {
        get => resource;
        set
        {
            if (cbTileResouceChanged != null) cbTileResouceChanged(this);
        }
    }
    public Building Building
    {
        get => building;
        set
        {
            if (cbTileBuildingChanged != null) cbTileBuildingChanged(this, value);
        }
    }

    Action<TileBroken, Building> cbTileBuildingChanged;
    Action<TileBroken> cbTileResouceChanged;

    private int x;
    public int X { get => x; }
    private int y;
    public int Y { get => y; }
    private int z;
    public int Z { get => z; }

    public TileBroken(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void RegisterTileBuildingChangedCallback(Action<TileBroken, Building> callback)
    {
        cbTileBuildingChanged += callback;
    }

    public void UnregisterTileBuildingChangedCallback(Action<TileBroken, Building> callback)
    {
        cbTileBuildingChanged -= callback;
    }

    public void RegisterTileResourceChangedCallback(Action<TileBroken> callback)
    {
        cbTileResouceChanged += callback;
    }

    public void UnregisterTileResourceChangedCallback(Action<TileBroken> callback)
    {
        cbTileResouceChanged -= callback;
    }
}
