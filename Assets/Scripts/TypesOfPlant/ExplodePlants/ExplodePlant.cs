using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ExplodePlant : Plant
{
    public float radius = 3f;
    public string typeOfExplode; //is like explode a square area or a straight line or anything else
    public bool isActive; // instantly explode or taking some condition to explode like potato mine
    public float dmgDealt = 2500; // default damage, can be changed
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        if (isActive)
        {
            animator.SetTrigger("Explode");
        }
    }
    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        health = 9999; // make sure the plant will die after exploding
        foreach(var hit in hits)
        {
            Zombie z = hit.GetComponent<Zombie>();
            if(z != null)
            {
                z.TakeDamage(dmgDealt);
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive) return;
        if (collision.CompareTag("Zombie"))
        {
            Debug.Log("Explode plant triggered by zombie collision!");
            animator.SetTrigger("Explode");
        }
    }
    void PotatoMineExplode()
    {
        SoundManager.instance.PlaySound(SoundManager.instance.potatoMineActivate);
    }
    void CherryBombExplode()
    {
        SoundManager.instance.PlaySound(SoundManager.instance.cherryBombActivate);
    }
    void DestroyPlant()
    {
        Die(false); // Don't play the sound again when destroying the plant
    }
}