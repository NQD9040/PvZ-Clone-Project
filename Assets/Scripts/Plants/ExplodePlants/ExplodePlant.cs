using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ExplodePlant : Plant
{
    [Header("Explode Plant Data")]
    ExplodePlantData explodePlantData;
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        explodePlantData = (ExplodePlantData)data;
        if (explodePlantData.isActive)
        {
            animator.SetTrigger("Explode");
        }
    }
    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodePlantData.radius);
        health = 9999; // make sure the plant will die after exploding
        foreach(var hit in hits)
        {
            Zombie z = hit.GetComponent<Zombie>();
            if(z != null)
            {
                z.TakeDamage(explodePlantData.dmgDealt);
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodePlantData.radius);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (explodePlantData.isActive) return;
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
    public void PotatoMineArm()
    {
        if (!explodePlantData.isActive)
        {
            animator.speed = 0;
        }
        StartCoroutine(ArmRoutine());
    }
    IEnumerator ArmRoutine()
    {
        yield return new WaitForSeconds(explodePlantData.armTime);

        animator.speed = 1;
        health = 9999;
    }
}