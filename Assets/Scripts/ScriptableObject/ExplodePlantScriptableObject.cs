using UnityEngine;

[CreateAssetMenu(menuName = "Game/Plant/Explode")]
public class ExplodePlantData : PlantData
{
    public float radius = 3f;
    public bool isActive; // instantly explode or taking some condition to explode like potato mine
    public float dmgDealt = 2500f; // default damage, can be changed
    public float armTime = 20f; // only for potato mine, after arm time, it will be active and explode when zombie collide with it
}