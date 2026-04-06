using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private string playerName;
    public string PlayerName { get { return playerName; } set { playerName = value; } }
    private float score;
    public float Score { get { return score; } set { score = value; } }
    private int progressLevel; // for example, 1.1 is 1, 1.2 is 2, 2.1 is 11, 2.2 is 12, etc.
    public int ProgressLevel { get { return progressLevel; } set { progressLevel = value; } }
    public void IncreaseLevel()
    {
        progressLevel++;
        Debug.Log($"[Player] {playerName} đã lên cấp! Cấp hiện tại: {progressLevel}");
    }
    public void SavePlayerData(List<Player> allPlayers)
    {
        PlayerDataService dataService = new PlayerDataService();
        dataService.Save(allPlayers, this.playerName);
        Debug.Log($"[Player] Đã lưu dữ liệu cho {playerName}");
    }
}
