using UnityEngine;

public class Altar : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 1000;
    [SerializeField] private int currentHP;

    [Header("State")]
    [SerializeField] private bool isDestroyed = false;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        ResetAltar();
    }

    public void ResetAltar()
    {
        currentHP = maxHP;
        isDestroyed = false;

        Debug.Log($"Altar reset: {currentHP}/{maxHP}");
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

        Debug.Log($"Altar HP: {currentHP}/{maxHP}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RefreshUI();
        }

        if (currentHP <= 0)
        {
            DestroyAltar();
        }
    }

    private void DestroyAltar()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        Debug.Log("Game Over! The altar has been destroyed.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    public void Heal(int amount)
    {
        if (isDestroyed) return;

        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);

        Debug.Log($"Altar healed: {currentHP}/{maxHP}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RefreshUI();
        }
    }
}