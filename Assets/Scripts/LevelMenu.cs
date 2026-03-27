using UnityEngine;
using UnityEngine.UI;
public class LevelMenu : MonoBehaviour
{
    public static LevelMenu Instance;
    public Canvas menuCanvas;
    public GameObject backToGameButton;
    public GameObject menuButton;
    public GameObject leaveBarImage;
    private GameObject leaveHitbox;
    private GameObject cancelHitbox;
    void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cancelHitbox = leaveBarImage.transform.GetChild(1).gameObject;
        leaveHitbox = leaveBarImage.transform.GetChild(0).gameObject;
        if (leaveHitbox == null)
        {
            Debug.LogError("Leave hitbox not found!");
        
        }
        if (cancelHitbox == null)
        {
            Debug.LogError("Cancel hitbox not found!");
        }
        leaveBarImage.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            Collider2D[] cols = Physics2D.OverlapPointAll(mousePos);

            foreach (Collider2D col in cols)
            {
                if (col == null) continue;

                if (!InputManager.Instance.isLeaving)
                {
                    if (col.gameObject == backToGameButton)
                    {
                        ToggleMenu();
                        return;
                    }
                    else if (col.gameObject == menuButton)
                    {
                        ToogleLeaveBar();
                        return;
                    }
                }
                else
                {
                    if (col.gameObject == leaveHitbox)
                    {
                        ChangeScene.Instance.LoadScene(0);
                        return;
                    }
                    else if (col.gameObject == cancelHitbox)
                    {
                        ToogleLeaveBar();
                        return;
                    }
                }
            }
        }
    }
    public void ToggleMenu()
    {
        Debug.Log("Toggling menu");
        try
        {
            bool isActive = menuCanvas.gameObject.activeSelf;
            InputManager.Instance.isBlocked = true;
            // toggle UI
            menuCanvas.gameObject.SetActive(!isActive);

            if (!isActive)
            {
                Time.timeScale = 0f; // pause
                SoundManager.instance.SetPause(true);
                InputManager.Instance.isBlocked = true;
            }
            else
            {
                Time.timeScale = 1f; // resume
                SoundManager.instance.SetPause(false);
                InputManager.Instance.isBlocked = false;
                if (leaveBarImage.gameObject.activeSelf)
                {
                    ToogleLeaveBar();
                }
            }
        } catch (System.Exception e)
        {
            Debug.LogError("Error toggling menu: " + e.Message);
        }
        
    }
    public void ToogleLeaveBar()
    {
        bool isActive = leaveBarImage.gameObject.activeSelf;
        leaveBarImage.gameObject.SetActive(!isActive);
        InputManager.Instance.isLeaving = !isActive;
    }
}
