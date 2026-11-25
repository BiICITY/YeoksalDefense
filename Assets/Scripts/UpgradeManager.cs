using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [Header("Money Facility Upgrade")]
    [SerializeField] private int moneyFacilityLevel = 1;
    [SerializeField] private int moneyFacilityUpgradeCost = 100;
    [SerializeField] private int moneyFacilityCostIncrease = 60;
    [SerializeField] private int waveGoldRewardIncrease = 30;
    [SerializeField] private TextMeshProUGUI moneyFacilityText;
    [SerializeField] private Button moneyFacilityButton;

    [Header("Archer Upgrade")]
    [SerializeField] private int archerDamageLevel = 1;
    [SerializeField] private int archerDamageUpgradeCost = 1;
    [SerializeField] private int archerDamageIncrease = 2;
    [SerializeField] private TextMeshProUGUI archerDamageText;
    [SerializeField] private Button archerDamageButton;

    [Header("Spearman Upgrade")]
    [SerializeField] private int spearmanDamageLevel = 1;
    [SerializeField] private int spearmanDamageUpgradeCost = 1;
    [SerializeField] private int spearmanDamageIncrease = 2;
    [SerializeField] private TextMeshProUGUI spearmanDamageText;
    [SerializeField] private Button spearmanDamageButton;

    private void Start()
    {
        if (moneyFacilityButton != null)
        {
            moneyFacilityButton.onClick.RemoveAllListeners();
            moneyFacilityButton.onClick.AddListener(UpgradeMoneyFacility);
        }

        if (archerDamageButton != null)
        {
            archerDamageButton.onClick.RemoveAllListeners();
            archerDamageButton.onClick.AddListener(UpgradeArcherDamage);
        }

        if (spearmanDamageButton != null)
        {
            spearmanDamageButton.onClick.RemoveAllListeners();
            spearmanDamageButton.onClick.AddListener(UpgradeSpearmanDamage);
        }

        RefreshUpgradeUI();
    }

    public void UpgradeMoneyFacility()
    {
        if (GameManager.Instance == null) return;

        bool success = GameManager.Instance.SpendGold(moneyFacilityUpgradeCost);

        if (!success)
        {
            Debug.Log("Money Facility Upgrade Failed: Not enough Gold.");
            return;
        }

        moneyFacilityLevel++;
        GameManager.Instance.IncreaseBaseWaveGoldReward(waveGoldRewardIncrease);

        moneyFacilityUpgradeCost += moneyFacilityCostIncrease;

        Debug.Log($"Money Facility upgraded to Lv.{moneyFacilityLevel}");

        RefreshUpgradeUI();
    }

    public void UpgradeArcherDamage()
    {
        if (GameManager.Instance == null) return;

        bool success = GameManager.Instance.SpendStatPoint(archerDamageUpgradeCost);

        if (!success)
        {
            Debug.Log("Archer Damage Upgrade Failed: Not enough Stat Point.");
            return;
        }

        archerDamageLevel++;

        Archer[] archers = FindObjectsByType<Archer>(FindObjectsSortMode.None);

        foreach (Archer archer in archers)
        {
            if (archer == null) continue;
            archer.UpgradeDamage(archerDamageIncrease);
        }

        Debug.Log($"Archer Damage upgraded to Lv.{archerDamageLevel}");

        RefreshUpgradeUI();
    }

    public void UpgradeSpearmanDamage()
    {
        if (GameManager.Instance == null) return;

        bool success = GameManager.Instance.SpendStatPoint(spearmanDamageUpgradeCost);

        if (!success)
        {
            Debug.Log("Spearman Damage Upgrade Failed: Not enough Stat Point.");
            return;
        }

        spearmanDamageLevel++;

        Spearman[] spearmen = FindObjectsByType<Spearman>(FindObjectsSortMode.None);

        foreach (Spearman spearman in spearmen)
        {
            if (spearman == null) continue;
            spearman.UpgradeDamage(spearmanDamageIncrease);
        }

        Debug.Log($"Spearman Damage upgraded to Lv.{spearmanDamageLevel}");

        RefreshUpgradeUI();
    }

    public void RefreshUpgradeUI()
    {
        if (moneyFacilityText != null)
        {
            moneyFacilityText.text =
                $"Money Facility Lv.{moneyFacilityLevel}\n" +
                $"Cost: Gold {moneyFacilityUpgradeCost}\n" +
                $"Effect: Wave Gold +{waveGoldRewardIncrease}";
        }

        if (archerDamageText != null)
        {
            archerDamageText.text =
                $"Archer Damage Lv.{archerDamageLevel}\n" +
                $"Cost: Stat {archerDamageUpgradeCost}\n" +
                $"Effect: Damage +{archerDamageIncrease}";
        }

        if (spearmanDamageText != null)
        {
            spearmanDamageText.text =
                $"Spearman Damage Lv.{spearmanDamageLevel}\n" +
                $"Cost: Stat {spearmanDamageUpgradeCost}\n" +
                $"Effect: Damage +{spearmanDamageIncrease}";
        }
    }
}