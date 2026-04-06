using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Player player;
    public Canvas completeLevelCanvas;
    public Button nextLevelButton;
    public Button leaveButton;
    [Header("Resource")]
    public float sunAmount = 50f;
    [SerializeField] private GameObject sunPrefab;

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private TextMeshProUGUI sunText;
    [SerializeField] private TextMeshProUGUI currentLevelText;

    [Header("Sun Spawn")]
    [SerializeField] private float sunSpawnDelay = 5f;

    private float sunTimer;

    #region Unity

    void Start()
    {
        Init();
    }

    void Update()
    {
        HandleSunSpawn();
        HandleInput();
    }

    #endregion

    #region Init

    void Init()
    {
        GetPlayer();
        nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        Debug.Log("Current Player: " + player.PlayerName + ", Progress Level: " + player.ProgressLevel);
        if (player.ProgressLevel == 0)
        {
            player.IncreaseLevel();
        }
        LevelManager.Instance.currentLevel = player.ProgressLevel;
        Time.timeScale = 1f;
        canvas.gameObject.SetActive(true);

        UpdateSunUI();
        UpdateCurrentLevelUI();

        SpawnSun();

        SoundManager.instance.StopMusic();
        SoundManager.instance.PlayMusic(SoundManager.instance.bgMusic);
    }

    #endregion

    #region Update Logic

    void HandleSunSpawn()
    {
        sunTimer += Time.deltaTime;

        if (sunTimer >= sunSpawnDelay)
        {
            SpawnSun();
            sunTimer = 0f;
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !InputManager.Instance.isComplete)
        {
            ToggleMenu();
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleMenuClick();
        }
    }

    #endregion

    #region UI

    void UpdateSunUI()
    {
        sunText.text = sunAmount.ToString();
    }

    void UpdateCurrentLevelUI()
    {
        if (player.ProgressLevel == 6)
        {
            currentLevelText.text = $"Level: {LevelManager.Instance.currentLevel}" + " (Endless)";
        }
        else
        {
            currentLevelText.text = $"Level: {LevelManager.Instance.currentLevel}";
        }
    }

    #endregion

    #region Sun System

    public void AddSun(float amount)
    {
        sunAmount += amount;
        UpdateSunUI();
    }

    void SpawnSun()
    {
        float randomX = Random.Range(-7f, 5f);
        Vector2 spawnPos = new(randomX, 6.4f);

        ResourcePool.Instance.GetResource(DropResource.ResourceType.Sun, spawnPos);
    }

    #endregion

    #region Zombie

    #endregion

    #region Menu

    void HandleMenuClick()
    {
        if (InputManager.Instance.isBlocked || InputManager.Instance.isComplete) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null && hit.gameObject == menuButton)
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        SoundManager.instance.PlaySound(SoundManager.instance.pauseSound);
        LevelMenu.Instance.ToggleMenu();
    }

    #endregion

    #region Game Flow

    public void EndGame()
    {
        Debug.Log("END GAME TRIGGERED");

        SoundManager.instance.StopMusic();
        ChangeScene.Instance.LoadScene(0);
    }
    public void CompleteLevel()
    {
        completeLevelCanvas.gameObject.SetActive(true);
        SoundManager.instance.StopMusic();
        Time.timeScale = 0f;
        InputManager.Instance.isComplete = true;
    }
    public void NextLevel()
    {
        IncreasePlayerLevel();
        ChangeScene.Instance.LoadScene(0);
        ChangeScene.Instance.LoadScene(1);
    }
    #endregion

    void OnDestroy()
    {
        Debug.Log("GameManager destroyed: " + gameObject.name);
    }

    # region Player
    void GetPlayer()
    {
        player = CurrentPlayer.Instance.player;
    }
    void IncreasePlayerLevel()
    {
        player.IncreaseLevel();
        CurrentPlayer.Instance.SaveCurrentPlayer();
    }
    #endregion
    void OnNextLevelButtonClicked()
    {
        NextLevel();
    }
    void OnLeaveButtonClicked()
    {
        IncreasePlayerLevel();
        EndGame();
    }
}