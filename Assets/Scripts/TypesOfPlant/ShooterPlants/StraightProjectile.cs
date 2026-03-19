using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 20f;
    public float lifeTime = 5f;
    private bool hasHit = false;
    private float lifeTimer;
    public enum ProjectileType
    {
        Pea,
        SnowPea,
        FirePea,
        CastusBullet,
        puffShroomBullet // use for puff-shroom and scaredy-shroom
    }
    public ProjectileType projectileType;
    void Start()
    {
        // Start
    }

    void OnEnable()
    {
        lifeTimer = 0f;
        hasHit = false;
    }

    void Update()
    {
        if (!hasHit)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifeTime)
        {
            ProjectilePool.Instance.ReturnResource(gameObject, projectileType.ToString());
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        if (collision.CompareTag("Zombie"))
        {
            hasHit = true;
            Zombie zombie = collision.GetComponent<Zombie>();

            if (projectileType == ProjectileType.SnowPea)
            {
                zombie.TakeDamage(damage, true); // Apply damage and slow effect
            }
            else
            {
                zombie.TakeDamage(damage);
            }

            SoundManager.instance.PlaySound(SoundManager.instance.normalHit);

            ProjectilePool.Instance.ReturnResource(gameObject, projectileType.ToString());
        }
    }
}