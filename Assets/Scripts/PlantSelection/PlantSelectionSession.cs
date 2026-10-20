using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// Holds the confirmed plant-card roster while the game changes scenes.
/// </summary>
public static class PlantSelectionSession
{
    private static readonly List<GameObject> selectedCards = new List<GameObject>();
    private static readonly ReadOnlyCollection<GameObject> readOnlySelectedCards = selectedCards.AsReadOnly();
    private static int selectedLevelNumber;
    private static bool isConfirmed;

    public static bool IsConfirmedFor(int levelNumber)
    {
        return isConfirmed && selectedLevelNumber == levelNumber && selectedCards.Count > 0;
    }

    public static IReadOnlyList<GameObject> GetSelectedCards(int levelNumber)
    {
        return selectedLevelNumber == levelNumber ? readOnlySelectedCards :
            (IReadOnlyList<GameObject>)new GameObject[0];
    }

    public static bool HasSelectionFor(int levelNumber)
    {
        return selectedLevelNumber == levelNumber && selectedCards.Count > 0;
    }

    public static void SetSelection(int levelNumber, IEnumerable<GameObject> cards)
    {
        selectedCards.Clear();
        selectedLevelNumber = levelNumber;
        isConfirmed = false;

        if (cards == null) return;
        foreach (GameObject card in cards)
        {
            if (card != null && !selectedCards.Contains(card))
                selectedCards.Add(card);
        }
        isConfirmed = selectedCards.Count > 0;
    }

    public static void Clear()
    {
        selectedCards.Clear();
        selectedLevelNumber = 0;
        isConfirmed = false;
    }
}
