using UnityEngine;

[CreateAssetMenu(menuName = "Game/Plant/Shooter")]
public class ShooterPlantData : PlantData
{
    public float dmgDealt;
    public float fireRate;
    public float fireRange;
    public Vector2 shootPoint;
    public GameObject projectilePrefab;
}