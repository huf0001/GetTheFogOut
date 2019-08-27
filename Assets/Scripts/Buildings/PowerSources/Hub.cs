using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hub : PowerSource
{ 
    public static Hub Instance { get; protected set; }
    [SerializeField] private GameObject Fires;
    [SerializeField] private Light fireLight;
    private float tick = 0;
    private bool stillFire = true;
    protected void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There should only be one hub in the scene, right?");
        }

        Instance = this;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        powerSource = null;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override bool SupplyingPower()
    {
        return true;
    }

    public bool isDestroyed()
    {
        if (Health <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void extinguishingFire()
    {
        if (stillFire)
        {
            ParticleSystem[] ps = Fires.GetComponentsInChildren<ParticleSystem>();
            // delay 6s - 100 particle so 60s (reparing time) = 1000 particles but because i roundtoint so it faster so 7s
            if (delay(7))
            {
                foreach (ParticleSystem p in ps)
                {
                    ParticleSystem.MainModule pe = p.main;
                    if (pe.maxParticles > 0)
                    {
                        
                        pe.maxParticles -= 111  ;
                    }
                    if (pe.maxParticles <= 1)  // particle left
                    {
                            fireLight.enabled = false;
                            stillFire = false;
                            Fires.SetActive(false);
                    }
                    tick = 0;
                }
            }
        }
    }

    private bool delay(int wait)
    {
        tick += Time.deltaTime;
        
        if (Mathf.RoundToInt(tick) == wait)
        {
            return true;
        }
        else
            return false;
    }

}
