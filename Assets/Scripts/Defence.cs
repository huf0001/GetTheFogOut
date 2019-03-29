using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defence : Building
{
    public Defence(PowerSource power)
    {
        powerSource = power;
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
}
