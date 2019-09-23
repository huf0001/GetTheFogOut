
using System;
using UnityEngine;

public class FogLightning : MonoBehaviour
{
    private ParticleSystem lightningPS;
    public GameObject lightning;

    public ParticleSystem LightningPS
    {
        get { return lightningPS; }
        set
        {
            lightningPS = value;
            var main = lightningPS.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }
    }

    private void Start()
    {
        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void Update()
    {
        if (lightningPS.isStopped)
        {
            lightning.SetActive(false);
            Fog.Instance.LightningInPlay.Remove(this);
            Fog.Instance.LightningInPool.Add(this);
        }
    }

//    void OnParticleSystemStopped()
//    {
//        lightning.SetActive(false);
//        Fog.Instance.lightningInPlay.Remove(this);
//        Fog.Instance.lightningInPool.Add(this);
//        Debug.Log("Worked?");
//    }
}
