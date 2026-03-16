using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float sumAmount = 50f;
    public GameObject canvas;
    public GameObject zombiePrefab;
    private Vector2 sunStartSpawnPoint = new Vector2(-7.0f, 6.4f);
    private Vector2 sunEndSpawnPoint = new Vector2(5.0f, 6.4f);

    public GameObject sunPrefab;
    public TextMeshProUGUI sunText;

    public float sunSpawnDelay = 5f;
    private float sunTimer;
    private int spawnColumn = 0;
    public float zombieSpawnDelay = 5f;
    private float zombieTimer;
    public float[] yColumnSpawn = new float[5] { 2.2f, 0.6f, -1.0f, -2.6f, -4.2f }; // y position of each row of slots, used for zombie spawning
    void Start()
    {
        canvas.gameObject.SetActive(true);
        SpawnSun();
    }

    void Update()
    {
        sunText.text = sumAmount.ToString();

        sunTimer += Time.deltaTime;
        zombieTimer += Time.deltaTime;

        if (sunTimer >= sunSpawnDelay)
        {
            SpawnSun();
            sunTimer = 0f;
        }
        if (zombieTimer >= zombieSpawnDelay)
        {
            SpawnZombie();
            zombieTimer = 0f;
            if (zombieSpawnDelay > 0.5f)
            {
                zombieSpawnDelay -= 0.1f;
            }
        }
    }

    public void UpdateSunAmount(float amount)
    {
        sumAmount += amount;
    }

    void SpawnSun()
    {
        float randomX = Random.Range(sunStartSpawnPoint.x, sunEndSpawnPoint.x);

        Vector2 spawnPos = new Vector2(randomX, sunStartSpawnPoint.y);

        ResourcePool.Instance.GetResource(DropResource.ResourceType.Sun, spawnPos);
    }
    void SpawnZombie()
    {
        spawnColumn = Random.Range(0, 5);
        Vector2 spawnPos = new Vector2(9.6f, yColumnSpawn[spawnColumn]);
        Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
    }
}