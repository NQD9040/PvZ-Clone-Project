using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    [System.Serializable]
    public class ProjectileData
    {
        public string name;
        public GameObject prefab;
        [HideInInspector] public Queue<GameObject> pool = new Queue<GameObject>();
    }

    public List<ProjectileData> projectiles = new List<ProjectileData>();
    public int initialPoolSize = 10;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (var p in projectiles)
        {
            if (p.prefab == null) continue; // skip nếu chưa gán prefab

            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = Instantiate(p.prefab);
                obj.SetActive(false);
                p.pool.Enqueue(obj);
            }
        }
    }

    public GameObject GetProjectile(string name, Vector2 pos)
    {
        ProjectileData pData = projectiles.Find(p => p.name == name);
        if (pData == null || pData.prefab == null)
        {
            Debug.LogWarning("Projectile prefab not found for " + name);
            return null;
        }

        GameObject obj;
        if (pData.pool.Count > 0)
        {
            obj = pData.pool.Dequeue();
        }
        else
        {
            obj = Instantiate(pData.prefab);
        }

        obj.transform.position = pos;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnResource(GameObject obj, string name)
    {
        ProjectileData pData = projectiles.Find(p => p.name == name);
        if (pData == null) return;

        obj.SetActive(false);
        pData.pool.Enqueue(obj);
    }
}