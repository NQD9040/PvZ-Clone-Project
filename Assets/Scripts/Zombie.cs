using UnityEngine;

public class Zombie : MonoBehaviour
{
    public float maxHealth;
    public float health;

    public float dmgDealt = 20;
    public float dmgRate = 0.5f;

    public float moveSpeed = 0.3f;

    private Animator animator;

    private Plant targetPlant;
    private float attackTimer;

    void Start()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (targetPlant == null)
        {
            Move();
        }
        else
        {
            Eat();
        }
    }

    void Move()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        animator.SetBool("isMoving", true);
        animator.SetBool("isEating", false);
    }

    void Eat()
    {
        if (targetPlant == null) return;

        animator.SetBool("isMoving", false);
        animator.SetBool("isEating", true);

        attackTimer += Time.deltaTime;

        if (attackTimer >= dmgRate)
        {
            targetPlant.TakeDamage(dmgDealt);
            SoundManager.instance.PlaySound(SoundManager.instance.zombieEat);
            attackTimer = 0f;
        }
    }

    public void TakeDamage(float dmgTaken)
    {
        health -= dmgTaken;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Plant"))
        {
            targetPlant = collision.GetComponent<Plant>();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Plant"))
        {
            targetPlant = null;
        }
    }
}