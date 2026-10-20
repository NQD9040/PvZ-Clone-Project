using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Uses the PlantSelection grid already placed in LevelCanvas.</summary>
public class PlantSelectionController : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField, Range(1, 9)] private int maxSelectedPlants = 9;
    [SerializeField] private RectTransform selectionPanel;
    [SerializeField] private GridLayoutGroup selectionGrid;
    [SerializeField] private Canvas hostCanvas;
    [SerializeField] private Transform frontyard;
    [SerializeField, Min(0.1f)] private float transitionDuration = 0.65f;

    private readonly List<GameObject> availableCards = new List<GameObject>();
    private readonly List<GameObject> selectedCards = new List<GameObject>();
    private readonly Dictionary<GameObject, GameObject> cardInstances = new Dictionary<GameObject, GameObject>();
    private readonly Dictionary<GameObject, GameObject> selectedSlotInstances = new Dictionary<GameObject, GameObject>();
    private readonly Dictionary<GameObject, Button> cardButtons = new Dictionary<GameObject, Button>();

    private LevelData levelData;
    private PlantSlots gameplaySlots;
    private Button confirmButton;
    private TMP_Text confirmLabel;
    private Vector3 sampleScale = Vector3.one;
    private Vector2 panelStartPosition;
    private bool isOpen;
    private bool transitionStarted;
    private bool confirmListenerBound;

    public IReadOnlyList<GameObject> AvailableCards => availableCards;
    public IReadOnlyList<GameObject> SelectedCards => selectedCards;
    public int SelectionLimit => Mathf.Clamp(maxSelectedPlants, 1, 9);
    public bool CanSelectMore => selectedCards.Count < SelectionLimit;
    public bool IsOpen => isOpen;
    public bool IsSelectionCard(Transform cardTransform) =>
        selectionGrid != null && cardTransform != null && cardTransform.parent == selectionGrid.transform;

    private void Awake()
    {
        if (hostCanvas == null) hostCanvas = GetComponentInParent<Canvas>();
        if (selectionPanel == null)
        {
            Transform panel = transform.Find("PlantSelection");
            if (panel == null && hostCanvas != null)
                panel = hostCanvas.transform.Find("PlantSelection");
            if (panel != null) selectionPanel = panel.GetComponent<RectTransform>();
        }
        if (selectionGrid == null && selectionPanel != null)
            selectionGrid = selectionPanel.GetComponent<GridLayoutGroup>();
        if (frontyard == null)
        {
            GameObject yard = GameObject.Find("Frontyard");
            if (yard != null) frontyard = yard.transform;
        }

        if (selectionPanel != null)
        {
            panelStartPosition = selectionPanel.anchoredPosition;
            isOpen = true;
            ClearSampleChildren();
        }
        if (frontyard != null)
        {
            Vector3 position = frontyard.position;
            position.x = -3f;
            frontyard.position = position;
        }
        if (InputManager.Instance != null) InputManager.Instance.isBlocked = true;
    }

    private IEnumerator Start()
    {
        // GameManager sets currentLevel and activates the canvas in Start.
        yield return null;

        PlantSelectionSession.Clear();
        gameplaySlots = FindAnyObjectByType<PlantSlots>();
        if (hostCanvas == null || selectionPanel == null || selectionGrid == null || gameplaySlots == null || LevelManager.Instance == null)
        {
            Debug.LogError("Plant selection needs the existing PlantSelection grid, PlantSlots, and LevelManager. Starting with the full roster.", this);
            StartWithFullRoster();
            yield break;
        }

        levelData = LevelManager.Instance.LoadLevel(LevelManager.Instance.currentLevel);
        if (levelData == null || levelData.plantCards == null || levelData.plantCards.Count == 0)
        {
            Debug.LogError("The current level has no plant cards. Starting with the full roster.", this);
            StartWithFullRoster();
            yield break;
        }

        availableCards.Clear();
        foreach (GameObject card in levelData.plantCards)
            if (card != null && !availableCards.Contains(card)) availableCards.Add(card);

        if (availableCards.Count == 0)
        {
            StartWithFullRoster();
            yield break;
        }

        DeactivateGameplayTools();
        PopulateSelectionGrid();
        EnsureConfirmButton();
        RefreshSelectionDisplay();
    }

    public bool IsSelected(GameObject cardPrefab) => cardPrefab != null && selectedCards.Contains(cardPrefab);

    public bool ToggleCard(GameObject cardPrefab)
    {
        if (!isOpen || transitionStarted || cardPrefab == null || !availableCards.Contains(cardPrefab)) return false;

        if (selectedCards.Remove(cardPrefab))
        {
            RemoveCardFromGameplaySlots(cardPrefab);
        }
        else
        {
            if (!CanSelectMore) return false;
            selectedCards.Add(cardPrefab);
            MoveCardToGameplaySlots(cardPrefab);
        }

        RefreshSelectionDisplay();
        return true;
    }

    public void OnConfirmButtonClicked() => ConfirmSelection();

    public bool ConfirmSelection()
    {
        if (!isOpen || transitionStarted) return false;
        if (selectedCards.Count == 0)
        {
            Debug.LogWarning("Choose at least one plant before starting the level.", this);
            return false;
        }

        transitionStarted = true;
        if (confirmButton != null) confirmButton.interactable = false;

        foreach (GameObject cardPrefab in availableCards)
        {
            if (!cardInstances.TryGetValue(cardPrefab, out GameObject cardInstance) || cardInstance == null) continue;
            Button button = cardButtons.TryGetValue(cardPrefab, out Button foundButton) ? foundButton : null;
            if (button != null) button.interactable = false;
        }

        PlantSelectionSession.SetSelection(levelData.levelNumber, selectedCards);
        StartCoroutine(FinishSelectionTransition());
        return true;
    }

    private void PopulateSelectionGrid()
    {
        cardInstances.Clear();
        cardButtons.Clear();
        foreach (GameObject cardPrefab in availableCards)
        {
            GameObject instance = Instantiate(cardPrefab, selectionGrid.transform, false);
            instance.name = cardPrefab.name;
            RectTransform rect = instance.GetComponent<RectTransform>();
            if (rect != null) rect.localScale = sampleScale;
            PlantCard plantCard = instance.GetComponent<PlantCard>();
            if (plantCard != null) plantCard.SetCooldownReducerVisible(false);

            Button button = instance.GetComponent<Button>();
            if (button == null) button = instance.AddComponent<Button>();
            Graphic target = instance.GetComponent<Graphic>();
            button.targetGraphic = target;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => ToggleCard(cardPrefab));

            cardInstances[cardPrefab] = instance;
            cardButtons[cardPrefab] = button;
        }
    }

    private void ClearSampleChildren()
    {
        if (selectionGrid == null) return;

        Transform grid = selectionGrid.transform;
        if (grid.childCount > 0)
        {
            RectTransform sample = grid.GetChild(0) as RectTransform;
            if (sample != null) sampleScale = sample.localScale;
        }

        // The authored card copies are layout samples; hide them before destroying so their
        // PlantCard scripts cannot initialize as gameplay cards.
        for (int i = grid.childCount - 1; i >= 0; i--)
        {
            GameObject sample = grid.GetChild(i).gameObject;
            sample.SetActive(false);
            Destroy(sample);
        }
    }

    private void EnsureConfirmButton()
    {
        if (confirmButton != null) return;

        Transform existing = hostCanvas.transform.Find("PlantSelectionStartButton");
        if (existing != null)
        {
            confirmButton = existing.GetComponent<Button>();
            confirmLabel = existing.GetComponentInChildren<TMP_Text>(true);
        }
        else
        {
            GameObject buttonObject = new GameObject("PlantSelectionStartButton",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button),
                typeof(Canvas), typeof(GraphicRaycaster));
            buttonObject.layer = hostCanvas.gameObject.layer;
            buttonObject.transform.SetParent(hostCanvas.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 65f);
            rect.sizeDelta = new Vector2(420f, 128f);

            Canvas buttonCanvas = buttonObject.GetComponent<Canvas>();
            buttonCanvas.overrideSorting = true;
            buttonCanvas.sortingOrder = 40;
            buttonCanvas.worldCamera = hostCanvas.worldCamera;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.22f, 0.55f, 0.16f, 1f);
            confirmButton = buttonObject.GetComponent<Button>();
            confirmButton.targetGraphic = image;
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            confirmListenerBound = true;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.layer = hostCanvas.gameObject.layer;
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(-12f, -8f);
            labelRect.offsetMax = new Vector2(12f, 8f);
            confirmLabel = labelObject.GetComponent<TMP_Text>();
            confirmLabel.text = "START LEVEL";
            confirmLabel.alignment = TextAlignmentOptions.Center;
            confirmLabel.fontSize = 34f;
            TMP_Text referenceText = hostCanvas.GetComponentInChildren<TMP_Text>(true);
            if (referenceText != null) confirmLabel.font = referenceText.font;
            confirmLabel.raycastTarget = false;
        }

        if (confirmButton != null && !confirmListenerBound)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            confirmListenerBound = true;
        }
    }

    private void RefreshSelectionDisplay()
    {
        foreach (GameObject cardPrefab in availableCards)
        {
            if (!cardInstances.TryGetValue(cardPrefab, out GameObject instance) || instance == null) continue;
            PlantCard card = instance.GetComponent<PlantCard>();
            if (card != null) card.SetSelected(selectedCards.Contains(cardPrefab));
        }

        if (confirmLabel != null)
            confirmLabel.text = $"START LEVEL  ({selectedCards.Count}/{SelectionLimit})";

        if (gameplaySlots != null) gameplaySlots.RefreshSlotReferences();
    }

    private void MoveCardToGameplaySlots(GameObject cardPrefab)
    {
        if (gameplaySlots == null || selectedSlotInstances.ContainsKey(cardPrefab)) return;

        GameObject slotCard = Instantiate(cardPrefab, gameplaySlots.transform, false);
        PlantCard plantCard = slotCard.GetComponent<PlantCard>();
        if (plantCard != null)
        {
            // Cards selected before the level begins start ready, without a pregame cooldown.
            plantCard.isStartCooldown = false;
            plantCard.SetCooldownReducerVisible(false);
        }
        slotCard.transform.SetAsLastSibling();
        selectedSlotInstances.Add(cardPrefab, slotCard);
    }

    private void RemoveCardFromGameplaySlots(GameObject cardPrefab)
    {
        if (!selectedSlotInstances.TryGetValue(cardPrefab, out GameObject slotCard)) return;
        if (slotCard != null)
        {
            slotCard.SetActive(false);
            slotCard.transform.SetParent(null);
            Destroy(slotCard);
        }
        selectedSlotInstances.Remove(cardPrefab);
    }

    private IEnumerator FinishSelectionTransition()
    {
        DeactivateGameplayTools();

        Vector3 yardStart = frontyard != null ? frontyard.position : Vector3.zero;
        Vector3 yardEnd = yardStart;
        yardEnd.x = 1f;
        Vector2 panelEndPosition = panelStartPosition + Vector2.down * 1400f;
        Vector2 buttonStart = confirmButton != null ? ((RectTransform)confirmButton.transform).anchoredPosition : Vector2.zero;
        Color buttonStartColor = confirmButton != null ? confirmButton.GetComponent<Image>().color : Color.clear;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
            if (frontyard != null) frontyard.position = Vector3.Lerp(yardStart, yardEnd, t);
            selectionPanel.anchoredPosition = Vector2.Lerp(panelStartPosition, panelEndPosition, t);
            if (confirmButton != null)
            {
                ((RectTransform)confirmButton.transform).anchoredPosition = buttonStart + Vector2.down * 150f * t;
                Image image = confirmButton.GetComponent<Image>();
                Color color = buttonStartColor;
                color.a = Mathf.Lerp(buttonStartColor.a, 0f, t);
                image.color = color;
                if (confirmLabel != null)
                {
                    Color labelColor = confirmLabel.color;
                    labelColor.a = Mathf.Lerp(1f, 0f, t);
                    confirmLabel.color = labelColor;
                }
            }
            yield return null;
        }

        if (frontyard != null) frontyard.position = yardEnd;
        selectionPanel.gameObject.SetActive(false);
        foreach (GameObject cardInstance in cardInstances.Values)
            if (cardInstance != null) Destroy(cardInstance);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        isOpen = false;

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null) gameManager.BeginGameplay();
        if (InputManager.Instance != null) InputManager.Instance.isBlocked = false;
        WaveManager waveManager = FindAnyObjectByType<WaveManager>();
        if (waveManager != null) waveManager.BeginGameplay();
        else Debug.LogError("WaveManager is missing; gameplay cannot start waves.", this);
    }

    private void DeactivateGameplayTools()
    {
        if (gameplaySlots == null) gameplaySlots = FindAnyObjectByType<PlantSlots>();
        if (gameplaySlots != null) gameplaySlots.DisableFollowImage();
        Shovel shovel = FindAnyObjectByType<Shovel>();
        if (shovel != null) shovel.DeactivateShovel();
    }

    private void StartWithFullRoster()
    {
        gameplaySlots = FindAnyObjectByType<PlantSlots>();
        if (LevelManager.Instance != null)
        {
            levelData = LevelManager.Instance.LoadLevel(LevelManager.Instance.currentLevel);
            if (levelData != null && levelData.plantCards != null)
            {
                PlantSelectionSession.SetSelection(levelData.levelNumber, levelData.plantCards);
                if (gameplaySlots != null)
                {
                    foreach (GameObject card in levelData.plantCards)
                        if (card != null) Instantiate(card, gameplaySlots.transform, false);
                }
            }
        }

        if (frontyard != null)
        {
            Vector3 position = frontyard.position;
            position.x = 1f;
            frontyard.position = position;
        }
        if (InputManager.Instance != null) InputManager.Instance.isBlocked = false;
        isOpen = false;
        if (selectionPanel != null) selectionPanel.gameObject.SetActive(false);
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null) gameManager.BeginGameplay();
        WaveManager waveManager = FindAnyObjectByType<WaveManager>();
        if (waveManager != null) waveManager.BeginGameplay();
    }
}
