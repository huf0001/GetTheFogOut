using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : Entity
{
    protected int upkeep;
    protected PowerSource powerSource;

    void Awake()
    {
        Collider[] tiles = Physics.OverlapSphere(this.transform.position, 0.25f);

        if (tiles[0].gameObject.GetComponent<Tile>().PowerSource != null)
        {
            powerSource = tiles[0].gameObject.GetComponent<Tile>().PowerSource;
            powerSource.PlugIn(this);
        }
        else
        {
            Debug.Log("There is no power source supplying this space.");
            //Destroy(this.gameObject);
            //Self destruct code when no power source available has been commented out for now to make testing easier
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (powerSource != null)
        {
            powerSource.Unplug(this);
        }
    }
}
