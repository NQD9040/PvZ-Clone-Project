using System.Collections;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Data")]
    public ZombieData data;

    [Header("Runtime")]
    private float health;
    private float currentShieldHealth;
    private float moveSpeed;
    private float dmgRate;

    private Animator animator;
    private GameManager gameManager;

    private Plant targetPlant;
    private float attackTimer;

    private Coroutine slowCoroutine;
    private bool isSlowed;

    private const float GAME_OVER_X = -8f;
    private GameObject shieldObject;
    private enum State
    {
        Move,
        Eat,
        Dead
    }

    private State currentState;

    #region Unity Methods

    void Start()
    {
        if (transform.childCount != 0)
        {
            shieldObject = transform.GetChild(0).gameObject;
            shieldObject.SetActive(true);
        }
        currentShieldHealth = data.shieldHealth;
        Init();
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        UpdateState();
        HandleState();
        CheckGameOver();
    }

    #endregion

    #region Init

    void Init()
    {
        health = data.maxHealth;
        moveSpeed = data.moveSpeed;
        dmgRate = data.dmgRate;

        animator = GetComponent<Animator>();
        gameManager = FindAnyObjectByType<GameManager>();

        currentState = State.Move;
    }

    #endregion

    #region State Logic

    void UpdateState()
    {
        if (targetPlant != null)
            currentState = State.Eat;
        else
            currentState = State.Move;
    }

    void HandleState()
    {
        switch (currentState)
        {
            case State.Move:
                HandleMove();
                break;
            case State.Eat:
                HandleEat();
                break;
        }
    }

    #endregion

    #region Actions

    void HandleMove()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        animator.SetBool("isMoving", true);
        animator.SetBool("isEating", false);
    }

    void HandleEat()
    {
        if (targetPlant == null) return;

        animator.SetBool("isMoving", false);
        animator.SetBool("isEating", true);

        attackTimer += Time.deltaTime;

        if (attackTimer >= dmgRate)
        {
            targetPlant.TakeDamage(data.dmgDealt);
            SoundManager.instance.PlaySound(SoundManager.instance.zombieEat);
            attackTimer = 0f;
        }
    }

    #endregion

    #region Damage & Effects

    public void TakeDamage(float damage, bool applySlow = false)
    {
        damage = ApplyShield(damage);
        if (currentShieldHealth <=0 && shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
        health -= damage;

        if (health <= 0)
        {
            Die();
            return;
        }

        if (applySlow)
        {
            ApplySlow();
        }
    }
    public string GetShieldStatus()
    {
        if (currentShieldHealth > 0)
        {
            if (ZombieData.shieldType.Conehead == data.shield) return "Conehead";
            if (ZombieData.shieldType.Buckethead == data.shield) return "Buckethead";
        }
        return "None";
    }
    float ApplyShield(float damage)
    {
        if (currentShieldHealth <= 0) return damage;

        currentShieldHealth -= damage;

        ChangeShield();
        if (currentShieldHealth < 0)
        {
            float remain = -currentShieldHealth;
            currentShieldHealth = 0;
            shieldObject.SetActive(false);
            return remain;
        }
        return 0;
    }
    void ChangeShield()
    {
        if (currentShieldHealth <= (2*data.shieldHealth) / 3)
        {
            shieldObject.gameObject.SetActive(false);
            shieldObject = transform.GetChild(1).gameObject;
            shieldObject.gameObject.SetActive(true);
        }
        if (currentShieldHealth <= (1*data.shieldHealth) / 3)
        {
            shieldObject.gameObject.SetActive(false);
            shieldObject = transform.GetChild(2).gameObject;
            shieldObject.gameObject.SetActive(true);
        }
    }
    void ApplySlow()
    {
        if (!isSlowed)
        {
            isSlowed = true;

            moveSpeed *= 0.5f;
            dmgRate *= 2f;

            animator.speed = 0.5f;

            PlaySlowSound();
        }

        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(RemoveSlowAfterTime(5f));
    }

    IEnumerator RemoveSlowAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);

        moveSpeed = data.moveSpeed;
        dmgRate = data.dmgRate;

        animator.speed = 1f;
        isSlowed = false;
    }

    void PlaySlowSound()
    {
        SoundManager.instance.PlaySound(SoundManager.instance.snowEffect);
        SoundManager.instance.PlaySound(SoundManager.instance.slowDownEffect);
    }

    #endregion

    #region Death

    void Die()
    {
        currentState = State.Dead;

        gameManager.IncrementZombiesKilled();
        Destroy(gameObject);
    }

    #endregion

    #region Collision

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

    #endregion

    #region Utils

    void CheckGameOver()
    {
        if (transform.position.x <= GAME_OVER_X)
        {
            gameManager.EndGame();
        }
    }

    #endregion
}