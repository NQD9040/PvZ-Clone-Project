using UnityEngine;

public class DefensePlant : Plant
{
    private Animator animator;
    private float cHealth;
    // this type of plant just have a large HP and some special ability, so we can just use the base class and add some new functions
    void Start()
    {
        animator = GetComponent<Animator>();
        cHealth = health;
        animator.SetFloat("HP", health);
    }
    void Update()
    {
        if (cHealth != health)
        {
            cHealth = health;
            animator.SetFloat("HP", health);
        }
    }
}