using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ResourcePlant : Plant
{
    public float produceRate = 10f;
    private float timer;
    public float doubleProduceChance = 20f;
    private Animator animator;
    void Start ()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= produceRate)
        {
            animator.SetBool("isProducing", true);
            timer = 0;
        }
    }
    void SpawnResource()
    {
        bool doubleProduce = Random.value * 100 < doubleProduceChance;
        GameObject res = ResourcePool.Instance.GetResource(doubleProduce ? DropResource.ResourceType.BigSun
         : DropResource.ResourceType.Sun, transform.position);
        DropResource drop = res.GetComponent<DropResource>();
    }
    void EndProduce()
    {
        animator.SetBool("isProducing", false);
    }
}