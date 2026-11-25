using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Resources")]
    [SerializeField] private int gold = 0;
    [SerializeField] private int soul = 0;
    [SerializeField] private int statPoint = 0;

    [Header("Total Earned For Score")]
    [SerializeField] private int totalGoldEarned = 0;
    [SerializeField] private int totalSoulEarned = 0;
    [SerializeField] private int totalStatEarned = 0;

    [Header("Wave Clear Reward")]
    [SerializeField] private int baseWaveGoldReward = 100;
    [SerializeField] private int goldRewardIncreasePerWave = 20;
    [SerializeField] private int statPointRewardPerWave = 3;

    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Altar altar;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private RhythmManager rhythmManager;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "StartScene";

    [Header("Game State")]
    [SerializeField] private bool isGameOver = false;

    public int Gold => gold;
    public int Soul => soul;
    public int StatPoint => statPoint;
    public int BaseWaveGoldReward => baseWaveGoldReward;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Time.timeScale = 1f;
    }

    private void Start()
    {
        RefreshUI();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        gold += amount;
        totalGoldEarned += amount;

        Debug.Log($"Gold +{amount} / Current Gold: {gold}");

        RefreshUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount)
        {
            Debug.Log("Not enough Gold!");
            return false;
        }

        gold -= amount;

        Debug.Log($"Gold -{amount} / Current Gold: {gold}");

        RefreshUI();
        return true;
    }

    public void AddSoul(int amount)
    {
        if (amount <= 0) return;

        soul += amount;
        totalSoulEarned += amount;

        Debug.Log($"Soul +{amount} / Current Soul: {soul}");

        RefreshUI();
    }

    public bool SpendSoul(int amount)
    {
        if (soul < amount)
        {
            Debug.Log("Not enough Soul!");
            return false;
        }

        soul -= amount;

        Debug.Log($"Soul -{amount} / Current Soul: {soul}");

        RefreshUI();
        return true;
    }

    public void AddStatPoint(int amount)
    {
        if (amount <= 0) return;

        statPoint += amount;
        totalStatEarned += amount;

        Debug.Log($"Stat Point +{amount} / Current Stat Point: {statPoint}");

        RefreshUI();
    }

    public bool SpendStatPoint(int amount)
    {
        if (statPoint < amount)
        {
            Debug.Log("Not enough Stat Point!");
            return false;
        }

        statPoint -= amount;

        Debug.Log($"Stat Point -{amount} / Current Stat Point: {statPoint}");

        RefreshUI();
        return true;
    }

    public void GiveEnemyKillReward(int goldReward, int soulReward)
    {
        if (goldReward > 0)
        {
            gold += goldReward;
            totalGoldEarned += goldReward;
        }

        if (soulReward > 0)
        {
            soul += soulReward;
            totalSoulEarned += soulReward;
        }

        Debug.Log($"Enemy Reward | Gold +{goldReward}, Soul +{soulReward}");

        RefreshUI();
    }

    public void GiveWaveClearReward(int wave)
    {
        int goldReward = baseWaveGoldReward + (wave - 1) * goldRewardIncreasePerWave;
        int statReward = statPointRewardPerWave;

        gold += goldReward;
        statPoint += statReward;

        totalGoldEarned += goldReward;
        totalStatEarned += statReward;

        Debug.Log($"Wave {wave} Reward | Gold +{goldReward}, Stat +{statReward}");

        RefreshUI();

        if (uiManager != null)
        {
            uiManager.ShowWaveClearPanel(wave, goldReward, statReward);
        }
    }

    public void IncreaseBaseWaveGoldReward(int amount)
    {
        baseWaveGoldReward += amount;

        Debug.Log($"Base Wave Gold Reward increased by {amount}. Current Base Reward: {baseWaveGoldReward}");

        RefreshUI();
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (rhythmManager != null)
        {
            rhythmManager.StopTrial();
        }

        int survivedWave = GetSurvivedWave();

        if (uiManager != null)
        {
            uiManager.ShowGameOverPanel(
                survivedWave,
                totalGoldEarned,
                totalSoulEarned,
                totalStatEarned
            );
        }

        Time.timeScale = 0f;

        Debug.Log($"GAME OVER | Survived Wave: {survivedWave}");
    }

    private int GetSurvivedWave()
    {
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }

        if (waveManager == null)
        {
            return 0;
        }

        return Mathf.Max(0, waveManager.CurrentWave - 1);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RefreshUI()
    {
        if (uiManager == null)
        {
            return;
        }

        int altarHP = altar != null ? altar.CurrentHP : 0;
        int altarMaxHP = altar != null ? altar.MaxHP : 0;

        uiManager.UpdateResourceUI(gold, soul, statPoint, altarHP, altarMaxHP);
    }

    public void SetUIManager(UIManager manager)
    {
        uiManager = manager;
        RefreshUI();
    }

    public void SetAltar(Altar targetAltar)
    {
        altar = targetAltar;
        RefreshUI();
    }

    public void SetWaveManager(WaveManager manager)
    {
        waveManager = manager;
    }

    public void SetRhythmManager(RhythmManager manager)
    {
        rhythmManager = manager;
    }
}