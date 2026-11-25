using System.Collections.Generic;
using UnityEngine;

public class Spearman : MonoBehaviour
{
    [Header("Attack Stats")]
    [SerializeField] private int baseDamage = 8;
    [SerializeField] private float attackInterval = 1.2f;

    [Header("Attack Area")]
    [Tooltip("창병 기준 오른쪽(+X)으로 얼마나 멀리 찌를 수 있는지")]
    [SerializeField] private float forwardRange = 3.0f;

    [Tooltip("창병 기준 뒤쪽(-X)으로도 약간 허용하는 거리. 너무 작으면 가까운 적을 놓칠 수 있음")]
    [SerializeField] private float backwardAllowance = 0.3f;

    [Tooltip("Z축 위아래 공격 폭")]
    [SerializeField] private float halfWidth = 0.8f;

    [Tooltip("한 번 찌르기로 맞출 수 있는 최대 적 수")]
    [SerializeField] private int maxHitCount = 2;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float attackMotionDistance = 0.25f;
    [SerializeField] private float attackMotionSpeed = 12f;

    [Header("Debug")]
    [SerializeField] private Enemy currentTarget;
    [SerializeField] private float attackTimer = 0f;
    [SerializeField] private bool showDebugLog = true;

    private Vector3 visualOriginLocalPosition;
    private float attackMotionTimer = 0f;
    private bool isPlayingAttackMotion = false;

    private void Awake()
    {
        if (visualRoot != null)
        {
            visualOriginLocalPosition = visualRoot.localPosition;
        }
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;

        currentTarget = FindBestTarget();

        if (currentTarget != null)
        {
            LookAtTarget(currentTarget);

            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                Attack();
            }
        }

        UpdateAttackMotion();
    }

    private Enemy FindBestTarget()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy bestTarget = null;
        float bestX = float.MaxValue;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.IsDead) continue;
            if (!IsEnemyInAttackArea(enemy)) continue;

            // 적은 오른쪽에서 왼쪽으로 오므로 X가 작을수록 재단에 가까운 위험한 적
            if (enemy.transform.position.x < bestX)
            {
                bestX = enemy.transform.position.x;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    private bool IsEnemyInAttackArea(Enemy enemy)
    {
        Vector3 localOffset = enemy.transform.position - transform.position;

        float dx = localOffset.x;
        float dz = Mathf.Abs(localOffset.z);

        bool inForwardRange = dx >= -backwardAllowance && dx <= forwardRange;
        bool inWidth = dz <= halfWidth;

        return inForwardRange && inWidth;
    }

    private void Attack()
    {
        List<Enemy> targets = FindTargetsInAttackArea();

        if (targets.Count <= 0)
        {
            return;
        }

        foreach (Enemy target in targets)
        {
            if (target == null || target.IsDead) continue;

            target.TakeDamage(baseDamage);

            if (showDebugLog)
            {
                Debug.Log($"{name} stabbed {target.name} Damage: {baseDamage}");
            }
        }

        PlayAttackMotion();
    }

    private List<Enemy> FindTargetsInAttackArea()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        List<Enemy> candidates = new List<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.IsDead) continue;
            if (!IsEnemyInAttackArea(enemy)) continue;

            candidates.Add(enemy);
        }

        // 재단에 가까운 적부터 공격
        candidates.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        if (candidates.Count > maxHitCount)
        {
            candidates.RemoveRange(maxHitCount, candidates.Count - maxHitCount);
        }

        return candidates;
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

    private void PlayAttackMotion()
    {
        if (visualRoot == null)
        {
            return;
        }

        isPlayingAttackMotion = true;
        attackMotionTimer = 0f;
    }

    private void UpdateAttackMotion()
    {
        if (visualRoot == null || !isPlayingAttackMotion)
        {
            return;
        }

        attackMotionTimer += Time.deltaTime * attackMotionSpeed;

        float t = Mathf.PingPong(attackMotionTimer, 1f);
        Vector3 forwardOffset = visualRoot.forward * attackMotionDistance * t;

        visualRoot.position = transform.position + visualOriginLocalPosition + forwardOffset;

        if (attackMotionTimer >= 2f)
        {
            isPlayingAttackMotion = false;
            visualRoot.localPosition = visualOriginLocalPosition;
        }
    }

    public void UpgradeDamage(int amount)
    {
        baseDamage += amount;
    }

    public void UpgradeRange(float amount)
    {
        forwardRange += amount;
    }

    public void UpgradeMaxHitCount(int amount)
    {
        maxHitCount += amount;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 center = transform.position;
        center.x += (forwardRange - backwardAllowance) * 0.5f;

        Vector3 size = new Vector3(forwardRange + backwardAllowance, 0.1f, halfWidth * 2f);

        Gizmos.DrawWireCube(center, size);
    }
}