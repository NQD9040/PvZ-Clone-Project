using UnityEngine;

public class DropResource : MonoBehaviour
{
    public float speed = 2f;
    public Vector2 target = new Vector2(-6.2f, 4.4f);
    public float amount = 25f;

    public GameManager gameManager;

    public enum ResourceType
    {
        Sun,
        BigSun,
        Coin
    }

    public ResourceType resourceType;

    private bool isMovingToTarget = false;
    private float lifeTimer;
    private static bool resourcePickedThisClick = false;

    void OnEnable()
    {
        lifeTimer = 0f;
        isMovingToTarget = false;

        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        // reset lock khi nhả chuột
        if (Input.GetMouseButtonUp(0))
        {
            resourcePickedThisClick = false;
        }

        // timeout sau 10s
        if (lifeTimer >= 10f)
        {
            ResourcePool.Instance.ReturnResource(gameObject);
            return;
        }

        HandleClick();

        if (!isMovingToTarget)
        {
            if (transform.position.y > -4f)
            {
                transform.Translate(Vector2.down * speed * Time.deltaTime);
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                target,
                20f * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, target) < 0.05f)
            {
                if (gameManager != null)
                {
                    gameManager.AddSun(amount);
                }

                ResourcePool.Instance.ReturnResource(gameObject);
            }
        }
    }

    void HandleClick()
    {
        if (isMovingToTarget) return;

        if (resourcePickedThisClick) return;

        if (InputManager.Instance.isBlocked)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject)
                {
                    resourcePickedThisClick = true;

                    SoundManager.instance.PlaySound(SoundManager.instance.sunPickup);

                    isMovingToTarget = true;
                    break;
                }
            }
        }
    }
}