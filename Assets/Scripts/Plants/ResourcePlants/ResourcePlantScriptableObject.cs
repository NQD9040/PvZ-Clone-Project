using UnityEngine;

[CreateAssetMenu(menuName = "Game/Plant/Resource")]
public class ResourcePlantData : PlantData
{
    public float produceRate = 10f; // how often the plant produces resources
    public float doubleProduceChance = 20f; // chance to produce double resources
}