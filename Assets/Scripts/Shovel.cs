using UnityEngine;
using UnityEngine.UI;

public class Shovel : MonoBehaviour
{
    public Image shovelFollowImage; // UI đi theo chuột
    public Sprite shovelActiveSprite;
    public Sprite shovelInactiveSprite;

    private bool isShovelActive = false;

    void Start()
    {
        shovelFollowImage.gameObject.SetActive(false);
        GetComponent<SpriteRenderer>().sprite = shovelInactiveSprite;
    }

    void Update()
    {
        if (!isShovelActive) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        shovelFollowImage.transform.position = mousePos;
        if (InputManager.Instance.isBlocked){
            DeactivateShovel();
        }
    }

    void OnMouseDown()
    {
        if (InputManager.Instance.isBlocked)
            return;
        ToggleShovel();
        SoundManager.instance.PlaySound(SoundManager.instance.pickupShovel);
    }

    void ToggleShovel()
    {
        isShovelActive = !isShovelActive;

        // đổi sprite icon shovel
        GetComponent<SpriteRenderer>().sprite = isShovelActive 
            ? shovelActiveSprite 
            : shovelInactiveSprite;

        // bật/tắt image follow chuột
        shovelFollowImage.gameObject.SetActive(isShovelActive);
    }

    public bool IsShovelActive()
    {
        return isShovelActive;
    }
    public void DeactivateShovel()
    {
        isShovelActive = false;
        GetComponent<SpriteRenderer>().sprite = shovelInactiveSprite;
        shovelFollowImage.gameObject.SetActive(false);
    }
}