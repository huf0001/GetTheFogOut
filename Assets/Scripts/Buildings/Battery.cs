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

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //audioSource = GetComponent<AudioSource>();
    }

    public override void Place()
    {
        base.Place();
        //if (!WorldController.Instance.Hub.Batteries.Contains(this))
        //{
        //    WorldController.Instance.Hub.Batteries.Add(this);
        //}
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        //if (WorldController.Instance.Hub.Batteries.Contains(this))
        //{
        //    WorldController.Instance.Hub.Batteries.Remove(this);
        //}
    }
    
}
