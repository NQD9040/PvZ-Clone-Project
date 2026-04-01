using UnityEngine;

public class ShooterPlant : Plant
{
    [Header("Shooter Plant Data")]
    ShooterPlantData shooterPlantData;
    private float timer;
    private Animator animator;
    public LayerMask zombieLayer;
    void Start()
    {
        animator = GetComponent<Animator>();
        shooterPlantData = (ShooterPlantData)data;
    }

    void Update()
    {
        if (!HasZombieInRange())
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

        GameObject proj = ProjectilePool.Instance.GetProjectile(
            shooterPlantData.projectilePrefab.GetComponent<StraightProjectile>().projectileType.ToString(),
            pos
        );

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