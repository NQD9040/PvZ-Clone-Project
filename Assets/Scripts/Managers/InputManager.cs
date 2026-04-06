using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool isBlocked = false;
    public bool isCreating = false;
    public bool isLeaving = false;
    public bool isShovelActive = false;
    public bool isPlanting = false;
    public bool isComplete = false;
    void Awake()
    {
        Instance = this;
    }
}