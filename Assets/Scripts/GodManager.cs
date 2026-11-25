using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GodManager : MonoBehaviour
{
    private enum GodType
    {
        Sangun,
        JadeEmperor,
        Yeomra,
        Seongju
    }

    [Header("God Buttons")]
    [SerializeField] private Button sangunButton;
    [SerializeField] private Button jadeEmperorButton;
    [SerializeField] private Button yeomraButton;
    [SerializeField] private Button seongjuButton;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI godUsesText;
    [SerializeField] private TextMeshProUGUI trialText;

    [Header("Rhythm")]
    [SerializeField] private RhythmManager rhythmManager;

    [Header("Avatar Spawn")]
    [SerializeField] private Transform godSpawnPoint;

    [Header("God Avatar Prefabs")]
    [SerializeField] private GameObject sangunAvatarPrefab;
    [SerializeField] private GameObject jadeEmperorAvatarPrefab;
    [SerializeField] private GameObject yeomraAvatarPrefab;
    [SerializeField] private GameObject seongjuAvatarPrefab;

    [Header("Battle References")]
    [SerializeField] private DefenseStructure barricadeTarget;
    [SerializeField] private DefenseStructure wallTarget;
    [SerializeField] private Altar altarTarget;

    [Header("Summon Settings")]
    [SerializeField] private int usesPerWave = 2;
    [SerializeField] private bool removePreviousAvatarWhenSummon = true;

    [Header("Sangun Effect")]
    [SerializeField] private int sangunDamage = 15;
    [SerializeField] private float sangunSlowMultiplier = 0.5f;
    [SerializeField] private float sangunSlowDuration = 6f;

    [Header("Jade Emperor Effect")]
    [SerializeField] private int jadeDamageBuff = 5;
    [SerializeField] private float jadeBuffDuration = 10f;

    [Header("Yeomra Effect")]
    [SerializeField] private float yeomraExecuteThreshold = 0.35f;
    [SerializeField] private int yeomraFallbackDamage = 10;
    [SerializeField] private int yeomraSoulBonus = 5;

    [Header("Seongju Effect")]
    [SerializeField] private int seongjuRepairAmount = 60;
    [SerializeField] private int seongjuAltarHealAmount = 40;

    [Header("Debug")]
    [SerializeField] private int usesLeft;
    [SerializeField] private bool hasActiveAvatar = false;
    [SerializeField] private bool isRhythmRunning = false;
    [SerializeField] private string currentGodName = "None";
    [SerializeField] private bool jadeBuffActive = false;
    [SerializeField] private float jadeBuffTimer = 0f;

    private GameObject currentAvatar;
    private GodType currentGodType;

    private void Start()
    {
        usesLeft = usesPerWave;

        RegisterButtons();
        RefreshUI("Choose a god.");
    }

    private void Update()
    {
        UpdateJadeBuff();
    }

    private void RegisterButtons()
    {
        if (sangunButton != null)
        {
            sangunButton.onClick.RemoveAllListeners();
            sangunButton.onClick.AddListener(() => SummonGod(GodType.Sangun));
        }

        if (jadeEmperorButton != null)
        {
            jadeEmperorButton.onClick.RemoveAllListeners();
            jadeEmperorButton.onClick.AddListener(() => SummonGod(GodType.JadeEmperor));
        }

        if (yeomraButton != null)
        {
            yeomraButton.onClick.RemoveAllListeners();
            yeomraButton.onClick.AddListener(() => SummonGod(GodType.Yeomra));
        }

        if (seongjuButton != null)
        {
            seongjuButton.onClick.RemoveAllListeners();
            seongjuButton.onClick.AddListener(() => SummonGod(GodType.Seongju));
        }
    }

    private void SummonGod(GodType god)
    {
        if (usesLeft <= 0)
        {
            RefreshUI("No god uses left.");
            return;
        }

        if (isRhythmRunning)
        {
            RefreshUI("Rhythm trial is already running.");
            return;
        }

        if (hasActiveAvatar && !removePreviousAvatarWhenSummon)
        {
            RefreshUI("A god is already present.");
            return;
        }

        if (removePreviousAvatarWhenSummon)
        {
            RemoveCurrentAvatar();
        }

        usesLeft--;

        currentGodType = god;
        currentGodName = GetGodName(god);

        SpawnGodAvatar(god);

        hasActiveAvatar = true;

        RefreshUI($"{currentGodName} appeared. Start rhythm!");

        StartRhythmForGod(god);

        Debug.Log($"{currentGodName} summoned. Uses left: {usesLeft}/{usesPerWave}");
    }

    private void StartRhythmForGod(GodType god)
    {
        if (rhythmManager == null)
        {
            Debug.LogWarning("RhythmManager is missing. God effect activates immediately.");
            ActivateGodEffect(god);
            RefreshUI($"{currentGodName} activated. RhythmManager missing.");
            return;
        }

        isRhythmRunning = true;
        RefreshButtons();

        rhythmManager.StartRandomRhythmTrial(
            currentGodName,
            HandleRhythmSuccess,
            HandleRhythmFail
        );
    }

    private RhythmNoteType[] GetRhythmPattern(GodType god)
    {
        switch (god)
        {
            case GodType.Sangun:
                return new RhythmNoteType[]
                {
                    RhythmNoteType.Kung,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak
                };

            case GodType.JadeEmperor:
                return new RhythmNoteType[]
                {
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak
                };

            case GodType.Yeomra:
                return new RhythmNoteType[]
                {
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak
                };

            case GodType.Seongju:
                return new RhythmNoteType[]
                {
                    RhythmNoteType.Kung,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak
                };

            default:
                return new RhythmNoteType[]
                {
                    RhythmNoteType.Kung,
                    RhythmNoteType.Ttak
                };
        }
    }

    private void HandleRhythmSuccess()
    {
        isRhythmRunning = false;

        ActivateGodEffect(currentGodType);

        RefreshUI($"{currentGodName} rhythm success. Power activated.");

        RefreshButtons();
    }

    private void HandleRhythmFail()
    {
        string failedGodName = currentGodName;

        isRhythmRunning = false;

        RemoveCurrentAvatar();

        RefreshUI($"{failedGodName} rhythm failed. God disappeared.");

        RefreshButtons();
    }

    private void ActivateGodEffect(GodType god)
    {
        switch (god)
        {
            case GodType.Sangun:
                ActivateSangun();
                break;

            case GodType.JadeEmperor:
                ActivateJadeEmperor();
                break;

            case GodType.Yeomra:
                ActivateYeomra();
                break;

            case GodType.Seongju:
                ActivateSeongju();
                break;
        }
    }

    private void ActivateSangun()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.IsDead) continue;

            enemy.TakeDamage(sangunDamage);
            enemy.ApplySlow(sangunSlowMultiplier, sangunSlowDuration);
        }

        Debug.Log("Sangun effect activated: damage and slow all enemies.");
    }

    private void ActivateJadeEmperor()
    {
        if (jadeBuffActive)
        {
            RemoveJadeBuff();
        }

        ApplySoldierDamageBuff(jadeDamageBuff);

        jadeBuffActive = true;
        jadeBuffTimer = jadeBuffDuration;

        Debug.Log("Jade Emperor effect activated: soldier damage buff.");
    }

    private void UpdateJadeBuff()
    {
        if (!jadeBuffActive) return;

        jadeBuffTimer -= Time.deltaTime;

        if (jadeBuffTimer <= 0f)
        {
            RemoveJadeBuff();
        }
    }

    private void RemoveJadeBuff()
    {
        if (!jadeBuffActive) return;

        ApplySoldierDamageBuff(-jadeDamageBuff);

        jadeBuffActive = false;
        jadeBuffTimer = 0f;

        Debug.Log("Jade Emperor buff ended.");
    }

    private void ActivateYeomra()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        int executedCount = 0;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.IsDead) continue;

            float hpRatio = enemy.MaxHP > 0 ? (float)enemy.CurrentHP / enemy.MaxHP : 1f;

            if (hpRatio <= yeomraExecuteThreshold)
            {
                enemy.TakeDamage(9999);
                executedCount++;
            }
            else
            {
                enemy.TakeDamage(yeomraFallbackDamage);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddSoul(yeomraSoulBonus);
        }

        Debug.Log($"Yeomra effect activated: executed {executedCount} enemies, Soul +{yeomraSoulBonus}.");
    }

    private void ActivateSeongju()
    {
        if (barricadeTarget != null)
        {
            if (barricadeTarget.IsDestroyed)
            {
                barricadeTarget.RepairFull();
            }
            else
            {
                barricadeTarget.Repair(seongjuRepairAmount);
            }
        }

        if (wallTarget != null)
        {
            if (wallTarget.IsDestroyed)
            {
                wallTarget.RepairFull();
            }
            else
            {
                wallTarget.Repair(seongjuRepairAmount);
            }
        }

        if (altarTarget != null)
        {
            altarTarget.Heal(seongjuAltarHealAmount);
        }

        Debug.Log("Seongju effect activated: defenses repaired.");
    }

    private void ApplySoldierDamageBuff(int amount)
    {
        Archer[] archers = FindObjectsByType<Archer>(FindObjectsSortMode.None);

        foreach (Archer archer in archers)
        {
            if (archer == null) continue;
            archer.UpgradeDamage(amount);
        }

        Spearman[] spearmen = FindObjectsByType<Spearman>(FindObjectsSortMode.None);

        foreach (Spearman spearman in spearmen)
        {
            if (spearman == null) continue;
            spearman.UpgradeDamage(amount);
        }
    }

    private void SpawnGodAvatar(GodType god)
    {
        GameObject prefab = GetAvatarPrefab(god);
        Vector3 spawnPosition = GetSpawnPosition();

        if (prefab != null)
        {
            currentAvatar = Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            currentAvatar = CreateFallbackAvatar(god, spawnPosition);
        }

        currentAvatar.name = $"{GetGodName(god)}_Avatar";
    }

    private Vector3 GetSpawnPosition()
    {
        if (godSpawnPoint != null)
        {
            return godSpawnPoint.position;
        }

        return transform.position + Vector3.up * 2f;
    }

    private GameObject GetAvatarPrefab(GodType god)
    {
        switch (god)
        {
            case GodType.Sangun:
                return sangunAvatarPrefab;

            case GodType.JadeEmperor:
                return jadeEmperorAvatarPrefab;

            case GodType.Yeomra:
                return yeomraAvatarPrefab;

            case GodType.Seongju:
                return seongjuAvatarPrefab;

            default:
                return null;
        }
    }

    private GameObject CreateFallbackAvatar(GodType god, Vector3 spawnPosition)
    {
        GameObject avatar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        avatar.transform.position = spawnPosition;
        avatar.transform.localScale = Vector3.one * 1.4f;

        Collider col = avatar.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        Renderer renderer = avatar.GetComponentInChildren<Renderer>();

        if (renderer != null)
        {
            renderer.material.color = GetGodColor(god);
        }

        return avatar;
    }

    private Color GetGodColor(GodType god)
    {
        switch (god)
        {
            case GodType.Sangun:
                return new Color(1.0f, 0.45f, 0.1f);

            case GodType.JadeEmperor:
                return new Color(1.0f, 0.9f, 0.2f);

            case GodType.Yeomra:
                return new Color(0.25f, 0.05f, 0.4f);

            case GodType.Seongju:
                return new Color(0.2f, 0.8f, 1.0f);

            default:
                return Color.white;
        }
    }

    private string GetGodName(GodType god)
    {
        switch (god)
        {
            case GodType.Sangun:
                return "Sangun";

            case GodType.JadeEmperor:
                return "Jade Emperor";

            case GodType.Yeomra:
                return "Yeomra";

            case GodType.Seongju:
                return "Seongju";

            default:
                return "God";
        }
    }

    private void RemoveCurrentAvatar()
    {
        if (currentAvatar != null)
        {
            Destroy(currentAvatar);
            currentAvatar = null;
        }

        hasActiveAvatar = false;
        currentGodName = "None";
    }

    private void RefreshUI(string message)
    {
        if (godUsesText != null)
        {
            godUsesText.text = $"God Uses {usesLeft}/{usesPerWave}";
        }

        if (trialText != null)
        {
            trialText.text = message;
        }

        RefreshButtons();
    }

    private void RefreshButtons()
    {
        bool canUse = usesLeft > 0 && !isRhythmRunning;

        if (sangunButton != null)
        {
            sangunButton.interactable = canUse;
        }

        if (jadeEmperorButton != null)
        {
            jadeEmperorButton.interactable = canUse;
        }

        if (yeomraButton != null)
        {
            yeomraButton.interactable = canUse;
        }

        if (seongjuButton != null)
        {
            seongjuButton.interactable = canUse;
        }
    }

    public void ResetForNewWave()
    {
        usesLeft = usesPerWave;
        isRhythmRunning = false;

        if (rhythmManager != null)
        {
            rhythmManager.StopTrial();
        }

        RemoveJadeBuff();
        RemoveCurrentAvatar();

        RefreshUI("Choose a god.");
    }

    public void OnWaveCleared()
    {
        isRhythmRunning = false;

        if (rhythmManager != null)
        {
            rhythmManager.StopTrial();
        }

        RemoveJadeBuff();
        RemoveCurrentAvatar();

        RefreshUI("Wave cleared. God avatar removed.");
    }

    public void NotifyEnemyKilled(Enemy enemy)
    {
        // 지금 단계에서는 별도 사용 없음.
    }

    public void IncreaseUsesPerWave(int amount)
    {
        usesPerWave += amount;
        usesLeft += amount;

        RefreshUI($"Ritual Capacity increased. God Uses {usesLeft}/{usesPerWave}");

        Debug.Log($"God Uses Per Wave increased by {amount}. Current: {usesPerWave}");
    }
}