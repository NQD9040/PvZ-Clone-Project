using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public LevelData[] levels;
    public int currentLevel;
    private WaveManager waveManager;
    private GameManager gameManager;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        waveManager = FindAnyObjectByType<WaveManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        if (levels.Length > 0)
        {
            LoadLevel(currentLevel);
        }
    }
    void Update()
    {
        if (waveManager.LevelCompleted())
        {
            NextLevel();
        }
    }
    void NextLevel()
    {
        currentLevel++;
        if (currentLevel > levels.Length)
        {
            Debug.Log("All levels completed!");
            return;
        }
        gameManager.CompleteLevel();
    }
    public LevelData LoadLevel(int level)
    {
        if (level >= 1 && level <= levels.Length)
        {
            return levels[level - 1];
        }
        Debug.LogError("Invalid level number");
        return null;
    }
}
