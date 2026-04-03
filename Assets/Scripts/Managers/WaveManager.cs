using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZombieCostData
{
    public GameObject prefab;
    public int cost;
}

public class WaveManager : MonoBehaviour
{
    [Header("Zombie Prefabs")]
    public List<ZombieCostData> zombiePrefabs;

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
    private bool isPlaySound = false;

    private List<int> lanePool = new List<int>();

    void Start()
    {
        Debug.Log($"Game started! You have {startDelay} seconds to prepare.");
        Invoke(nameof(StartNextWave), startDelay);
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
            Invoke(nameof(StartNextWave), 5f);
        }
    }

    void StartNextWave()
    {
        waveNumber++;
        float budget = CalculateBudget(waveNumber);

        waveTimer = 0f;
        waveInProgress = true;
        isPlaySound = false;

        Debug.Log($"<color=red>Wave {waveNumber} started | Budget: {budget}</color>");

        // Sound
        if ((waveNumber % 10 == 0 || waveNumber == 1) && !isPlaySound)
        {
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySound(SoundManager.instance.firstWaveSound);
            }
            isPlaySound = true;
        }

        // Difficulty scaling
        if (waveNumber % 5 == 0 && waveNumber != 1)
        {
            spawnInterval = Mathf.Max(0.3f, spawnInterval - 0.2f);
            maxWaveTime = Mathf.Max(10f, maxWaveTime - 5f);
            increment += 5f;

            Debug.Log($"<color=yellow>Difficulty increased! Spawn Interval: {spawnInterval}s | Max Wave Time: {maxWaveTime}s | Budget Increment: {increment}</color>");
        }

        StartCoroutine(SpawnWave(budget));
    }

    IEnumerator SpawnWave(float currentBudget)
    {
        isSpawning = true;
        float remainingBudget = currentBudget;

        while (remainingBudget > 0)
        {
            // Lọc zombie spawn được
            List<ZombieCostData> available = zombiePrefabs.FindAll(z => z.cost <= remainingBudget);

            if (available.Count == 0)
                break;

            ZombieCostData chosen = available[Random.Range(0, available.Count)];

            // Lane random không bị trùng liên tục
            if (lanePool.Count == 0)
                ResetLanePool();

            int randomIndex = Random.Range(0, lanePool.Count);
            int selectedLane = lanePool[randomIndex];
            lanePool.RemoveAt(randomIndex);

            Vector2 spawnPos = new Vector2(9.6f, yColumnSpawn[selectedLane]);
            Instantiate(chosen.prefab, spawnPos, Quaternion.identity);

            remainingBudget -= chosen.cost;

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
        lanePool.Clear();
        for (int i = 0; i < yColumnSpawn.Length; i++)
        {
            lanePool.Add(i);
        }
    }
}