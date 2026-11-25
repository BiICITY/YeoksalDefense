using UnityEngine;

public class DefenseStructure : MonoBehaviour
{
    [Header("Structure Info")]
    [SerializeField] private string structureName = "Structure";

    [Header("HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;

    [Header("Break Option")]
    [Tooltip("켜면 파괴될 때 이 오브젝트 전체를 비활성화합니다.")]
    [SerializeField] private bool hideWholeObjectOnBreak = true;

    [Tooltip("비워두면 이 오브젝트 전체를 숨깁니다. 특정 모델 묶음만 숨기고 싶으면 VisualRoot를 연결하세요.")]
    [SerializeField] private GameObject visualRoot;

    [Header("State")]
    [SerializeField] private bool isDestroyed = false;

    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;

    public string StructureName => structureName;
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        CacheComponents();
        ResetStructure();
    }

    private void CacheComponents()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider>(true);
    }

    private void ResetStructure()
    {
        currentHP = maxHP;
        isDestroyed = false;

        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
        }

        if (cachedRenderers != null)
        {
            foreach (Renderer r in cachedRenderers)
            {
                if (r != null)
                {
                    r.enabled = true;
                }
            }
        }

        if (cachedColliders != null)
        {
            foreach (Collider c in cachedColliders)
            {
                if (c != null)
                {
                    c.enabled = true;
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamage(damage, transform.position);
        }

        Debug.Log($"{structureName} HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            BreakStructure();
        }
    }

    private void BreakStructure()
    {
        if (isDestroyed) return;

        isDestroyed = true;
        currentHP = 0;

        Debug.Log($"{structureName} destroyed!");

        // 가장 확실한 방식: 루트 오브젝트 전체 숨김
        if (hideWholeObjectOnBreak && visualRoot == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 특정 모델 묶음만 숨기고 싶을 때
        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }

        // 보조 안전장치
        if (cachedRenderers != null)
        {
            foreach (Renderer r in cachedRenderers)
            {
                if (r != null)
                {
                    r.enabled = false;
                }
            }
        }

        if (cachedColliders != null)
        {
            foreach (Collider c in cachedColliders)
            {
                if (c != null)
                {
                    c.enabled = false;
                }
            }
        }
    }

    public void RepairFull()
    {
        gameObject.SetActive(true);

        CacheComponents();

        currentHP = maxHP;
        isDestroyed = false;

        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
        }

        if (cachedRenderers != null)
        {
            foreach (Renderer r in cachedRenderers)
            {
                if (r != null)
                {
                    r.enabled = true;
                }
            }
        }

        if (cachedColliders != null)
        {
            foreach (Collider c in cachedColliders)
            {
                if (c != null)
                {
                    c.enabled = true;
                }
            }
        }

        Debug.Log($"{structureName} repaired: {currentHP}/{maxHP}");
    }

    public void Repair(int amount)
    {
        if (isDestroyed) return;

        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);

        Debug.Log($"{structureName} repaired: {currentHP}/{maxHP}");
    }

    [ContextMenu("Debug Break Structure")]
    private void DebugBreakStructure()
    {
        BreakStructure();
    }

    [ContextMenu("Debug Repair Full")]
    private void DebugRepairFull()
    {
        RepairFull();
    }
}