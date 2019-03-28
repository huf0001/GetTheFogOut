using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerSource : Building
{
    [SerializeField] protected float range;

    private List<Building> suppliedBuildings = new List<Building>();
    
    void Awake()
    {
        ActivateTiles();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ActivateTiles()
    {
        Collider[] tilesToActivate = Physics.OverlapSphere(transform.position, range);

        foreach (Collider c in tilesToActivate)
        {
            if (c.gameObject.GetComponent<Tile>() != null)
            {
                c.gameObject.GetComponent<Tile>().PowerUp(this);
            }
        }
    }

    public float Range
    {
        get
        {
            return range;
        }
    }

    public void PlugIn(Building newBuilding)
    {
        suppliedBuildings.Add(newBuilding);
    }

    public void Unplug(Building unplug)
    {
        if (suppliedBuildings.Contains(unplug))
        {
            suppliedBuildings.Remove(unplug);
        }
    }

    public abstract bool SupplyingPower();
}
