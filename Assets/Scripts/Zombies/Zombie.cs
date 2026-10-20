using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Data")]
    public ZombieData data;

    private float health;
    private float currentShieldHealth;
    private float moveSpeed;
    private float dmgRate;
    private Animator animator;
    private WorldHealthBar worldHealthBar;
    private WorldHealthBar shieldHealthBar;
    private SpriteRenderer[] bodyRenderers;
    private Color[] originalBodyColors;
    private GameManager gameManager;
    private readonly HashSet<Plant> plantsInContact = new HashSet<Plant>();
    private Plant targetPlant;
    private float attackTimer;
    private Coroutine slowCoroutine;
    private bool isSlowed;
    private bool hasTriggeredGameOver;
    private readonly GameObject[] shieldStages = new GameObject[3];
    private const float GAME_OVER_X = -8f;

    private enum State { Move, Eat, Dead }
    private State currentState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        worldHealthBar = GetComponent<WorldHealthBar>();
        if (worldHealthBar == null) worldHealthBar = gameObject.AddComponent<WorldHealthBar>();
        shieldHealthBar = gameObject.AddComponent<WorldHealthBar>();
        shieldHealthBar.SetVerticalOffset(0.34f);
        shieldHealthBar.SetShieldStyle();
        shieldHealthBar.SetVisible(false);
        CacheBodyRenderers();
    }

    public void PrepareForSpawn()
    {
        if (data == null)
        {
            Debug.LogError($"Zombie data is missing on {name}.", this);
            return;
        }

        if (animator == null) animator = GetComponent<Animator>();
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        CacheShieldStages();
        SetSlowedVisual(false);

        health = data.maxHealth;
        currentShieldHealth = Mathf.Max(0f, data.shieldHealth);
        moveSpeed = data.moveSpeed;
        dmgRate = data.dmgRate;
        attackTimer = 0f;
        isSlowed = false;
        hasTriggeredGameOver = false;
        plantsInContact.Clear();
        targetPlant = null;
        currentState = State.Move;

        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool("isMoving", true);
            animator.SetBool("isEating", false);
        }

        UpdateShieldVisuals();
        UpdateHealthBar();
    }

    private void OnDisable()
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }
        plantsInContact.Clear();
        targetPlant = null;
    }

    private void Update()
    {
        if (currentState == State.Dead) return;

        targetPlant = GetContactPlant();
        currentState = targetPlant != null ? State.Eat : State.Move;

        if (currentState == State.Eat)
            HandleEat();
        else
            HandleMove();

        CheckGameOver();
    }

    private Plant GetContactPlant()
    {
        plantsInContact.RemoveWhere(plant => plant == null);
        foreach (Plant plant in plantsInContact)
            return plant;
        return null;
    }

    private void HandleMove()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
            animator.SetBool("isEating", false);
        }
    }

    private void HandleEat()
    {
        if (targetPlant == null) return;

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isEating", true);
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= dmgRate)
        {
            targetPlant.TakeDamage(data.dmgDealt);
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.zombieEat);
            attackTimer = 0f;
        }
    }

    public void TakeDamage(float damage, bool applySlow = false)
    {
        if (currentState == State.Dead) return;

        float healthDamage = ApplyShield(Mathf.Max(0f, damage));
        health -= healthDamage;
        UpdateHealthBar();

        if (health <= 0f)
        {
            Die();
            return;
        }

        if (applySlow) ApplySlow();
    }

    public string GetShieldStatus()
    {
        if (currentShieldHealth <= 0f) return "None";
        if (data.shield == ZombieData.shieldType.Conehead) return "Conehead";
        if (data.shield == ZombieData.shieldType.Buckethead) return "Buckethead";
        return "None";
    }

    private float ApplyShield(float damage)
    {
        if (currentShieldHealth <= 0f) return damage;

        float absorbed = Mathf.Min(currentShieldHealth, damage);
        currentShieldHealth -= absorbed;
        UpdateShieldVisuals();
        return damage - absorbed;
    }

    private void UpdateShieldVisuals()
    {
        string prefix = data.shield == ZombieData.shieldType.Conehead ? "Conehead_" :
            data.shield == ZombieData.shieldType.Buckethead ? "Buckethead_" : null;
        foreach (GameObject stage in shieldStages)
            if (stage != null) stage.SetActive(false);

        if (prefix == null || data.shieldHealth <= 0f || currentShieldHealth <= 0f) return;

        float remaining = currentShieldHealth / data.shieldHealth;
        int visibleStage = remaining <= 1f / 3f ? 2 : remaining <= 2f / 3f ? 1 : 0;
        if (shieldStages[visibleStage] != null)
            shieldStages[visibleStage].SetActive(true);
    }

    private void UpdateHealthBar()
    {
        if (worldHealthBar == null || data == null) return;
        worldHealthBar.SetHealth(health, data.maxHealth);
        if (shieldHealthBar != null)
        {
            shieldHealthBar.SetHealth(currentShieldHealth, data.shieldHealth);
            shieldHealthBar.SetVisible(
                data.shield != ZombieData.shieldType.None &&
                data.shieldHealth > 0f &&
                currentShieldHealth > 0f);
        }
    }

    private void CacheShieldStages()
    {
        for (int i = 0; i < shieldStages.Length; i++) shieldStages[i] = null;
        if (data.shield == ZombieData.shieldType.None) return;

        string prefix = data.shield == ZombieData.shieldType.Conehead ? "Conehead_" : "Buckethead_";
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.name.StartsWith(prefix)) continue;
            string suffix = child.name.Substring(prefix.Length);
            if (int.TryParse(suffix, out int stage) && stage >= 0 && stage < shieldStages.Length)
                shieldStages[stage] = child.gameObject;
        }
    }

    private void ApplySlow()
    {
        if (!isSlowed)
        {
            isSlowed = true;
            moveSpeed *= 0.5f;
            dmgRate *= 2f;
            if (animator != null) animator.speed = 0.5f;
            SetSlowedVisual(true);

            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySound(SoundManager.instance.snowEffect);
                SoundManager.instance.PlaySound(SoundManager.instance.slowDownEffect);
            }
        }

        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(RemoveSlowAfterTime(5f));
    }

    private System.Collections.IEnumerator RemoveSlowAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        moveSpeed = data.moveSpeed;
        dmgRate = data.dmgRate;
        if (animator != null) animator.speed = 1f;
        isSlowed = false;
        SetSlowedVisual(false);
        slowCoroutine = null;
    }

    private void CacheBodyRenderers()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        List<SpriteRenderer> body = new List<SpriteRenderer>(allRenderers.Length);
        List<Color> colors = new List<Color>(allRenderers.Length);

        foreach (SpriteRenderer spriteRenderer in allRenderers)
        {
            if (spriteRenderer.transform.name.StartsWith("HealthBar")) continue;
            body.Add(spriteRenderer);
            colors.Add(spriteRenderer.color);
        }

        bodyRenderers = body.ToArray();
        originalBodyColors = colors.ToArray();
    }

    private void SetSlowedVisual(bool slowed)
    {
        if (bodyRenderers == null || originalBodyColors == null) return;

        Color iceTint = new Color(0.45f, 0.85f, 1f, 1f);
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] == null) continue;
            bodyRenderers[i].color = slowed
                ? Color.Lerp(originalBodyColors[i], iceTint, 0.65f)
                : originalBodyColors[i];
        }
    }

    private void Die()
    {
        currentState = State.Dead;
        if (ZombiePool.Instance != null)
            ZombiePool.Instance.ReturnZombie(this);
        else
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Plant")) return;
        Plant plant = collision.GetComponentInParent<Plant>();
        if (plant != null) plantsInContact.Add(plant);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Plant")) return;
        Plant plant = collision.GetComponentInParent<Plant>();
        if (plant != null) plantsInContact.Remove(plant);
    }

    private void CheckGameOver()
    {
        if (!hasTriggeredGameOver && transform.position.x <= GAME_OVER_X)
        {
            hasTriggeredGameOver = true;
            if (gameManager != null) gameManager.EndGame();
        }
    }
}
