using UnityEngine;

public class Archer : MonoBehaviour
{
    [Header("Attack Stats")]
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private float attackRange = 9f;
    [SerializeField] private float attackInterval = 1f;

    [Header("Distance Damage")]
    [SerializeField] private bool ignoreDistancePenalty = false;
    [SerializeField] private float closeRange = 3f;
    [SerializeField] private float midRange = 6f;
    [SerializeField] private float midDamageMultiplier = 0.75f;
    [SerializeField] private float farDamageMultiplier = 0.5f;

    [Header("Projectile")]
    [SerializeField] private ArrowProjectile arrowPrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;

    [Header("Debug")]
    [SerializeField] private Enemy currentTarget;
    [SerializeField] private float attackTimer = 0f;

    public int BaseDamage => baseDamage;
    public float AttackRange => attackRange;
    public float AttackInterval => attackInterval;

    private void Update()
    {
        attackTimer += Time.deltaTime;

        if (currentTarget == null || currentTarget.IsDead || !IsTargetInRange(currentTarget))
        {
            currentTarget = FindTarget();
        }

        if (currentTarget == null)
        {
            return;
        }

        LookAtTarget(currentTarget);

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            Attack(currentTarget);
        }
    }

    private Enemy FindTarget()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy bestTarget = null;
        float bestX = float.MaxValue;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.IsDead) continue;
            if (!IsTargetInRange(enemy)) continue;

            if (enemy.transform.position.x < bestX)
            {
                bestX = enemy.transform.position.x;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    private bool IsTargetInRange(Enemy enemy)
    {
        float distance = Vector3.Distance(
            GetFlatPosition(transform.position),
            GetFlatPosition(enemy.transform.position)
        );

        return distance <= attackRange;
    }

    private void Attack(Enemy target)
    {
        int finalDamage = CalculateDamageByDistance(target);

        if (arrowPrefab != null)
        {
            Vector3 spawnPosition = GetShootPosition();

            ArrowProjectile arrow = Instantiate(
                arrowPrefab,
                spawnPosition,
                Quaternion.identity
            );

            arrow.Initialize(target, finalDamage);
        }
        else
        {
            target.TakeDamage(finalDamage);
        }

        Debug.Log($"{name} attacked {target.name} Damage: {finalDamage}");
    }

    private int CalculateDamageByDistance(Enemy target)
    {
        if (ignoreDistancePenalty)
        {
            return baseDamage;
        }

        float distance = Vector3.Distance(
            GetFlatPosition(transform.position),
            GetFlatPosition(target.transform.position)
        );

        float multiplier;

        if (distance <= closeRange)
        {
            multiplier = 1f;
        }
        else if (distance <= midRange)
        {
            multiplier = midDamageMultiplier;
        }
        else
        {
            multiplier = farDamageMultiplier;
        }

        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
    }

    private Vector3 GetShootPosition()
    {
        if (shootPoint != null)
        {
            return shootPoint.position;
        }

        Vector3 position = transform.position;
        position.y += 0.8f;
        position.x += 0.3f;

        return position;
    }

    private void LookAtTarget(Enemy target)
    {
        if (visualRoot == null)
        {
            return;
        }

        Vector3 direction = target.transform.position - visualRoot.position;
        direction.y = 0f;

        if (direction == Vector3.zero)
        {
            return;
        }

        visualRoot.rotation = Quaternion.LookRotation(direction);
    }

    private Vector3 GetFlatPosition(Vector3 position)
    {
        return new Vector3(position.x, 0f, position.z);
    }

    public void UpgradeDamage(int amount)
    {
        baseDamage += amount;
        Debug.Log($"{name} Archer Damage upgraded. Current Damage: {baseDamage}");
    }

    public void UpgradeRange(float amount)
    {
        attackRange += amount;
        Debug.Log($"{name} Archer Range upgraded. Current Range: {attackRange}");
    }

    public void UpgradeAttackSpeed(float intervalDecrease)
    {
        attackInterval -= intervalDecrease;
        attackInterval = Mathf.Max(0.2f, attackInterval);

        Debug.Log($"{name} Archer Attack Interval upgraded. Current Interval: {attackInterval}");
    }

    public void SetIgnoreDistancePenalty(bool value)
    {
        ignoreDistancePenalty = value;
        Debug.Log($"{name} Ignore Distance Penalty: {ignoreDistancePenalty}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}