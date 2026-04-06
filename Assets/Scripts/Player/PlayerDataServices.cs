using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class PlayerDataService
{
    private string filePath;

    public PlayerDataService()
    {
#if !UNITY_WEBGL
        filePath = Path.Combine(Application.persistentDataPath, "PlayerInformation.csv");
#endif
    }

    // ================= SAVE =================
    public void Save(List<Player> players, string selectedPlayerName)
    {
#if UNITY_WEBGL
        SaveToPlayerPrefs(players, selectedPlayerName);
#else
        SaveToCSV(players, selectedPlayerName);
#endif
    }

    // ---------- CSV ----------
    void SaveToCSV(List<Player> players, string selectedPlayerName)
    {
        List<string> lines = new List<string>();

        // dòng selected
        lines.Add("SELECTED," + (selectedPlayerName ?? "null"));

        foreach (var p in players)
        {
            // name,score,level
            lines.Add($"{p.PlayerName},{p.Score},{p.ProgressLevel}");
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    // ---------- WEB ----------
    void SaveToPlayerPrefs(List<Player> players, string selectedPlayerName)
    {
        PlayerListWrapper wrapper = new PlayerListWrapper();

        foreach (var p in players)
        {
            wrapper.players.Add(new PlayerData
            {
                name = p.PlayerName,
                score = p.Score,
                progressLevel = p.ProgressLevel
            });
        }

        string json = JsonUtility.ToJson(wrapper);

        PlayerPrefs.SetString("players", json);
        PlayerPrefs.SetString("selectedPlayer", selectedPlayerName ?? "");
        PlayerPrefs.Save();
    }

    // ================= LOAD =================
    public void Load(List<Player> players, System.Action<Player> onCreateUI,
                     out string selectedPlayerName)
    {
#if UNITY_WEBGL
        LoadFromPlayerPrefs(players, onCreateUI, out selectedPlayerName);
#else
        LoadFromCSV(players, onCreateUI, out selectedPlayerName);
#endif
    }

    // ---------- CSV ----------
    void LoadFromCSV(List<Player> players, System.Action<Player> onCreateUI,
                     out string selectedName)
    {
        selectedName = null;
        players.Clear();

        if (!File.Exists(filePath)) return;

        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // xử lý selected
            if (line.StartsWith("SELECTED,"))
            {
                var parts = line.Split(',');
                if (parts.Length > 1 && parts[1] != "null")
                    selectedName = parts[1];

                continue;
            }

            // parse player
            string[] data = line.Split(',');

            string name = data[0];
            float score = data.Length > 1 ? float.Parse(data[1]) : 0f;
            int level = data.Length > 2 ? int.Parse(data[2]) : 1;

            CreatePlayer(name, score, level, players, onCreateUI);
        }
    }

    // ---------- WEB ----------
    void LoadFromPlayerPrefs(List<Player> players, System.Action<Player> onCreateUI,
                             out string selectedName)
    {
        players.Clear();

        selectedName = PlayerPrefs.GetString("selectedPlayer", "");

        string json = PlayerPrefs.GetString("players", "");

        if (string.IsNullOrEmpty(json)) return;

        PlayerListWrapper wrapper = JsonUtility.FromJson<PlayerListWrapper>(json);

        foreach (var data in wrapper.players)
        {
            CreatePlayer(data.name, data.score, data.progressLevel, players, onCreateUI);
        }
    }

    // ================= HELPER =================
    void CreatePlayer(string name, float score, int level,
                      List<Player> players, System.Action<Player> onCreateUI)
    {
        GameObject obj = new GameObject("Player_" + name);
        Player p = obj.AddComponent<Player>();

        p.PlayerName = name;
        p.Score = score;
        p.ProgressLevel = level;

        players.Add(p);
        onCreateUI?.Invoke(p);
    }
    // Thêm hàm này vào class PlayerDataService
    public Player GetSelectedPlayer(List<Player> players)
    {
        string selectedName = null;

    #if UNITY_WEBGL
        selectedName = PlayerPrefs.GetString("selectedPlayer", "");
    #else
        if (!File.Exists(filePath)) return null;

        using (StreamReader reader = new StreamReader(filePath))
        {
            string firstLine = reader.ReadLine();
            if (firstLine != null && firstLine.StartsWith("SELECTED,"))
            {
                var parts = firstLine.Split(',');
                if (parts.Length > 1 && parts[1] != "null")
                    selectedName = parts[1];
            }
        }
    #endif

        if (string.IsNullOrEmpty(selectedName)) return null;

        // Tìm thằng có tên trùng với selectedName trong list truyền vào
        return players.Find(p => p.PlayerName == selectedName);
    }
}