using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ResourcePlant : Plant
{
    [Header("Resource Plant Data")]
    ResourcePlantData resourcePlantData;
    private float timer;
    private Animator animator;
    void Start ()
    {
        animator = GetComponent<Animator>();
        resourcePlantData = (ResourcePlantData)data;
        timer += resourcePlantData.produceRate / 2f;
    }
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= resourcePlantData.produceRate)
        {
            animator.SetBool("isProducing", true);
            timer = 0;
        }
    }
    void SpawnResource()
    {
        bool doubleProduce = Random.value * 100 < resourcePlantData.doubleProduceChance;
        GameObject res = ResourcePool.Instance.GetResource(doubleProduce ? DropResource.ResourceType.BigSun
         : DropResource.ResourceType.Sun, transform.position);
        DropResource drop = res.GetComponent<DropResource>();
    }
    void EndProduce()
    {
        animator.SetBool("isProducing", false);
    }
}