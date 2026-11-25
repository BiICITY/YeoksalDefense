using System;
using System.Collections;
using UnityEngine;

public enum EnemyVariant
{
    Basic,
    Fast,
    Tank,
    Boss
}

public class Enemy : MonoBehaviour
{
    private enum EnemyTargetType
    {
        Barricade,
        Wall,
        Altar
    }

    [Header("Variant")]
    [SerializeField] private EnemyVariant enemyVariant = EnemyVariant.Basic;

    [Header("Stats")]
    [SerializeField] private int maxHP = 30;
    [SerializeField] private int currentHP;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackInterval = 1f;

    [Header("Reward")]
    [SerializeField] private int goldReward = 3;
    [SerializeField] private int soulReward = 1;

    [Header("Movement")]
    [SerializeField] private float arriveDistance = 0.15f;
    [SerializeField] private float attackSideOffsetX = 0.9f;

    [Header("Anti Overlap")]
    [SerializeField] private float separationRadius = 0.65f;
    [SerializeField] private float separationStrength = 1.6f;

    [Header("Targets")]
    [SerializeField] private DefenseStructure barricadeTarget;
    [SerializeField] private DefenseStructure wallTarget;
    [SerializeField] private Altar altarTarget;

    [Header("Variant Visual")]
    [SerializeField] private bool useVariantTint = true;
    [SerializeField] private Color basicColor = Color.white;
    [SerializeField] private Color fastColor = Color.cyan;
    [SerializeField] private Color tankColor = new Color(1f, 0.45f, 0.15f);
    [SerializeField] private float basicScale = 1f;
    [SerializeField] private float fastScale = 0.85f;
    [SerializeField] private float tankScale = 1.35f;

    [Header("Hit Feedback")]
    [SerializeField] private bool useHitFlash = true;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Debug")]
    [SerializeField] private EnemyTargetType currentTargetType = EnemyTargetType.Barricade;
    [SerializeField] private bool isDead = false;
    [SerializeField] private float assignedLaneZ = 0f;

    private float attackTimer = 0f;
    private float slowTimer = 0f;
    private float slowMultiplier = 1f;

    private Renderer[] cachedRenderers;
    private Vector3 baseLocalScale;
    private Color currentVisualColor = Color.white;
    private Coroutine hitFlashRoutine;

    public event Action<Enemy> OnDied;

    public bool IsDead => isDead;
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public int GoldReward => goldReward;
    public int SoulReward => soulReward;
    public EnemyVariant Variant => enemyVariant;

    private void Awake()
    {
        currentHP = maxHP;

        baseLocalScale = transform.localScale;
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
    }

    public void Initialize(
        DefenseStructure barricade,
        DefenseStructure wall,
        Altar altar,
        int hp,
        float speed,
        int damage,
        float laneZ,
        int rewardGold,
        int rewardSoul,
        EnemyVariant variant
    )
    {
        barricadeTarget = barricade;
        wallTarget = wall;
        altarTarget = altar;

        enemyVariant = variant;

        maxHP = hp;
        currentHP = maxHP;
        moveSpeed = speed;
        attackDamage = damage;
        assignedLaneZ = laneZ;

        goldReward = rewardGold;
        soulReward = rewardSoul;

        currentTargetType = EnemyTargetType.Barricade;
        isDead = false;
        attackTimer = 0f;
        slowTimer = 0f;
        slowMultiplier = 1f;

        ApplyVariantVisual();
    }

    private void ApplyVariantVisual()
    {
        float scaleMultiplier = GetVariantScale();
        transform.localScale = baseLocalScale * scaleMultiplier;

        currentVisualColor = GetVariantColor();

        ApplyBaseVisualColor();
    }

    private float GetVariantScale()
    {
        switch (enemyVariant)
        {
            case EnemyVariant.Fast:
                return fastScale;
            case EnemyVariant.Tank:
                return tankScale;
            case EnemyVariant.Basic:
            default:
                return basicScale;
        }
    }

    private Color GetVariantColor()
    {
        switch (enemyVariant)
        {
            case EnemyVariant.Fast:
                return fastColor;
            case EnemyVariant.Tank:
                return tankColor;
            case EnemyVariant.Basic:
            default:
                return basicColor;
        }
    }

    private void ApplyBaseVisualColor()
    {
        if (useVariantTint)
        {
            ApplyRenderColor(currentVisualColor);
        }
        else
        {
            ClearRenderColor();
        }
    }

    private void ApplyRenderColor(Color color)
    {
        if (cachedRenderers == null)
        {
            return;
        }

        foreach (Renderer renderer in cachedRenderers)
        {
            if (renderer == null) continue;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);

            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);

            renderer.SetPropertyBlock(block);
        }
    }

    private void ClearRenderColor()
    {
        if (cachedRenderers == null)
        {
            return;
        }

        foreach (Renderer renderer in cachedRenderers)
        {
            if (renderer == null) continue;

            renderer.SetPropertyBlock(null);
        }
    }

    private void Update()
    {
        if (isDead) return;

        UpdateSlow();
        UpdateTargetState();

        Vector3 targetPoint = GetCurrentAttackPoint();

        if (targetPoint == Vector3.zero)
        {
            return;
        }

        float distance = Vector3.Distance(
            GetFlatPosition(transform.position),
            GetFlatPosition(targetPoint)
        );

        if (distance > arriveDistance)
        {
            MoveToPoint(targetPoint);
        }
        else
        {
            AttackCurrentTarget();
        }

        ApplySeparation();
    }

    private void UpdateSlow()
    {
        if (slowTimer <= 0f)
        {
            slowMultiplier = 1f;
            return;
        }

        slowTimer -= Time.deltaTime;

        if (slowTimer <= 0f)
        {
            slowMultiplier = 1f;
        }
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer = Mathf.Max(slowTimer, duration);

        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowStatus("SLOW", transform.position);
        }
    }

    private void UpdateTargetState()
    {
        if (currentTargetType == EnemyTargetType.Barricade)
        {
            if (barricadeTarget == null || barricadeTarget.IsDestroyed)
            {
                currentTargetType = EnemyTargetType.Wall;
                attackTimer = 0f;
            }
        }

        if (currentTargetType == EnemyTargetType.Wall)
        {
            if (wallTarget == null || wallTarget.IsDestroyed)
            {
                currentTargetType = EnemyTargetType.Altar;
                attackTimer = 0f;
            }
        }
    }

    private Vector3 GetCurrentAttackPoint()
    {
        Transform targetTransform = GetCurrentTargetTransform();

        if (targetTransform == null)
        {
            return Vector3.zero;
        }

        Vector3 point = targetTransform.position;
        point.x += attackSideOffsetX;
        point.y = transform.position.y;
        point.z = assignedLaneZ;

        return point;
    }

    private Transform GetCurrentTargetTransform()
    {
        switch (currentTargetType)
        {
            case EnemyTargetType.Barricade:
                return barricadeTarget != null ? barricadeTarget.transform : null;
            case EnemyTargetType.Wall:
                return wallTarget != null ? wallTarget.transform : null;
            case EnemyTargetType.Altar:
                return altarTarget != null ? altarTarget.transform : null;
            default:
                return null;
        }
    }

    private void MoveToPoint(Vector3 targetPoint)
    {
        Vector3 targetPosition = targetPoint;
        targetPosition.y = transform.position.y;

        Vector3 direction = (targetPosition - transform.position).normalized;
        float finalSpeed = moveSpeed * slowMultiplier;

        transform.position += direction * finalSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void ApplySeparation()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, separationRadius);

        Vector3 push = Vector3.zero;
        int count = 0;

        foreach (Collider col in nearbyColliders)
        {
            if (col == null) continue;

            Enemy otherEnemy = col.GetComponentInParent<Enemy>();

            if (otherEnemy == null) continue;
            if (otherEnemy == this) continue;
            if (otherEnemy.IsDead) continue;

            Vector3 away = transform.position - otherEnemy.transform.position;
            away.y = 0f;

            float distance = away.magnitude;

            if (distance <= 0.001f)
            {
                away = UnityEngine.Random.insideUnitSphere;
                away.y = 0f;
                distance = 0.1f;
            }

            float weight = 1f - Mathf.Clamp01(distance / separationRadius);
            push += away.normalized * weight;
            count++;
        }

        if (count > 0)
        {
            Vector3 move = push.normalized * separationStrength * Time.deltaTime;
            move.y = 0f;

            transform.position += move;
        }
    }

    private void AttackCurrentTarget()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
        {
            return;
        }

        attackTimer = 0f;

        switch (currentTargetType)
        {
            case EnemyTargetType.Barricade:
                if (barricadeTarget != null && !barricadeTarget.IsDestroyed)
                {
                    barricadeTarget.TakeDamage(attackDamage);
                    Debug.Log($"{name} attacked Barricade");
                }
                break;

            case EnemyTargetType.Wall:
                if (wallTarget != null && !wallTarget.IsDestroyed)
                {
                    wallTarget.TakeDamage(attackDamage);
                    Debug.Log($"{name} attacked Wall");
                }
                break;

            case EnemyTargetType.Altar:
                if (altarTarget != null && !altarTarget.IsDestroyed)
                {
                    altarTarget.TakeDamage(attackDamage);
                    Debug.Log($"{name} attacked Altar");
                }
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamage(damage, transform.position);
        }

        PlayHitFlash();

        Debug.Log($"{name} HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void PlayHitFlash()
    {
        if (!useHitFlash)
        {
            return;
        }

        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
        }

        hitFlashRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        ApplyRenderColor(hitFlashColor);

        yield return new WaitForSeconds(hitFlashDuration);

        ApplyBaseVisualColor();

        hitFlashRoutine = null;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log($"{name} died");

        OnDied?.Invoke(this);

        Destroy(gameObject);
    }

    private Vector3 GetFlatPosition(Vector3 position)
    {
        return new Vector3(position.x, 0f, position.z);
    }
}