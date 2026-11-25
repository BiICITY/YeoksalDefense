using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private Enemy basicEnemyPrefab;
    [SerializeField] private Enemy fastEnemyPrefab;
    [SerializeField] private Enemy tankEnemyPrefab;
    [SerializeField] private Enemy bossEnemyPrefab;

    [Header("References")]
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private DefenseStructure barricadeTarget;
    [SerializeField] private DefenseStructure wallTarget;
    [SerializeField] private Altar altarTarget;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GodManager godManager;

    [Header("Wave")]
    [SerializeField] private int currentWave = 1;
    [SerializeField] private int baseEnemyCount = 5;
    [SerializeField] private float spawnInterval = 0.8f;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private bool autoStartFirstWave = true;

    [Header("Boss Wave")]
    [SerializeField] private bool useBossWave = true;
    [SerializeField] private int bossWaveInterval = 5;
    [SerializeField] private int bossCount = 1;

    [Header("Enemy Scaling")]
    [SerializeField] private int baseEnemyHP = 30;
    [SerializeField] private int hpIncreasePerWave = 5;
    [SerializeField] private float baseEnemySpeed = 2f;
    [SerializeField] private float speedIncreasePerWave = 0.05f;
    [SerializeField] private int baseEnemyDamage = 5;
    [SerializeField] private int damageIncreasePerWave = 1;

    [Header("Enemy Reward")]
    [SerializeField] private int baseEnemyGoldReward = 3;
    [SerializeField] private int baseEnemySoulReward = 1;

    [Header("Enemy Variants")]
    [SerializeField] private bool useEnemyVariants = true;
    [SerializeField] private int fastEnemyStartWave = 3;
    [SerializeField] private int tankEnemyStartWave = 5;

    [Header("Fast Enemy Modifier")]
    [SerializeField] private float fastHPMultiplier = 0.65f;
    [SerializeField] private float fastSpeedMultiplier = 1.65f;
    [SerializeField] private float fastDamageMultiplier = 0.8f;
    [SerializeField] private int fastGoldBonus = 1;
    [SerializeField] private int fastSoulBonus = 0;

    [Header("Tank Enemy Modifier")]
    [SerializeField] private float tankHPMultiplier = 2.4f;
    [SerializeField] private float tankSpeedMultiplier = 0.55f;
    [SerializeField] private float tankDamageMultiplier = 1.6f;
    [SerializeField] private int tankGoldBonus = 5;
    [SerializeField] private int tankSoulBonus = 2;

    [Header("Boss Enemy Modifier")]
    [SerializeField] private float bossHPMultiplier = 8.0f;
    [SerializeField] private float bossSpeedMultiplier = 0.45f;
    [SerializeField] private float bossDamageMultiplier = 3.0f;
    [SerializeField] private int bossGoldBonus = 60;
    [SerializeField] private int bossSoulBonus = 12;

    [Header("Spawn Lanes")]
    [SerializeField] private int laneCount = 5;
    [SerializeField] private float laneSpacing = 0.55f;
    [SerializeField] private float laneJitter = 0.08f;

    [Header("Defense Reset")]
    [SerializeField] private bool repairDefensesOnWaveStart = false;

    [Header("Debug")]
    [SerializeField] private bool isWaveActive = false;
    [SerializeField] private bool isSpawning = false;
    [SerializeField] private int aliveEnemyCount = 0;

    private int totalEnemyCountThisWave = 0;

    public int CurrentWave => currentWave;
    public bool IsWaveActive => isWaveActive;

    private void Start()
    {
        if (uiManager != null)
        {
            uiManager.SetWaveManager(this);
            uiManager.UpdateWaveText(currentWave);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetAltar(altarTarget);
            GameManager.Instance.SetUIManager(uiManager);
            GameManager.Instance.SetWaveManager(this);
        }

        if (altarTarget != null)
        {
            altarTarget.ResetAltar();
        }

        if (barricadeTarget != null)
        {
            barricadeTarget.RepairFull();
        }

        if (wallTarget != null)
        {
            wallTarget.RepairFull();
        }

        if (autoStartFirstWave)
        {
            StartCoroutine(AutoStartFirstWave());
        }
    }

    private IEnumerator AutoStartFirstWave()
    {
        yield return new WaitForSeconds(startDelay);
        StartWave();
    }

    public void StartWave()
    {
        if (isWaveActive || isSpawning)
        {
            return;
        }

        if (altarTarget != null && altarTarget.IsDestroyed)
        {
            Debug.Log("Cannot start wave. Altar is destroyed.");
            return;
        }

        if (repairDefensesOnWaveStart)
        {
            if (barricadeTarget != null)
            {
                barricadeTarget.RepairFull();
            }

            if (wallTarget != null)
            {
                wallTarget.RepairFull();
            }
        }

        if (godManager != null)
        {
            godManager.ResetForNewWave();
        }

        totalEnemyCountThisWave = GetEnemyCountForWave();
        aliveEnemyCount = 0;

        isWaveActive = true;

        if (uiManager != null)
        {
            uiManager.UpdateWaveText(currentWave);
            uiManager.HideWaveClearPanel();
        }

        if (IsBossWave())
        {
            Debug.Log($"BOSS WAVE {currentWave} Start! Enemy Count: {totalEnemyCountThisWave}");
        }
        else
        {
            Debug.Log($"Wave {currentWave} Start! Enemy Count: {totalEnemyCountThisWave}");
        }

        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;

        for (int i = 0; i < totalEnemyCountThisWave; i++)
        {
            SpawnEnemy(i);
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;

        Debug.Log($"Wave {currentWave} spawn finished");

        CheckWaveClear();
    }

    private void SpawnEnemy(int index)
    {
        if (enemySpawnPoint == null)
        {
            Debug.LogError("Enemy Spawn Point is missing in WaveManager.");
            return;
        }

        EnemyVariant variant = ChooseEnemyVariant(index);
        Enemy selectedPrefab = GetEnemyPrefabByVariant(variant);

        if (selectedPrefab == null)
        {
            Debug.LogError($"Enemy prefab is missing for variant: {variant}");
            return;
        }

        float laneZ = GetLaneZ(index, variant);

        Vector3 spawnPosition = enemySpawnPoint.position;
        spawnPosition.y = 0.5f;
        spawnPosition.z = laneZ;

        Enemy enemy = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        enemy.name = $"Enemy_{variant}_W{currentWave}_{index + 1}";

        int hp = GetEnemyHPForWave();
        float speed = GetEnemySpeedForWave();
        int damage = GetEnemyDamageForWave();
        int goldReward = baseEnemyGoldReward;
        int soulReward = baseEnemySoulReward;

        ApplyVariantStats(
            variant,
            ref hp,
            ref speed,
            ref damage,
            ref goldReward,
            ref soulReward
        );

        enemy.Initialize(
            barricadeTarget,
            wallTarget,
            altarTarget,
            hp,
            speed,
            damage,
            laneZ,
            goldReward,
            soulReward,
            variant
        );

        enemy.OnDied += HandleEnemyDied;

        aliveEnemyCount++;

        if (variant == EnemyVariant.Boss)
        {
            Debug.Log("Boss Enemy Spawned!");

            if (DamagePopupManager.Instance != null)
            {
                DamagePopupManager.Instance.ShowStatus("BOSS", enemy.transform.position);
            }
        }
    }

    private EnemyVariant ChooseEnemyVariant(int index)
    {
        if (IsBossWave() && index < bossCount)
        {
            return EnemyVariant.Boss;
        }

        if (!useEnemyVariants)
        {
            return EnemyVariant.Basic;
        }

        if (currentWave < fastEnemyStartWave)
        {
            return EnemyVariant.Basic;
        }

        float roll = Random.value;

        if (currentWave < tankEnemyStartWave)
        {
            if (roll < 0.3f)
            {
                return EnemyVariant.Fast;
            }

            return EnemyVariant.Basic;
        }

        if (roll < 0.2f)
        {
            return EnemyVariant.Tank;
        }

        if (roll < 0.55f)
        {
            return EnemyVariant.Fast;
        }

        return EnemyVariant.Basic;
    }

    private Enemy GetEnemyPrefabByVariant(EnemyVariant variant)
    {
        switch (variant)
        {
            case EnemyVariant.Fast:
                if (fastEnemyPrefab != null)
                {
                    return fastEnemyPrefab;
                }

                Debug.LogWarning("Fast Enemy Prefab is missing. Basic Enemy Prefab will be used instead.");
                return basicEnemyPrefab;

            case EnemyVariant.Tank:
                if (tankEnemyPrefab != null)
                {
                    return tankEnemyPrefab;
                }

                Debug.LogWarning("Tank Enemy Prefab is missing. Basic Enemy Prefab will be used instead.");
                return basicEnemyPrefab;

            case EnemyVariant.Boss:
                if (bossEnemyPrefab != null)
                {
                    return bossEnemyPrefab;
                }

                Debug.LogWarning("Boss Enemy Prefab is missing. Tank Enemy Prefab will be used instead.");

                if (tankEnemyPrefab != null)
                {
                    return tankEnemyPrefab;
                }

                return basicEnemyPrefab;

            case EnemyVariant.Basic:
            default:
                return basicEnemyPrefab;
        }
    }

    private void ApplyVariantStats(
        EnemyVariant variant,
        ref int hp,
        ref float speed,
        ref int damage,
        ref int goldReward,
        ref int soulReward
    )
    {
        switch (variant)
        {
            case EnemyVariant.Fast:
                hp = Mathf.Max(1, Mathf.RoundToInt(hp * fastHPMultiplier));
                speed *= fastSpeedMultiplier;
                damage = Mathf.Max(1, Mathf.RoundToInt(damage * fastDamageMultiplier));
                goldReward += fastGoldBonus;
                soulReward += fastSoulBonus;
                break;

            case EnemyVariant.Tank:
                hp = Mathf.Max(1, Mathf.RoundToInt(hp * tankHPMultiplier));
                speed *= tankSpeedMultiplier;
                damage = Mathf.Max(1, Mathf.RoundToInt(damage * tankDamageMultiplier));
                goldReward += tankGoldBonus;
                soulReward += tankSoulBonus;
                break;

            case EnemyVariant.Boss:
                hp = Mathf.Max(1, Mathf.RoundToInt(hp * bossHPMultiplier));
                speed *= bossSpeedMultiplier;
                damage = Mathf.Max(1, Mathf.RoundToInt(damage * bossDamageMultiplier));
                goldReward += bossGoldBonus;
                soulReward += bossSoulBonus;
                break;

            case EnemyVariant.Basic:
            default:
                break;
        }
    }

    private bool IsBossWave()
    {
        if (!useBossWave)
        {
            return false;
        }

        if (bossWaveInterval <= 0)
        {
            return false;
        }

        return currentWave % bossWaveInterval == 0;
    }

    private float GetLaneZ(int index, EnemyVariant variant)
    {
        if (variant == EnemyVariant.Boss)
        {
            return enemySpawnPoint.position.z;
        }

        if (laneCount <= 1)
        {
            return enemySpawnPoint.position.z;
        }

        int laneIndex = index % laneCount;

        float centerOffset = (laneCount - 1) * 0.5f;
        float laneZ = (laneIndex - centerOffset) * laneSpacing;

        laneZ += Random.Range(-laneJitter, laneJitter);

        return enemySpawnPoint.position.z + laneZ;
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        enemy.OnDied -= HandleEnemyDied;

        if (godManager != null)
        {
            godManager.NotifyEnemyKilled(enemy);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GiveEnemyKillReward(enemy.GoldReward, enemy.SoulReward);
        }

        aliveEnemyCount--;
        aliveEnemyCount = Mathf.Max(aliveEnemyCount, 0);

        Debug.Log($"Alive Enemy: {aliveEnemyCount}");

        CheckWaveClear();
    }

    private void CheckWaveClear()
    {
        if (!isWaveActive) return;
        if (isSpawning) return;

        if (aliveEnemyCount <= 0)
        {
            ClearWave();
        }
    }

    private void ClearWave()
    {
        int clearedWave = currentWave;

        isWaveActive = false;

        Debug.Log($"Wave {clearedWave} Clear!");

        if (godManager != null)
        {
            godManager.OnWaveCleared();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GiveWaveClearReward(clearedWave);
        }

        currentWave++;

        if (uiManager != null)
        {
            uiManager.UpdateWaveText(currentWave);
        }
    }

    private int GetEnemyCountForWave()
    {
        int count = baseEnemyCount + (currentWave - 1) * 2;

        if (IsBossWave())
        {
            count += bossCount;
        }

        return count;
    }

    private int GetEnemyHPForWave()
    {
        return baseEnemyHP + (currentWave - 1) * hpIncreasePerWave;
    }

    private float GetEnemySpeedForWave()
    {
        return baseEnemySpeed + (currentWave - 1) * speedIncreasePerWave;
    }

    private int GetEnemyDamageForWave()
    {
        return baseEnemyDamage + (currentWave - 1) * damageIncreasePerWave;
    }
}