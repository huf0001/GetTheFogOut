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
    private bool isPlayingSiren;

    public GameObject BrokenShip;
    public GameObject RepairedShip;
    public GameObject AttachedWing;

    private bool dismantling = false;

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
        if (isDestroyed())
        {
            WorldController.Instance.HubDestroyed = true;
        }
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
            if (delay(6))
            {
                foreach (ParticleSystem p in ps)
                {
                    ParticleSystem.MainModule pe = p.main;
                    if (pe.maxParticles > 0)
                    {
                        
                        pe.maxParticles -= 95  ;
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

    public GameObject getActiveShip()
    {
        GameObject act;
        if (BrokenShip.activeInHierarchy)
        {
            act = BrokenShip;
        }
        else if (RepairedShip.activeInHierarchy)
        {
            act = RepairedShip;
        }
        else        {
            act = AttachedWing;
        }

        return act;
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

    public void PlaySiren()
    {
        if (!isPlayingSiren)
        {
            isPlayingSiren = true;
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/3D-BuildingSirenHub", transform.position);
            Invoke(nameof(StopSirenNoise), 1f);
        }
    }

    private void StopSirenNoise()
    {
        isPlayingSiren = false;
    }

    protected override void CheckDismantle()
    {
        if (GotNoHealth() && !dismantling)
        {
            dismantling = true;
            Invoke(nameof(DismantleBuilding), 0.5f);
        }
    }
}
