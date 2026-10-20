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

    private readonly Dictionary<string, ProjectileData> projectilesByName = new Dictionary<string, ProjectileData>();
    private readonly Dictionary<GameObject, ProjectileData> dataByObject = new Dictionary<GameObject, ProjectileData>();
    private readonly HashSet<GameObject> pooledObjects = new HashSet<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (ProjectileData data in projectiles)
        {
            if (data == null || string.IsNullOrEmpty(data.name) || data.prefab == null) continue;
            if (projectilesByName.ContainsKey(data.name))
            {
                Debug.LogWarning($"Duplicate projectile pool key '{data.name}'.", this);
                continue;
            }
            projectilesByName.Add(data.name, data);
        }
    }

    private void Start()
    {
        foreach (ProjectileData data in projectilesByName.Values)
        {
            for (int i = 0; i < Mathf.Max(0, initialPoolSize); i++)
            {
                GameObject obj = CreateProjectile(data);
                if (obj == null) break;
                obj.SetActive(false);
                data.pool.Enqueue(obj);
                pooledObjects.Add(obj);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public GameObject GetProjectile(string name, Vector2 pos)
    {
        if (!projectilesByName.TryGetValue(name, out ProjectileData data))
        {
            Debug.LogWarning($"Projectile prefab not found for '{name}'.", this);
            return null;
        }

        GameObject obj = null;
        while (data.pool.Count > 0 && obj == null)
        {
            obj = data.pool.Dequeue();
            pooledObjects.Remove(obj);
        }
        if (obj == null) obj = CreateProjectile(data);
        if (obj == null) return null;

        obj.transform.position = pos;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnResource(GameObject obj, string name)
    {
        if (obj == null || pooledObjects.Contains(obj)) return;
        if (!dataByObject.TryGetValue(obj, out ProjectileData data))
        {
            Debug.LogWarning($"Projectile '{obj.name}' is not registered with this pool.", obj);
            obj.SetActive(false);
            return;
        }

        obj.SetActive(false);
        data.pool.Enqueue(obj);
        pooledObjects.Add(obj);
    }

    private GameObject CreateProjectile(ProjectileData data)
    {
        GameObject obj = Instantiate(data.prefab, transform);
        if (obj.GetComponent<StraightProjectile>() == null)
        {
            Debug.LogError($"Projectile prefab '{data.prefab.name}' has no StraightProjectile component.", data.prefab);
            Destroy(obj);
            return null;
        }
        dataByObject[obj] = data;
        return obj;
    }
}
