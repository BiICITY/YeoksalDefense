using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceShopManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DefenseStructure barricadeTarget;
    [SerializeField] private DefenseStructure wallTarget;
    [SerializeField] private Altar altarTarget;
    [SerializeField] private GodManager godManager;

    [Header("Repair Defenses")]
    [SerializeField] private int repairLevel = 1;
    [SerializeField] private int repairGoldCost = 80;
    [SerializeField] private int repairGoldCostIncrease = 30;
    [SerializeField] private int structureRepairAmount = 80;
    [SerializeField] private int altarHealAmount = 50;
    [SerializeField] private Button repairDefensesButton;
    [SerializeField] private TextMeshProUGUI repairDefensesText;

    [Header("Ritual Capacity")]
    [SerializeField] private int ritualCapacityLevel = 1;
    [SerializeField] private int ritualCapacitySoulCost = 8;
    [SerializeField] private int ritualCapacitySoulCostIncrease = 4;
    [SerializeField] private int godUseIncreaseAmount = 1;
    [SerializeField] private Button ritualCapacityButton;
    [SerializeField] private TextMeshProUGUI ritualCapacityText;

    private void Start()
    {
        if (repairDefensesButton != null)
        {
            repairDefensesButton.onClick.RemoveAllListeners();
            repairDefensesButton.onClick.AddListener(BuyRepairDefenses);
        }

        if (ritualCapacityButton != null)
        {
            ritualCapacityButton.onClick.RemoveAllListeners();
            ritualCapacityButton.onClick.AddListener(BuyRitualCapacity);
        }

        RefreshShopUI();
    }

    public void BuyRepairDefenses()
    {
        if (GameManager.Instance == null) return;

        bool success = GameManager.Instance.SpendGold(repairGoldCost);

        if (!success)
        {
            Debug.Log("Repair failed: Not enough Gold.");
            return;
        }

        RepairDefenseStructure(barricadeTarget);
        RepairDefenseStructure(wallTarget);

        if (altarTarget != null)
        {
            altarTarget.Heal(altarHealAmount);
        }

        repairLevel++;
        repairGoldCost += repairGoldCostIncrease;

        Debug.Log("Defenses repaired with Gold.");

        GameManager.Instance.RefreshUI();
        RefreshShopUI();
    }

    public void BuyRitualCapacity()
    {
        if (GameManager.Instance == null) return;

        bool success = GameManager.Instance.SpendSoul(ritualCapacitySoulCost);

        if (!success)
        {
            Debug.Log("Ritual Capacity upgrade failed: Not enough Soul.");
            return;
        }

        if (godManager != null)
        {
            godManager.IncreaseUsesPerWave(godUseIncreaseAmount);
        }

        ritualCapacityLevel++;
        ritualCapacitySoulCost += ritualCapacitySoulCostIncrease;

        Debug.Log("Ritual Capacity upgraded with Soul.");

        GameManager.Instance.RefreshUI();
        RefreshShopUI();
    }

    private void RepairDefenseStructure(DefenseStructure structure)
    {
        if (structure == null) return;

        if (structure.IsDestroyed)
        {
            structure.RepairFull();
        }
        else
        {
            structure.Repair(structureRepairAmount);
        }
    }

    private void RefreshShopUI()
    {
        if (repairDefensesText != null)
        {
            repairDefensesText.text =
                $"Repair Defenses Lv.{repairLevel}\n" +
                $"Cost: Gold {repairGoldCost}\n" +
                $"Wall/Barricade +{structureRepairAmount}\n" +
                $"Altar +{altarHealAmount}";
        }

        if (ritualCapacityText != null)
        {
            ritualCapacityText.text =
                $"Ritual Capacity Lv.{ritualCapacityLevel}\n" +
                $"Cost: Soul {ritualCapacitySoulCost}\n" +
                $"God Uses Per Wave +{godUseIncreaseAmount}";
        }
    }
}