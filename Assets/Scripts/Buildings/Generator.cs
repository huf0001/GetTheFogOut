using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Building
{
    public Generator()
    {
        // costs 30 to build
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        // costs 30 to build
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected void Upkeep()
    {
        // call Hub.ChangePower(upkeep)
    }
}
