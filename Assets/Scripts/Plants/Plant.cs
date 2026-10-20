using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Plant : MonoBehaviour
{
    [Header("Data")]
    public PlantData data;

    [Header("Runtime")]
    public float health;
    private FieldSlot fieldSlot;
    private WorldHealthBar worldHealthBar;
    private float healthBarMaxHealth;

    protected virtual void Awake()
    {
        if (data != null)
        {
            health = data.maxHealth;
            healthBarMaxHealth = data.maxHealth;
        }
        worldHealthBar = GetComponent<WorldHealthBar>();
        if (worldHealthBar == null) worldHealthBar = gameObject.AddComponent<WorldHealthBar>();
        UpdateHealthBar();
    }

    void Start()
    {
        health = data.maxHealth;
        healthBarMaxHealth = data.maxHealth;
        UpdateHealthBar();
    }
    public void TakeDamage(float dmgTaken)
    {
        health -= dmgTaken;

        if (health <= 0) Die();
        else UpdateHealthBar();
    }
    public void Die(bool playSound = true)
    {
        if (playSound)
        {
            SoundManager.instance.PlaySound(SoundManager.instance.zombieGulp);
        }
        Destroy(gameObject);
        if (fieldSlot != null)
        {
            fieldSlot.SetOccupied(false);
        }
    }
    public float GetCost()
    {
        return data.cost;
    }
    public void SetFieldSlot(FieldSlot slot)
    {
        fieldSlot = slot;
        Debug.Log("Field slot set for plant at " + fieldSlot.name);
    }
    public float GetCooldownTime()
    {
        return data.cooldownTime;
    }

    protected void UpdateHealthBar()
    {
        if (worldHealthBar == null) return;
        if (data != null) healthBarMaxHealth = Mathf.Max(healthBarMaxHealth, data.maxHealth);
        healthBarMaxHealth = Mathf.Max(healthBarMaxHealth, health);
        worldHealthBar.SetHealth(health, healthBarMaxHealth > 0f ? healthBarMaxHealth : 1f);
    }
}
