using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button button;
    [SerializeField] private Button removePlayerButton;
    private Player player;

    public void Setup(Player p, System.Action<Player> onClick, System.Action<Player> onRemove)
    {
        player = p;
        nameText.text = player.PlayerName;
        Debug.Log("Setting up UI for player: " + player.PlayerName);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke(player);
        });
        removePlayerButton.onClick.RemoveAllListeners();
        removePlayerButton.onClick.AddListener(() => {
            onRemove?.Invoke(player);
        });
    }
}