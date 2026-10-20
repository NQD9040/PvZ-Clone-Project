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
    private bool healthBarsVisible = true;
    private TextMeshProUGUI healthBarToggleLabel;
    private bool gameplayStarted;

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
        if (InputManager.Instance != null)
            InputManager.Instance.isBlocked = true;
        PlantSelectionSession.Clear();
        // Keep the world-space HUD at its scene order so collectible sprites remain visible.
        canvas.overrideSorting = false;
        canvas.sortingOrder = 0;
        canvas.gameObject.SetActive(true);
        ConfigureMenuSorting();
        healthBarsVisible = true;
        WorldHealthBar.SetAllVisible(true);
        CreateHealthBarToggleButton();

        UpdateSunUI();
        UpdateCurrentLevelUI();

        PlantSelectionController selectionController = canvas.GetComponent<PlantSelectionController>();
        if (selectionController == null || !selectionController.isActiveAndEnabled)
            StartWithoutPlantSelection();

        SoundManager.instance.StopMusic();
        SoundManager.instance.PlayMusic(SoundManager.instance.bgMusic);
    }

    #endregion

    #region Update Logic

    void HandleSunSpawn()
    {
        if (InputManager.Instance != null && InputManager.Instance.isBlocked) return;
        sunTimer += Time.deltaTime;

        if (sunTimer >= sunSpawnDelay)
        {
            SpawnSun();
            sunTimer = 0f;
        }
    }

    void HandleInput()
    {
        if (InputManager.Instance != null && InputManager.Instance.isBlocked &&
            !InputManager.Instance.isComplete) return;
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

    public void BeginGameplay()
    {
        if (gameplayStarted) return;
        gameplayStarted = true;
        sunTimer = 0f;
        if (InputManager.Instance != null)
            InputManager.Instance.isBlocked = false;
        SpawnSun();
    }

    private void StartWithoutPlantSelection()
    {
        LevelData currentLevelData = LevelManager.Instance.LoadLevel(LevelManager.Instance.currentLevel);
        if (currentLevelData != null && currentLevelData.plantCards != null)
            PlantSelectionSession.SetSelection(currentLevelData.levelNumber, currentLevelData.plantCards);

        BeginGameplay();
        WaveManager waveManager = FindAnyObjectByType<WaveManager>();
        if (waveManager != null) waveManager.BeginGameplay();
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
        WorldHealthBar.SetAllVisible(false);
        healthBarsVisible = false;
        UpdateHealthBarToggleLabel();
        if (completeLevelCanvas != null)
        {
            completeLevelCanvas.overrideSorting = true;
            completeLevelCanvas.sortingOrder = 100;
        }
        completeLevelCanvas.gameObject.SetActive(true);
        SoundManager.instance.StopMusic();
        Time.timeScale = 0f;
        InputManager.Instance.isComplete = true;
    }

    private void CreateHealthBarToggleButton()
    {
        if (canvas == null || healthBarToggleLabel != null) return;

        GameObject buttonObject = new GameObject(
            "HealthBarToggleButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button),
            typeof(Canvas), typeof(GraphicRaycaster));
        buttonObject.layer = canvas.gameObject.layer;
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(canvas.transform, false);
        Canvas buttonCanvas = buttonObject.GetComponent<Canvas>();
        buttonCanvas.overrideSorting = true;
        buttonCanvas.sortingOrder = 30;
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(738f, 365f);
        buttonRect.sizeDelta = new Vector2(220f, 68f);

        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.14f, 0.2f, 0.92f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(ToggleHealthBars);

        GameObject labelObject = new GameObject(
            "Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.layer = canvas.gameObject.layer;
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(buttonObject.transform, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 4f);
        labelRect.offsetMax = new Vector2(-8f, -4f);

        healthBarToggleLabel = labelObject.GetComponent<TextMeshProUGUI>();
        if (currentLevelText != null) healthBarToggleLabel.font = currentLevelText.font;
        healthBarToggleLabel.fontSize = 25f;
        healthBarToggleLabel.alignment = TextAlignmentOptions.Center;
        healthBarToggleLabel.color = Color.white;
        healthBarToggleLabel.raycastTarget = false;
        UpdateHealthBarToggleLabel();
    }

    private void ConfigureMenuSorting()
    {
        if (menuButton == null) return;

        Canvas menuCanvas = menuButton.GetComponent<Canvas>();
        if (menuCanvas == null) menuCanvas = menuButton.AddComponent<Canvas>();
        menuCanvas.overrideSorting = true;
        menuCanvas.sortingOrder = 30;
    }

    private void ToggleHealthBars()
    {
        if (InputManager.Instance != null && InputManager.Instance.isComplete) return;
        healthBarsVisible = !healthBarsVisible;
        WorldHealthBar.SetAllVisible(healthBarsVisible);
        UpdateHealthBarToggleLabel();
    }

    private void UpdateHealthBarToggleLabel()
    {
        if (healthBarToggleLabel != null)
            healthBarToggleLabel.text = healthBarsVisible ? "HP: ON" : "HP: OFF";
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
