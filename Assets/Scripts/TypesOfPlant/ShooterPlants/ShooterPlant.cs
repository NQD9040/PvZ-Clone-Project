using UnityEngine;

public class ShooterPlant : Plant
{
    public GameObject projectilePrefab;
    public float fireRate = 1f;
    public float fireRange = 12f;

    private float timer;

    public float dmgDealt = 20;

    private Animator animator;

    public Vector2 shootPoint = new Vector2(0.2f, 0.19f);

    public LayerMask zombieLayer;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!HasZombieInRange())
        {
            animator.SetBool("isShoot", false);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            animator.SetBool("isShoot", true);
            timer = 0f;
        }
    }

    bool HasZombieInRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right,
            fireRange,
            zombieLayer
        );

        return hit.collider != null;
    }

    void Shoot()
    {
        Vector2 pos = transform.position;
        pos.x += shootPoint.x;
        pos.y += shootPoint.y;

        GameObject proj = ProjectilePool.Instance.GetProjectile(pos);

        StraightProjectile pea = proj.GetComponent<StraightProjectile>();
        pea.damage = dmgDealt;

        SoundManager.instance.PlaySound(SoundManager.instance.shoot);
    }

    public void EndShoot()
    {
        animator.SetBool("isShoot", false);
    }
}