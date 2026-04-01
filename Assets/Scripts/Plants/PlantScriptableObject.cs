using UnityEngine;

public abstract class PlantData : ScriptableObject
{
    public string plantName;
    public float cost;
    public float maxHealth;
    public float cooldownTime;
    public enum PlantType { ShooterPlant, ResourcePlant, DefensePlant, ExplodePlant, OtherPlant }
    public PlantType plantType;
    
}