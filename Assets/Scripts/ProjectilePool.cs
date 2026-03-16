using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;
    public GameObject peaPrefab;
    public int poolSize = 20;
    private Queue<GameObject> peaPool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(peaPrefab);
            obj.SetActive(false);
            peaPool.Enqueue(obj);
        }
    }
    public GameObject GetProjectile(Vector2 pos)
    {

        GameObject obj;

        if (peaPool.Count > 0)
        {
            obj = peaPool.Dequeue();
        }
        else
        {
            obj = Instantiate(peaPrefab);
        }

        obj.transform.position = pos;
        obj.SetActive(true);

        return obj;
    }

    public void ReturnResource(GameObject obj)
    {
        obj.SetActive(false);

        peaPool.Enqueue(obj);
    }
}