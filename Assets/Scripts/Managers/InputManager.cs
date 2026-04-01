using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool isBlocked = false;
    public bool isLeaving = false;
    public bool isShovelActive = false;
    public bool isPlanting = false;
    void Awake()
    {
        Instance = this;
    }
}