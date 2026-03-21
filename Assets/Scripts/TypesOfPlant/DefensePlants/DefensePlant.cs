using UnityEngine;

public class DefensePlant : Plant
{
    private Animator animator;
    private float cHealth;
    private float timer;
    public float healRate = 10f;
    public float healPercentage = 0.1f;
    // this type of plant just have a large HP and some special ability, so we can just use the base class and add some new functions
    void Start()
    {
        animator = GetComponent<Animator>();
        cHealth = health;
        animator.SetFloat("HP", health);
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= healRate)
        {
            Heal(maxHealth * healPercentage);
            timer = 0f;
        }
        if (cHealth != health)
        {
            cHealth = health;
            animator.SetFloat("HP", health);
        }
    }
    void Heal(float healAmount)
    {
        health += healAmount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }
}