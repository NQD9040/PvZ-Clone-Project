using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject startButton;
    public GameObject moreButton;
    public GameObject quitButton;
    public GameObject changePlayerButton;
    public Canvas playerCanvas;
    private GameObject startHover;
    private GameObject moreHover;
    private GameObject quitHover;
    void Start()
    {
        SoundManager.instance.PlayMusic(SoundManager.instance.menuMusic);
        startHover = startButton.transform.GetChild(0).gameObject;
        moreHover = moreButton.transform.GetChild(0).gameObject;
        quitHover = quitButton.transform.GetChild(0).gameObject;
    }

    void Update()
    {
        HoverCheck();
        StartCheck();
    }

    void HoverCheck()
    {
        if (InputManager.Instance.isBlocked) return;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Collider2D col = Physics2D.OverlapPoint(worldPos);

        if (col != null)
        {
            if (col.gameObject == startButton)
            {
                startHover.SetActive(true);                
                moreHover.SetActive(false);
                quitHover.SetActive(false);
            }
            else if (col.gameObject == moreButton)
            {
                moreHover.SetActive(true);
                startHover.SetActive(false);
                quitHover.SetActive(false);
            }  
            else if (col.gameObject == quitButton)
            {
                quitHover.SetActive(true);
                startHover.SetActive(false);
                moreHover.SetActive(false);
            }
        }
        else
        {
            startHover.SetActive(false);
            moreHover.SetActive(false);
            quitHover.SetActive(false);
        }
    }

    void StartCheck(int sceneId = 1)
    {
        if (InputManager.Instance.isBlocked) return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Collider2D col = Physics2D.OverlapPoint(worldPos);

            if (col != null)
            {
                if (col.gameObject == startButton)
                {
                    ChangeScene.Instance.LoadScene(sceneId);
                }
                else if (col.gameObject == quitButton)
                {
                    ExitGame();
                }
                else if (col.gameObject == changePlayerButton)
                {
                    playerCanvas.gameObject.SetActive(true);
                    InputManager.Instance.isBlocked = true;
                }
            }
        }
    }
    void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}