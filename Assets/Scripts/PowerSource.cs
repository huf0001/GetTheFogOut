using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerSource : Building
{
    [SerializeField] protected float range;

    [SerializeField] protected List<Building> suppliedBuildings = new List<Building>();
    
    void Awake()
    {
        ActivateTiles();
    }

    private void OnDestroy()
    {
        DeactivateTiles();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
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

    private void DeactivateTiles()
    {
        Collider[] tilesToDeactivate = Physics.OverlapSphere(transform.position, range);

        foreach (Collider c in tilesToDeactivate)
        {
            if (c.gameObject.GetComponent<Tile>() != null)
            {
                c.gameObject.GetComponent<Tile>().PowerDown(this);
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
