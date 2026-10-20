using System.Collections.Generic;
using UnityEngine;

public class ZombiePool : MonoBehaviour
{
    public static ZombiePool Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<Zombie>> pools = new Dictionary<GameObject, Queue<Zombie>>();
    private readonly HashSet<Zombie> pooledZombies = new HashSet<Zombie>();
    private readonly Dictionary<Zombie, GameObject> prefabByZombie = new Dictionary<Zombie, GameObject>();
    public int ActiveZombieCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Prewarm(IEnumerable<ZombieCostData> zombieTypes, int countPerType)
    {
        if (zombieTypes == null) return;

        foreach (ZombieCostData type in zombieTypes)
        {
            if (type == null || type.prefab == null) continue;
            Queue<Zombie> pool = GetOrCreatePool(type.prefab);
            while (pool.Count < countPerType)
            {
                Zombie zombie = CreateZombie(type.prefab);
                if (zombie == null) break;
                ReturnZombie(zombie);
            }
        }
    }

    public Zombie GetZombie(GameObject prefab, Vector2 position)
    {
        if (prefab == null)
        {
            Debug.LogError("Cannot spawn a zombie without a prefab.");
            return null;
        }

        Queue<Zombie> pool = GetOrCreatePool(prefab);
        Zombie zombie = null;
        while (pool.Count > 0 && zombie == null)
        {
            zombie = pool.Dequeue();
            pooledZombies.Remove(zombie);
        }

        if (zombie == null) zombie = CreateZombie(prefab);
        if (zombie == null) return null;

        zombie.transform.SetParent(transform, false);
        zombie.transform.position = position;
        zombie.gameObject.SetActive(true);
        zombie.PrepareForSpawn();
        ActiveZombieCount++;
        return zombie;
    }

    public void ReturnZombie(Zombie zombie)
    {
        if (zombie == null || pooledZombies.Contains(zombie)) return;

        if (!prefabByZombie.TryGetValue(zombie, out GameObject prefab) || prefab == null)
        {
            Destroy(zombie.gameObject);
            return;
        }

        zombie.gameObject.SetActive(false);
        zombie.transform.SetParent(transform, false);
        GetOrCreatePool(prefab).Enqueue(zombie);
        pooledZombies.Add(zombie);
        ActiveZombieCount = Mathf.Max(0, ActiveZombieCount - 1);
    }

    private Queue<Zombie> GetOrCreatePool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Queue<Zombie> pool))
        {
            pool = new Queue<Zombie>();
            pools.Add(prefab, pool);
        }
        return pool;
    }

    private Zombie CreateZombie(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, transform);
        Zombie zombie = instance.GetComponent<Zombie>();
        if (zombie == null)
        {
            Debug.LogError($"Zombie prefab '{prefab.name}' has no Zombie component.", prefab);
            Destroy(instance);
            return null;
        }
        instance.SetActive(false);
        prefabByZombie[zombie] = prefab;
        return zombie;
    }
}
