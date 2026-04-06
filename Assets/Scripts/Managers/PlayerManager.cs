using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private List<Player> players = new List<Player>();
    private PlayerDataService dataService;

    public TMP_InputField playerNameInputField;
    public Button addPlayerButton;

    public Transform contentParent;
    public GameObject playerItemPrefab;

    public Canvas createPlayerCanvas;
    private Canvas selectPlayerCanvas;

    public Button createPlayerButton;
    public TMP_Text selectedPlayerText;

    private Player selectedPlayer;
    private string selectedPlayerName;
    private Dictionary<Player, GameObject> playerUIMap = new Dictionary<Player, GameObject>();

    void Start()
    {
        dataService = new PlayerDataService();

        selectPlayerCanvas = GetComponent<Canvas>();

        addPlayerButton.onClick.AddListener(AddPlayer);
        createPlayerButton.onClick.AddListener(CreatePlayer);

        LoadPlayers();
    }

    // ================= ADD =================
    void AddPlayer()
    {
        string name = playerNameInputField.text;

        if (string.IsNullOrEmpty(name))
        {
            Debug.Log("Tên trống thì thêm cái gì bro :)");
            return;
        }

        CreatePlayer(name);

        dataService.Save(players, selectedPlayerName);

        playerNameInputField.text = "";
        createPlayerCanvas.gameObject.SetActive(false);
        InputManager.Instance.isCreating = false;
    }

    void CreatePlayer(string name)
    {
        GameObject obj = new GameObject("Player_" + name);
        Player p = obj.AddComponent<Player>();
        p.PlayerName = name;

        players.Add(p);
        CreatePlayerUI(p);
    }

    void LoadPlayers()
    {
        dataService.Load(players, CreatePlayerUI, out selectedPlayerName);

        if (!string.IsNullOrEmpty(selectedPlayerName))
        {
            selectedPlayer = players.Find(p => p.PlayerName == selectedPlayerName);

            if (selectedPlayer != null)
            {
                selectedPlayerText.text = selectedPlayer.PlayerName;
                selectPlayerCanvas.gameObject.SetActive(false);
                InputManager.Instance.isBlocked = false;
            }
        }
    }

    void CreatePlayerUI(Player player)
    {
        if (playerItemPrefab == null)
        {
            Debug.LogError("❌ playerItemPrefab NULL");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("❌ contentParent NULL");
            return;
        }

        GameObject item = Instantiate(playerItemPrefab, contentParent);

        if (item == null)
        {
            Debug.LogError("❌ Instantiate FAIL");
            return;
        }

        PlayerItemUI ui = item.GetComponentInChildren<PlayerItemUI>();

        if (ui == null)
        {
            Debug.LogError("❌ Prefab thiếu PlayerItemUI script");
            return;
        }

        ui.Setup(player, OnSelectPlayer, OnRemovePlayer);
        item.SetActive(true);
        playerUIMap[player] = item;
    }

    void OnSelectPlayer(Player player)
    {
        selectedPlayer = player;
        selectedPlayerName = player.PlayerName;

        selectedPlayerText.text = player.PlayerName;

        dataService.Save(players, selectedPlayerName);

        selectPlayerCanvas.gameObject.SetActive(false);
        InputManager.Instance.isBlocked = false;
    }
    void OnRemovePlayer(Player player)
    {
        if (player == selectedPlayer)
        {
            selectedPlayer = null;
            selectedPlayerName = null;
            selectedPlayerText.text = "No player selected";
        }

        players.Remove(player);

        if (playerUIMap.ContainsKey(player))
        {
            Destroy(playerUIMap[player]);
            playerUIMap.Remove(player);
        }

        dataService.Save(players, selectedPlayerName);
        Destroy(player.gameObject);
    }
    void CreatePlayer()
    {
        if (InputManager.Instance.isCreating) return;

        createPlayerCanvas.gameObject.SetActive(true);
        InputManager.Instance.isCreating = true;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            selectPlayerCanvas.gameObject.SetActive(false);
            createPlayerCanvas.gameObject.SetActive(false);
            InputManager.Instance.isCreating = false;
            InputManager.Instance.isBlocked = false;
        }
    }
    public Player getSelectedPlayer()
    {
        return selectedPlayer;
    }
}