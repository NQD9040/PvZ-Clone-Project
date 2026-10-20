using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

public class PlayerDataService
{
    private static readonly char[] CsvSpecialCharacters = { ',', '"', '\r', '\n' };
    private string filePath;

    // Resolve the path on first use. This service is also created by MonoBehaviour
    // field initializers, where Unity APIs such as persistentDataPath are unavailable.
    private string FilePath
    {
        get
        {
#if !UNITY_WEBGL
            if (string.IsNullOrEmpty(filePath))
                filePath = Path.Combine(Application.persistentDataPath, "PlayerInformation.csv");
#endif
            return filePath;
        }
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
        lines.Add("SELECTED," + EscapeCsv(selectedPlayerName ?? "null"));

        foreach (var p in players)
        {
            // name,score,level
            lines.Add($"{EscapeCsv(p.PlayerName)},{p.Score.ToString(CultureInfo.InvariantCulture)},{p.ProgressLevel}");
        }

        File.WriteAllLines(FilePath, lines, Encoding.UTF8);
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

        if (!File.Exists(FilePath)) return;

        string[] lines = File.ReadAllLines(FilePath, Encoding.UTF8);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // xử lý selected
            if (line.StartsWith("SELECTED,"))
            {
                var parts = ParseCsvLine(line);
                if (parts.Length > 1 && parts[1] != "null")
                    selectedName = parts[1];

                continue;
            }

            // parse player
            string[] data = ParseCsvLine(line);
            if (data.Length == 0 || string.IsNullOrWhiteSpace(data[0])) continue;

            string name = data[0];
            float score = data.Length > 1 && float.TryParse(data[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedScore)
                ? parsedScore : 0f;
            int level = data.Length > 2 && int.TryParse(data[2], out int parsedLevel)
                ? parsedLevel : 1;

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
        if (wrapper == null || wrapper.players == null) return;

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
        if (!File.Exists(FilePath)) return null;

        using (StreamReader reader = new StreamReader(FilePath))
        {
            string firstLine = reader.ReadLine();
            if (firstLine != null && firstLine.StartsWith("SELECTED,"))
            {
                var parts = ParseCsvLine(firstLine);
                if (parts.Length > 1 && parts[1] != "null")
                    selectedName = parts[1];
            }
        }
    #endif

        if (string.IsNullOrEmpty(selectedName)) return null;

        // Tìm thằng có tên trùng với selectedName trong list truyền vào
        return players.Find(p => p.PlayerName == selectedName);
    }

    private static string EscapeCsv(string value)
    {
        if (value.IndexOfAny(CsvSpecialCharacters) < 0) return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static string[] ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
        StringBuilder field = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char current = line[i];
            if (current == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else inQuotes = !inQuotes;
            }
            else if (current == ',' && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else field.Append(current);
        }

        fields.Add(field.ToString());
        return fields.ToArray();
    }
}
