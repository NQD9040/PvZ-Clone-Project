using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public LevelData[] levels;
    public int currentLevel;

    private WaveManager waveManager;
    private GameManager gameManager;
    private bool completionHandled;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        waveManager = FindAnyObjectByType<WaveManager>();
        gameManager = FindAnyObjectByType<GameManager>();

        if (waveManager == null)
            Debug.LogError("WaveManager is missing from the level scene.", this);
        else
            waveManager.LevelCompleted += HandleLevelCompleted;

        if (levels == null || levels.Length == 0)
            Debug.LogError("No levels are configured on LevelManager.", this);
    }

    private void OnDestroy()
    {
        if (waveManager != null)
            waveManager.LevelCompleted -= HandleLevelCompleted;
        if (Instance == this) Instance = null;
    }

    private void HandleLevelCompleted()
    {
        if (completionHandled) return;
        completionHandled = true;
        currentLevel++;

        if (levels == null || currentLevel > levels.Length)
        {
            Debug.Log("All levels completed!");
            return;
        }

        if (gameManager != null)
            gameManager.CompleteLevel();
        else
            Debug.LogError("GameManager is missing; cannot show level completion UI.", this);
    }

    public LevelData LoadLevel(int level)
    {
        if (levels != null && level >= 1 && level <= levels.Length)
            return levels[level - 1];

        Debug.LogError($"Invalid level number: {level}", this);
        return null;
    }
}
