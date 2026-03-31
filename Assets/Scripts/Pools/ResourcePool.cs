using System.Collections.Generic;
using UnityEngine;

public class ResourcePool : MonoBehaviour
{
    public static ResourcePool Instance;

    public GameObject sunPrefab;
    public GameObject bigSunPrefab;

    private Queue<GameObject> sunPool = new Queue<GameObject>();
    private Queue<GameObject> bigSunPool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetResource(DropResource.ResourceType type, Vector2 pos)
    {
        Queue<GameObject> pool = type == DropResource.ResourceType.Sun ? sunPool : bigSunPool;
        GameObject prefab = type == DropResource.ResourceType.Sun ? sunPrefab : bigSunPrefab;

        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab);
        }

        obj.transform.position = pos;
        obj.SetActive(true);

        return obj;
    }

    public void ReturnResource(GameObject obj)
    {
        obj.SetActive(false);

        DropResource drop = obj.GetComponent<DropResource>();

        if (drop.resourceType == DropResource.ResourceType.Sun)
        {
            sunPool.Enqueue(obj);
        }
        else
        {
            bigSunPool.Enqueue(obj);
        }
    }
}