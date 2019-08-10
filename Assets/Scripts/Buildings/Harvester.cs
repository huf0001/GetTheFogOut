using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvester : Building
{
    [SerializeField] private int harvestAmt = +5;
    [SerializeField] private Canvas noMineralCanvas;
    [SerializeField] private Light offLight;
    [SerializeField] public ParticleSystem hvtProgress;

    public int HarvestAmt { get => harvestAmt; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!location.Resource)
        {
            noMineralCanvas.transform.LookAt(cam.transform);
        }
    }

    public void TurnOnMineralIndicator()
    {
        noMineralCanvas.gameObject.SetActive(true);
        
        offLight.GetComponent<Light>().enabled = true; //turn the red light on 
        hvtProgress.Stop(); // stop the particle
    }
    
    public override void PowerUp()
    {
        //    //if (location.Resource.ResourceType == Resource.Mineral)
        //    //{ 
        base.PowerUp();
        if (hvtProgress.isStopped)
        {
            hvtProgress.Play();
        }

    //    //if (!WorldController.Instance.Hub.Harvesters.Contains(this))
    //    //{
    //    //    WorldController.Instance.Hub.Harvesters.Add(this);
    //    //}
    }
    
    public override void PowerDown()
    {
        if (hvtProgress.isPlaying)
        {
            hvtProgress.Stop();
        }
            base.PowerDown();

        //    //if (WorldController.Instance.Hub.Harvesters.Contains(this))
        //    //{
        //    //    WorldController.Instance.Hub.Harvesters.Remove(this);
        //    //}
    }
}
