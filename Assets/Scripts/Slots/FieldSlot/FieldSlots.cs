using UnityEngine;

public class FieldSlots : MonoBehaviour
{
    public GameObject field;
    public GameObject[] slot;
    public int[] slotColumn = new int[5] { 8, 17, 26, 35, 44 }; // x position of each column of slots, used for plant placement and projectile spawning
    void Start()
    {
        int childCount = field.transform.childCount;

        slot = new GameObject[childCount];

        for(int i = 0; i < childCount; i++)
        {
            slot[i] = field.transform.GetChild(i).gameObject;
        }
    }
}
