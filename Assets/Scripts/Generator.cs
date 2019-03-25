using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Building
{
    public Generator()
    {
        upkeep = +2;
        // costs 30 to build
    }

    // Start is called before the first frame update
    void Start()
    {
        // costs 30 to build
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Upkeep()
    {
        // call Hub.ChangePower(upkeep)
    }
}
