using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Resource")]
    public float sunAmount = 50f;
    [SerializeField] private GameObject sunPrefab;

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private TextMeshProUGUI sunText;
    [SerializeField] private TextMeshProUGUI zombieKilledText;

    [Header("Sun Spawn")]
    [SerializeField] private float sunSpawnDelay = 5f;

    private float sunTimer;
    private int zombiesKilled;

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
        Time.timeScale = 1f;
        canvas.gameObject.SetActive(true);

        UpdateSunUI();
        UpdateKillUI();

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
        if (Input.GetKeyDown(KeyCode.Escape))
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

    void UpdateKillUI()
    {
        zombieKilledText.text = $"Zombies Killed: {zombiesKilled}";
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

    public void IncrementZombiesKilled()
    {
        zombiesKilled++;
        UpdateKillUI();
    }

    #endregion

    #region Menu

    void HandleMenuClick()
    {
        if (InputManager.Instance.isBlocked) return;

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

    #endregion

    void OnDestroy()
    {
        Debug.Log("GameManager destroyed: " + gameObject.name);
    }
}