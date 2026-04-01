using System.Collections;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Data")]
    public ZombieData data;

    [Header("Runtime")]
    private float health;
    private float currentMoveSpeed;
    private float currentDmgRate;

    private Animator animator;

    private Plant targetPlant;
    private float attackTimer;
    private Coroutine slowCoroutine;
    private bool isSlowed = false;

    private float gameOverPositionX = -8f;
    private GameManager gameManager;

    void Start()
    {
        // Init từ ScriptableObject
        health = data.maxHealth;
        currentMoveSpeed = data.moveSpeed;
        currentDmgRate = data.dmgRate;

        animator = GetComponent<Animator>();
        gameManager = FindAnyObjectByType<GameManager>();
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

        if (transform.position.x <= gameOverPositionX)
        {
            gameManager.EndGame();
        }
    }

    void Move()
    {
        transform.Translate(Vector2.left * currentMoveSpeed * Time.deltaTime);

        animator.SetBool("isMoving", true);
        animator.SetBool("isEating", false);
    }

    void Eat()
    {
        if (targetPlant == null) return;

        animator.SetBool("isMoving", false);
        animator.SetBool("isEating", true);

        attackTimer += Time.deltaTime;

        if (attackTimer >= currentDmgRate)
        {
            targetPlant.TakeDamage(data.dmgDealt);
            SoundManager.instance.PlaySound(SoundManager.instance.zombieEat);
            attackTimer = 0f;
        }
    }

    public void TakeDamage(float dmgTaken, bool isSlow = false)
    {
        if (data.shieldHealth > 0)
        {
            data.shieldHealth -= dmgTaken;

            if (data.shieldHealth < 0)
            {
                dmgTaken = -data.shieldHealth;
                data.shieldHealth = 0;
            }
            else
            {
                dmgTaken = 0;
            }
        }
        health -= dmgTaken;

        if (health <= 0)
        {
            Die();
            return;
        }

        if (isSlow)
        {
            ApplySlow();
        }
    }

    private void ApplySlow()
    {
        if (!isSlowed)
        {
            isSlowed = true;

            currentMoveSpeed *= 0.5f;
            currentDmgRate *= 2f;

            animator.speed = 0.5f;

            SoundManager.instance.PlaySound(SoundManager.instance.snowEffect);
            SoundManager.instance.PlaySound(SoundManager.instance.slowDownEffect);
        }

        // Reset timer nếu bị slow lại
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        slowCoroutine = StartCoroutine(SlowDown());
    }

    private IEnumerator SlowDown()
    {
        yield return new WaitForSeconds(5f);

        currentMoveSpeed = data.moveSpeed;
        currentDmgRate = data.dmgRate;

        animator.speed = 1f;
        isSlowed = false;
    }

    void Die()
    {
        gameManager.IncrementZombiesKilled();
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