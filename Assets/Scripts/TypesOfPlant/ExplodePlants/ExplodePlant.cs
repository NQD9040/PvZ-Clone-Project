using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ExplodePlant : Plant
{
    public float radius = 3f;
    public string typeOfExplode; //is like explode a square area or a straight line or anything else
    public bool isActive; // instantly explode or taking some condition to explode like potato mine
    public float dmgDealt = 2500; // default damage, can be changed
    void Start()
    {
        if (isActive) Explode();
    }
    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach(var hit in hits)
        {
            Zombie z = hit.GetComponent<Zombie>();
            if(z != null)
            {
                z.TakeDamage(dmgDealt);
            }
        }

        Destroy(gameObject);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}