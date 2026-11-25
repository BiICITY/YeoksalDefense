using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float hitDistance = 0.25f;
    [SerializeField] private float lifeTime = 3f;

    private Enemy target;
    private int damage;
    private float timer;

    public void Initialize(Enemy targetEnemy, int finalDamage)
    {
        target = targetEnemy;
        damage = finalDamage;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position;
        targetPosition.y += 0.5f;

        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= hitDistance)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}