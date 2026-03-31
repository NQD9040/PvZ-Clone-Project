using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Zombie Prefabs")]
    public GameObject normalZombiePrefab;   // cost = 10

    [Header("Wave Settings")]
    public float basePointValue = 20f;      
    public float increment = 10f;          
    public float flagMultiplier = 2.5f;
    public int flagInterval = 10;

    public float startDelay = 30f;
    public float maxWaveTime = 45f;         
    public float spawnInterval = 1.5f;

    public float[] yColumnSpawn = new float[5] { 2.2f, 0.6f, -1.0f, -2.6f, -4.2f };

    private int waveNumber = 0;
    private float waveTimer = 0f;
    private bool isSpawning = false;
    private bool waveInProgress = false;
    private List<int> lanePool = new List<int>();

    void Start()
    {
        Debug.Log($"Game started! You have {startDelay} seconds to prepare.");
        Invoke("StartNextWave", startDelay);
    }

    void Update()
    {
        if (!waveInProgress || isSpawning) return;

        waveTimer += Time.deltaTime;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Zombie");
        bool timerExpired = waveTimer >= maxWaveTime;
        bool allEnemiesDefeated = enemies.Length == 0;

        if (timerExpired || allEnemiesDefeated)
        {
            waveInProgress = false;
            Debug.Log(timerExpired ? "Time's up! Next wave." : "All enemies defeated! Next wave.");
            Invoke("StartNextWave", 5f);
        }
    }

    void StartNextWave()
    {
        waveNumber++;
        float budget = CalculateBudget(waveNumber);
        waveTimer = 0f;
        waveInProgress = true;

        Debug.Log($"<color=red>Wave {waveNumber} bắt đầu | Budget: {budget}</color>");
        StartCoroutine(SpawnWave(budget));
    }

    IEnumerator SpawnWave(float currentBudget)
    {
        isSpawning = true;
        float remainingBudget = currentBudget;

        while (remainingBudget >= 10)
        {
            if (lanePool.Count == 0) ResetLanePool();
            int randomIndex = Random.Range(0, lanePool.Count);
            int selectedLane = lanePool[randomIndex];
            lanePool.RemoveAt(randomIndex);

            // Spawn
            Vector2 spawnPos = new Vector2(9.6f, yColumnSpawn[selectedLane]);
            Instantiate(normalZombiePrefab, spawnPos, Quaternion.identity);

            remainingBudget -= 10;

            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"Wave {waveNumber} has spawned all enemies.");
    }

    float CalculateBudget(int wave)
    {
        float currentPoints = basePointValue + (wave * increment);
        
        if (wave % flagInterval == 0)
        {
            return currentPoints * flagMultiplier;
        }
        
        return currentPoints;
    }

    void ResetLanePool()
    {
        for (int i = 0; i < yColumnSpawn.Length; i++)
        {
            lanePool.Add(i);
        }
    }
}