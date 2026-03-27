using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool isBlocked = false;
    public bool isLeaving = false;
    void Awake()
    {
        Instance = this;
    }
}