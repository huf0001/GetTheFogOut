using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; protected set; }

    [SerializeField] List<Projectile> projectilesInPlay = new List<Projectile>();
    [SerializeField] List<Projectile> projectilesInPool = new List<Projectile>();

    [SerializeField] private int poolSize = 30;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material invisibleMaterial;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more world managers.");
        }

        Instance = this;
        PopulatePool();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PopulatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            projectilesInPool.Add(CreateProjectile());
        }
    }

    Projectile CreateProjectile()
    {
        Projectile p = Instantiate<Projectile>(projectilePrefab);
        p.gameObject.SetActive(false);
        p.transform.SetParent(this.transform, true);
        return p;
    }

    public Projectile GetFromPool()
    {
        Projectile p;
        int lastAvailableIndex = projectilesInPool.Count - 1;
        if (lastAvailableIndex >= 0)
        {
            p = projectilesInPool[lastAvailableIndex];
            projectilesInPool.RemoveAt(lastAvailableIndex);
            p.gameObject.SetActive(true);
            p.gameObject.GetComponent<Renderer>().material = visibleMaterial;
        }
        else
        {
            p = CreateProjectile();
            p.gameObject.SetActive(true);
        }

        projectilesInPlay.Add(p);

        return p;
    }

    public void ReturnToPool(Projectile p)
    {
        p.gameObject.SetActive(false);
        p.gameObject.GetComponent<Renderer>().material = invisibleMaterial;
        projectilesInPool.Add(p);
        projectilesInPlay.Remove(p);
    }
}
