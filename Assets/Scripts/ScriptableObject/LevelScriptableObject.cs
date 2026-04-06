using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]

public class LevelData : ScriptableObject
{
    public int levelNumber;
    public int maxWaves;
    public List<ZombieCostData> zombieTypes;
    public List<GameObject> plantCards;
}