using UnityEngine;

public class ShooterPlant : Plant
{
    [Header("Shooter Plant Data")]
    ShooterPlantData shooterPlantData;
    private float timer;
    private Animator animator;
    private string projectilePoolKey;
    public LayerMask zombieLayer;
    void Start()
    {
        animator = GetComponent<Animator>();
        shooterPlantData = (ShooterPlantData)data;
        StraightProjectile projectile = shooterPlantData.projectilePrefab != null
            ? shooterPlantData.projectilePrefab.GetComponent<StraightProjectile>() : null;
        if (projectile == null)
        {
            Debug.LogError($"Projectile prefab is missing StraightProjectile on {name}.", this);
            enabled = false;
            return;
        }
        projectilePoolKey = projectile.projectileType.ToString();
    }

    void Update()
    {
        if (shooterPlantData == null || !HasZombieInRange())
        {
            animator.SetBool("isShoot", false);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= shooterPlantData.fireRate)
        {
            animator.SetBool("isShoot", true);
        }
    }

    bool HasZombieInRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right,
            shooterPlantData.fireRange,
            zombieLayer
        );

        return hit.collider != null;
    }

    void Shoot()
    {
        Vector2 pos = transform.position;
        pos.x += shooterPlantData.shootPoint.x;
        pos.y += shooterPlantData.shootPoint.y;

        if (ProjectilePool.Instance == null) return;
        GameObject proj = ProjectilePool.Instance.GetProjectile(projectilePoolKey, pos);
        if (proj == null) return;

        StraightProjectile pea = proj.GetComponent<StraightProjectile>();
        pea.damage = shooterPlantData.dmgDealt;

        SoundManager.instance.PlaySound(SoundManager.instance.shoot);
        timer = 0f;
    }

    public void EndShoot()
    {
        animator.SetBool("isShoot", false);
    }
    void SnowPeaShoot()
    {
        SoundManager.instance.PlaySound(SoundManager.instance.snowEffect);
    }
}
