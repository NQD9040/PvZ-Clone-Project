using System.Collections.Generic;
using UnityEngine;

public class ResourcePool : MonoBehaviour
{
    public static ResourcePool Instance;

    public GameObject sunPrefab;
    public GameObject bigSunPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private readonly Queue<GameObject> sunPool = new Queue<GameObject>();
    private readonly Queue<GameObject> bigSunPool = new Queue<GameObject>();
    private readonly HashSet<GameObject> pooledObjects = new HashSet<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Prewarm(sunPrefab, sunPool, initialPoolSize);
        Prewarm(bigSunPrefab, bigSunPool, initialPoolSize);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public GameObject GetResource(DropResource.ResourceType type, Vector2 pos)
    {
        bool isSun = type == DropResource.ResourceType.Sun;
        Queue<GameObject> pool = isSun ? sunPool : bigSunPool;
        GameObject prefab = isSun ? sunPrefab : bigSunPrefab;

        GameObject obj = null;
        while (pool.Count > 0 && obj == null)
        {
            obj = pool.Dequeue();
            pooledObjects.Remove(obj);
        }

        if (obj == null)
        {
            if (prefab == null)
            {
                Debug.LogError($"No prefab configured for resource type {type}.", this);
                return null;
            }
            obj = Instantiate(prefab, transform);
        }

        obj.transform.SetParent(transform, false);
        obj.transform.position = pos;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnResource(GameObject obj)
    {
        if (obj == null || pooledObjects.Contains(obj)) return;

        DropResource drop = obj.GetComponent<DropResource>();
        if (drop == null)
        {
            Debug.LogWarning($"Cannot return {obj.name}: DropResource component is missing.", obj);
            obj.SetActive(false);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform, false);
        Queue<GameObject> pool = drop.resourceType == DropResource.ResourceType.Sun ? sunPool : bigSunPool;
        pool.Enqueue(obj);
        pooledObjects.Add(obj);
    }

    private void Prewarm(GameObject prefab, Queue<GameObject> pool, int count)
    {
        if (prefab == null) return;
        for (int i = 0; i < Mathf.Max(0, count); i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
            pooledObjects.Add(obj);
        }
    }
}
