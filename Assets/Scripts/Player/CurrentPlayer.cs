using System.Collections.Generic;
using UnityEngine;

public class CurrentPlayer : MonoBehaviour
{
    public static CurrentPlayer Instance;

    public Player player;

    private PlayerDataService dataService = new PlayerDataService();
    private List<Player> tempPlayers = new List<Player>();

    void Awake()
    {
        Instance = this;
        LoadCurrentPlayer();
    }

    public void LoadCurrentPlayer()
    {
        string selectedName;
        dataService.Load(tempPlayers, null, out selectedName);

        if (!string.IsNullOrEmpty(selectedName))
        {
            player = tempPlayers.Find(p => p.PlayerName == selectedName);
        }

        if (player != null)
        {
            player.transform.SetParent(this.transform);
        }
    }
    public void SaveCurrentPlayer()
    {
        if (player != null)
        {
            player.SavePlayerData(tempPlayers);
        }
    }
}