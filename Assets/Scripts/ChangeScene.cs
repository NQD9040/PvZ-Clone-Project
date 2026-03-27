using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public static ChangeScene Instance;
    void Awake()
    {
        Instance = this;

    }
    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}