using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("Prefab")]
    [SerializeField] private DamagePopup popupPrefab;

    [Header("Position")]
    [SerializeField] private float yOffset = 1.3f;

    [Header("Colors")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color statusColor = Color.cyan;
    [SerializeField] private Color healColor = Color.green;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowDamage(int damage, Vector3 worldPosition)
    {
        ShowText($"-{damage}", worldPosition, damageColor);
    }

    public void ShowStatus(string message, Vector3 worldPosition)
    {
        ShowText(message, worldPosition, statusColor);
    }

    public void ShowHeal(int amount, Vector3 worldPosition)
    {
        ShowText($"+{amount}", worldPosition, healColor);
    }

    public void ShowText(string message, Vector3 worldPosition, Color color)
    {
        if (popupPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = worldPosition;
        spawnPosition.y += yOffset;

        DamagePopup popup = Instantiate(
            popupPrefab,
            spawnPosition,
            Quaternion.identity
        );

        popup.Initialize(message, color);
    }
}