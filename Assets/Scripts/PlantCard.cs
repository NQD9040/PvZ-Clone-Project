using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantCard : MonoBehaviour
{
    public GameObject plantPrefab;

    private PlantSlots plantSlots;
    private GameObject selected;
    private GameObject cooldownReducer;
    private Image cooldownImage;
    private Image plantIcon;
    private TextMeshProUGUI plantCost;
    public Sprite plantImg;
    private GameManager gameManager;

    private float cooldownTimer = 0f;
    private float cooldownTime = 0f; // cache cho đỡ GetComponent mỗi frame
    public bool isStartCooldown = true;
    void Start()
    {
        plantSlots = FindAnyObjectByType<PlantSlots>();

        selected = transform.Find("Selected").gameObject;
        cooldownReducer = transform.Find("CooldownReducer").gameObject;
        cooldownImage = cooldownReducer.GetComponent<Image>();

        selected.SetActive(false);
        cooldownReducer.SetActive(false);

        plantIcon = transform.Find("PlantIcon").GetComponent<Image>();
        plantCost = transform.Find("PlantCost").GetComponentInChildren<TextMeshProUGUI>();

        Plant plant = plantPrefab.GetComponent<Plant>();
        cooldownTime = plant.cooldownTime;

        plantIcon.sprite = plantImg;
        plantCost.text = plant.cost.ToString();

        gameManager = FindAnyObjectByType<GameManager>();

        // reset fill ban đầu
        cooldownImage.fillAmount = 0;
        if (isStartCooldown)
        {
            StartCooldown(0.5f);
        }
    }

    void Update()
    {
        // giảm cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        bool isCoolingDown = cooldownTimer > 0;
        bool isEnoughSun = gameManager.sumAmount >= plantPrefab.GetComponent<Plant>().cost;

        // selected overlay
        if (plantSlots.selectedPlant == plantPrefab || !isEnoughSun || isCoolingDown)
            SetSelected(true);
        else
            SetSelected(false);

        // ===== COOLDOWN UI =====
        if (isCoolingDown)
        {
            cooldownReducer.SetActive(true);

            float percent = cooldownTimer / cooldownTime;
            cooldownImage.fillAmount = percent;
        }
        else
        {
            cooldownReducer.SetActive(false);
            cooldownImage.fillAmount = 0;
        }

        // ===== optional: làm mờ icon =====
        plantIcon.color = isCoolingDown 
            ? new Color(1,1,1,0.5f) 
            : Color.white;
    }

    void OnMouseDown()
    {
        Plant plant = plantPrefab.GetComponent<Plant>();

        bool isEnoughSun = gameManager.sumAmount >= plant.cost;
        bool isCoolingDown = cooldownTimer > 0;

        if (!isEnoughSun || isCoolingDown)
        {
            SoundManager.instance.PlaySound(SoundManager.instance.canNotPick);
            return;
        }

        plantSlots.GetImg(plantImg);
        plantSlots.EnableFollowImage();
        plantSlots.GetSelectedPlant(plantPrefab, this.gameObject);
    }
    public void StartCooldown(float amount = 1)
    {
        cooldownTimer = cooldownTime * amount;

        cooldownReducer.SetActive(true);
        cooldownImage.fillAmount = 1;
    }
    public void SetSelected(bool isSelected)
    {
        selected.SetActive(isSelected);
    }
}