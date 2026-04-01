using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Chomper : Plant
{
    public float attackRange = 1f;
    public float attackDmg = 200f;
    public float chewDuration = 42f;
    public LayerMask zombieLayer;
    private bool isChewing = false;
    private Zombie currentTarget;
    // unchompable zombies list
    public List<string> unchompableNames = new List<string>()
    {
        "Gargantuar",
        "Zomboni",
        "Dr. Zomboss"
    };

    private HashSet<string> unchompableSet;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        unchompableSet = new HashSet<string>(unchompableNames);
    }
    void Update()
    {
        if (!isChewing)
        {
            Chomp();
        }
    }
    bool HasZombieInRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right,
            attackRange,
            zombieLayer
        );

        return hit.collider != null;
    }
    void Chomp()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right,
            attackRange,
            zombieLayer
        );

        if (hit.collider == null) return;

        Zombie target = hit.collider.GetComponent<Zombie>();
        if (target == null) return;

        currentTarget = target;
        animator.SetTrigger("Chomp");
    }
    void Chomping()
    {
        if (currentTarget == null) return;

        if (unchompableSet.Contains(currentTarget.gameObject.name.Replace("(Clone)", "")))
        {
            // dealt damage as normal
            currentTarget.TakeDamage(attackDmg);
        }
        else
        {
            // instant kill
            currentTarget.TakeDamage(99999);
            StartCoroutine(Chew());
        }

        SoundManager.instance.PlaySound(SoundManager.instance.chomperEat);

        currentTarget = null;
    }
    IEnumerator Chew()
    {
        isChewing = true;
        animator.ResetTrigger("Chomp");

        animator.SetBool("isChewing", true);

        yield return new WaitForSeconds(chewDuration);

        isChewing = false;
        animator.SetBool("isChewing", false);
    }
}