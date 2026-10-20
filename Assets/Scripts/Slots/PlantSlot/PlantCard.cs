using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantCard : MonoBehaviour
{
    public GameObject plantPrefab;

    private PlantSlots plantSlots;
    private PlantSelectionController selectionController;
    private GameObject selected;
    private GameObject cooldownReducer;
    private Image cooldownImage;
    private Image plantIcon;
    private TextMeshProUGUI plantCost;
    public Sprite plantImg;
    private GameManager gameManager;
    private Plant plant;
    private bool previousCoolingState;
    private bool hasInitializedCoolingState;

    private float cooldownTimer = 0f;
    private float cooldownTime = 0f; // cache cho đỡ GetComponent mỗi frame
    public bool isStartCooldown = true;
    void Start()
    {
        plantSlots = FindAnyObjectByType<PlantSlots>();
        selectionController = FindAnyObjectByType<PlantSelectionController>();

        selected = transform.Find("Selected").gameObject;
        cooldownReducer = transform.Find("CooldownReducer").gameObject;
        cooldownImage = cooldownReducer.GetComponent<Image>();

        selected.SetActive(false);
        cooldownReducer.SetActive(false);

        plantIcon = transform.Find("PlantIcon").GetComponent<Image>();
        plantCost = transform.Find("PlantCost").GetComponentInChildren<TextMeshProUGUI>();

        plant = plantPrefab != null ? plantPrefab.GetComponent<Plant>() : null;
        if (plant == null)
        {
            Debug.LogError($"PlantCard '{name}' has no Plant prefab/component.", this);
            enabled = false;
            return;
        }
        cooldownTime = plant.data.cooldownTime;

        plantIcon.sprite = plantImg;
        plantCost.text = plant.data.cost.ToString();

        gameManager = FindAnyObjectByType<GameManager>();

        // reset fill ban đầu
        cooldownImage.fillAmount = 0;
        bool isPreGameSelection = selectionController != null && selectionController.IsOpen;
        if (isStartCooldown && !isPreGameSelection)
        {
            StartCooldown(0.5f);
        }
        if (selectionController != null && selectionController.IsSelectionCard(transform))
            SetCooldownReducerVisible(false);
    }

    void Update()
    {
        // Choice cards do not show cooldown or gameplay state in the selection grid.
        if (selectionController != null && selectionController.IsSelectionCard(transform))
        {
            SetCooldownReducerVisible(false);
            return;
        }

        // giảm cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        bool isCoolingDown = cooldownTimer > 0;
        bool isEnoughSun = gameManager != null && gameManager.sunAmount >= plant.data.cost;

        // selected overlay
        if (plantSlots != null && (plantSlots.selectedPlant == plantPrefab || !isEnoughSun || isCoolingDown))
            SetSelected(true);
        else
            SetSelected(false);

        // ===== COOLDOWN UI =====
        if (isCoolingDown)
        {
            if (!cooldownReducer.activeSelf) cooldownReducer.SetActive(true);

            float percent = cooldownTimer / cooldownTime;
            cooldownImage.fillAmount = percent;
        }
        else
        {
            if (cooldownReducer.activeSelf) cooldownReducer.SetActive(false);
            cooldownImage.fillAmount = 0;
        }

        // ===== optional: làm mờ icon =====
        if (!hasInitializedCoolingState || previousCoolingState != isCoolingDown)
        {
            plantIcon.color = isCoolingDown ? new Color(1, 1, 1, 0.5f) : Color.white;
            previousCoolingState = isCoolingDown;
            hasInitializedCoolingState = true;
        }
    }

    void OnMouseDown()
    {
        if (selectionController != null && selectionController.IsSelectionCard(transform)) return;
        if (InputManager.Instance.isBlocked || InputManager.Instance.isShovelActive)
            return;
        if (plant == null || plantSlots == null || gameManager == null) return;

        bool isEnoughSun = gameManager.sunAmount >= plant.data.cost;
        bool isCoolingDown = cooldownTimer > 0;

        if (!isEnoughSun || isCoolingDown)
        {
            SoundManager.instance.PlaySound(SoundManager.instance.canNotPick);
            return;
        }

        plantSlots.GetImg(plantImg);
        plantSlots.EnableFollowImage();
        plantSlots.GetSelectedPlant(plantPrefab, this.gameObject);
        InputManager.Instance.isPlanting = true;
    }
    public void StartCooldown(float amount = 1)
    {
        cooldownTimer = cooldownTime * amount;

        cooldownReducer.SetActive(true);
        cooldownImage.fillAmount = 1;
    }
    public void SetSelected(bool isSelected)
    {
        if (selected != null && selected.activeSelf != isSelected)
            selected.SetActive(isSelected);
    }

    public void SetCooldownReducerVisible(bool visible)
    {
        if (cooldownReducer == null)
        {
            Transform reducer = transform.Find("CooldownReducer");
            if (reducer != null) cooldownReducer = reducer.gameObject;
        }

        if (cooldownReducer != null && cooldownReducer.activeSelf != visible)
            cooldownReducer.SetActive(visible);
    }
}
