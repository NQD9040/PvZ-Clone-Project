using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float sumAmount = 50f;
    public Canvas canvas;
    public GameObject menuButton;
    public TextMeshProUGUI zombiesKilledCountText;
    private int zombiesKilledCount = 0;
    public GameObject sunPrefab;
    public TextMeshProUGUI sunText;

    public float sunSpawnDelay = 5f;
    private float sunTimer;

    void Start()
    {
        Time.timeScale = 1f;
        canvas.gameObject.SetActive(true);
        SpawnSun();
        SoundManager.instance.StopMusic();
        SoundManager.instance.PlayMusic(SoundManager.instance.bgMusic);
    }

    void Update()
    {
        sunText.text = sumAmount.ToString();
        zombiesKilledCountText.text = "Zombies Killed: " + zombiesKilledCount;
        sunTimer += Time.deltaTime;
        if (sunTimer >= sunSpawnDelay)
        {
            SpawnSun();
            sunTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SoundManager.instance.PlaySound(SoundManager.instance.pauseSound);
            LevelMenu.Instance.ToggleMenu();
        }

        if (!Input.GetMouseButtonDown(0)) return;
        HandleMenuClick();
    }

    void HandleMenuClick()
    {
        if (InputManager.Instance.isBlocked) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null && hit.gameObject == menuButton)
        {
            SoundManager.instance.PlaySound(SoundManager.instance.pauseSound);
            LevelMenu.Instance.ToggleMenu();
        }
    }

    public void UpdateSunAmount(float amount)
    {
        sumAmount += amount;
    }

    void SpawnSun()
    {
        float randomX = Random.Range(-7.0f, 5.0f);
        Vector2 spawnPos = new Vector2(randomX, 6.4f);
        ResourcePool.Instance.GetResource(DropResource.ResourceType.Sun, spawnPos);
    }

    public void EndGame()
    {
        Debug.Log("END GAME TRIGGERED");
        SoundManager.instance.StopMusic();
        ChangeScene.Instance.LoadScene(0);
    }
    public void IncrementZombiesKilled()
    {
        zombiesKilledCount++;
    }
    void OnDestroy()
    {
        Debug.Log("GameManager bị destroy: " + gameObject.name);
    }
}