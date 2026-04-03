using UnityEngine;

[CreateAssetMenu(fileName = "ZombieData", menuName = "Game/Zombie Data")]
public class ZombieData : ScriptableObject
{
    public float maxHealth;
    public float shieldHealth = 0; // used for conehead and buckethead
    public float dmgDealt;
    public float dmgRate;
    public float moveSpeed;
    public enum shieldType
    {
        None,
        Conehead,
        Buckethead
    }
    public shieldType shield;
}