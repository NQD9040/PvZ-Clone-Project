using UnityEngine;

public class DefensePlant : Plant
{
    [Header("Defense Plant Data")]
    DefensePlantData defensePlantData;
    private Animator animator;
    private float cHealth;
    private float timer;
    // this type of plant just have a large HP and some special ability, so we can just use the base class and add some new functions
    void Start()
    {
        animator = GetComponent<Animator>();
        defensePlantData = (DefensePlantData)data;
        cHealth = health;
        animator.SetFloat("HP", health);
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= defensePlantData.healRate)
        {
            Heal(data.maxHealth * defensePlantData.healPercentage);
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
        if (health > data.maxHealth)
        {
            health = data.maxHealth;
        }
    }
}