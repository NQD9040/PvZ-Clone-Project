using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ZombieCostData
{
    public GameObject prefab;
    public int cost;
}

public class WaveManager : MonoBehaviour
{
    [Header("Level Data")]
    private LevelData levelData;

    [Header("Wave Settings")]
    public float basePointValue = 20f;
    public float increment = 10f;
    public float flagMultiplier = 2.5f;
    public int flagInterval = 10;
    public float startDelay = 30f;
    public float maxWaveTime = 45f;
    public float spawnInterval = 1.5f;
    public float[] yColumnSpawn = new float[5] { 2.2f, 0.6f, -1.0f, -2.6f, -4.2f };
    public Image progressBar;

    private int waveNumber;
    private float waveTimer;
    private bool isSpawning;
    private bool waveInProgress;
    private bool levelCompleted;
    private bool gameplayRequested;
    private bool gameplayStarted;
    private ZombiePool zombiePool;
    private readonly List<int> lanePool = new List<int>();
    private readonly List<ZombieCostData> availableZombies = new List<ZombieCostData>();

    public event Action LevelCompleted;

    private IEnumerator Start()
    {
        // GameManager assigns the selected player's level from Start.
        yield return null;

        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager is missing; waves cannot start.", this);
            enabled = false;
            yield break;
        }

        levelData = LevelManager.Instance.LoadLevel(LevelManager.Instance.currentLevel);
        if (levelData == null || levelData.maxWaves <= 0)
        {
            Debug.LogError("Current level has no valid wave data.", this);
            enabled = false;
            yield break;
        }

        zombiePool = ZombiePool.Instance;
        if (zombiePool == null)
            zombiePool = new GameObject("ZombiePool").AddComponent<ZombiePool>();
        zombiePool.Prewarm(levelData.zombieTypes, 3);

        Debug.Log("Wave data is ready; waiting for plant selection confirmation.");
        TryStartGameplay();
    }

    public void BeginGameplay()
    {
        if (gameplayStarted || levelCompleted) return;
        gameplayRequested = true;
        TryStartGameplay();
    }

    private void TryStartGameplay()
    {
        if (!gameplayRequested || gameplayStarted || levelCompleted || levelData == null) return;
        gameplayStarted = true;
        Debug.Log($"Game started! You have {startDelay} seconds to prepare.");
        Invoke(nameof(StartNextWave), Mathf.Max(0f, startDelay));
    }

    private void Update()
    {
        if (!waveInProgress || isSpawning || levelCompleted) return;

        waveTimer += Time.deltaTime;
        bool timerExpired = waveTimer >= maxWaveTime;
        bool allEnemiesDefeated = zombiePool == null || zombiePool.ActiveZombieCount == 0;

        if (timerExpired || allEnemiesDefeated)
        {
            Debug.Log(timerExpired ? "Time's up! Wave ended." : "All enemies defeated! Wave ended.");
            FinishWave();
        }
    }

    private void StartNextWave()
    {
        if (levelCompleted || levelData == null) return;
        if (waveNumber >= levelData.maxWaves)
        {
            CompleteLevel();
            return;
        }

        waveNumber++;
        if (progressBar != null)
            progressBar.fillAmount = (float)waveNumber / levelData.maxWaves;

        float budget = CalculateBudget(waveNumber);
        waveTimer = 0f;
        waveInProgress = true;
        Debug.Log($"Wave {waveNumber} started | Budget: {budget}");

        if (waveNumber % 10 == 0 || waveNumber == 1 || waveNumber == levelData.maxWaves)
        {
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.firstWaveSound);
        }

        if (waveNumber % 5 == 0 && waveNumber != 1)
        {
            spawnInterval = Mathf.Max(0.3f, spawnInterval - 0.2f);
            maxWaveTime = Mathf.Max(10f, maxWaveTime - 5f);
            increment += 5f;
        }

        StartCoroutine(SpawnWave(budget));
    }

    private IEnumerator SpawnWave(float currentBudget)
    {
        isSpawning = true;
        float remainingBudget = currentBudget;

        while (remainingBudget > 0f)
        {
            availableZombies.Clear();
            if (levelData.zombieTypes != null)
            {
                foreach (ZombieCostData zombie in levelData.zombieTypes)
                {
                    if (zombie != null && zombie.prefab != null && zombie.cost > 0 && zombie.cost <= remainingBudget)
                        availableZombies.Add(zombie);
                }
            }

            if (availableZombies.Count == 0 || yColumnSpawn == null || yColumnSpawn.Length == 0)
                break;

            ZombieCostData chosen = availableZombies[UnityEngine.Random.Range(0, availableZombies.Count)];
            if (lanePool.Count == 0) ResetLanePool();
            int randomIndex = UnityEngine.Random.Range(0, lanePool.Count);
            int selectedLane = lanePool[randomIndex];
            lanePool.RemoveAt(randomIndex);

            Vector2 spawnPos = new Vector2(9.6f, yColumnSpawn[selectedLane]);
            zombiePool.GetZombie(chosen.prefab, spawnPos);
            remainingBudget -= chosen.cost;

            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"Wave {waveNumber} has spawned all enemies.");
    }

    private void FinishWave()
    {
        waveInProgress = false;
        if (waveNumber >= levelData.maxWaves)
        {
            CompleteLevel();
            return;
        }

        Invoke(nameof(StartNextWave), 5f);
    }

    private void CompleteLevel()
    {
        if (levelCompleted) return;
        levelCompleted = true;
        waveInProgress = false;
        CancelInvoke(nameof(StartNextWave));
        Debug.Log("Level completed!");
        LevelCompleted?.Invoke();
    }

    private float CalculateBudget(int wave)
    {
        float currentPoints = basePointValue + wave * increment;
        return flagInterval > 0 && wave % flagInterval == 0
            ? currentPoints * flagMultiplier
            : currentPoints;
    }

    private void ResetLanePool()
    {
        lanePool.Clear();
        for (int i = 0; i < yColumnSpawn.Length; i++) lanePool.Add(i);
    }
}
