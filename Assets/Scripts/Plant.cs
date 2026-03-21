using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float cost;
    public float maxHealth;
    public float health;
    public float cooldownTime;
    private FieldSlot fieldSlot;
    void Start()
    {
        health = maxHealth;
    }
    public void TakeDamage(float dmgTaken)
    {
        health -= dmgTaken;

        if (health <= 0) Die();
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
        return cost;
    }
    public void SetFieldSlot(FieldSlot slot)
    {
        fieldSlot = slot;
        Debug.Log("Field slot set for plant at " + fieldSlot.name);
    }
    public float GetCooldownTime()
    {
        return cooldownTime;
    }
}
