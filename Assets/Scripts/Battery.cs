using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Building
{
    public Battery()
    {
        // call Hub.AddBattery(); adds 10 to maxStorage
        // costs 30 to build
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (!WorldController.Instance.Hub.Batteries.Contains(this))
        {
            WorldController.Instance.Hub.Batteries.Add(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (WorldController.Instance.Hub.Batteries.Contains(this))
        {
            WorldController.Instance.Hub.Batteries.Remove(this);
        }
    }
}
