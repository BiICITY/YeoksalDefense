using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Top HUD")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI soulText;
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private TextMeshProUGUI altarHPText;

    [Header("Wave Clear Panel")]
    [SerializeField] private GameObject waveClearPanel;
    [SerializeField] private TextMeshProUGUI waveClearTitleText;
    [SerializeField] private TextMeshProUGUI waveClearRewardText;
    [SerializeField] private Button nextWaveButton;
    [SerializeField] private TextMeshProUGUI nextWaveButtonText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverTitleText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI restartButtonText;
    [SerializeField] private TextMeshProUGUI mainMenuButtonText;

    private WaveManager waveManager;

    private void Start()
    {
        if (waveClearPanel != null)
        {
            waveClearPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (nextWaveButton != null)
        {
            nextWaveButton.onClick.RemoveAllListeners();
            nextWaveButton.onClick.AddListener(HandleNextWaveButtonClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(HandleRestartButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(HandleMainMenuButtonClicked);
        }

        if (nextWaveButtonText != null)
        {
            nextWaveButtonText.text = "Next Wave";
        }

        if (restartButtonText != null)
        {
            restartButtonText.text = "Restart";
        }

        if (mainMenuButtonText != null)
        {
            mainMenuButtonText.text = "Main Menu";
        }
    }

    public void SetWaveManager(WaveManager manager)
    {
        waveManager = manager;
    }

    public void UpdateWaveText(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {wave}";
        }
    }

    public void UpdateResourceUI(int gold, int soul, int statPoint, int altarHP, int altarMaxHP)
    {
        if (goldText != null)
        {
            goldText.text = $"Gold {gold}";
        }

        if (soulText != null)
        {
            soulText.text = $"Soul {soul}";
        }

        if (statText != null)
        {
            statText.text = $"Stat {statPoint}";
        }

        if (altarHPText != null)
        {
            altarHPText.text = $"Altar HP {altarHP}/{altarMaxHP}";
        }
    }

    public void ShowWaveClearPanel(int clearedWave, int goldReward, int statReward)
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            return;
        }

        if (waveClearPanel != null)
        {
            waveClearPanel.SetActive(true);
        }

        if (waveClearTitleText != null)
        {
            waveClearTitleText.text = $"Wave {clearedWave} Clear!";
        }

        if (waveClearRewardText != null)
        {
            waveClearRewardText.text =
                $"Gold +{goldReward}\n" +
                $"Stat Point +{statReward}\n" +
                $"Soul gained from defeated enemies";
        }

        if (nextWaveButtonText != null)
        {
            nextWaveButtonText.text = "Next Wave";
        }
    }

    public void HideWaveClearPanel()
    {
        if (waveClearPanel != null)
        {
            waveClearPanel.SetActive(false);
        }
    }

    public void ShowGameOverPanel(int survivedWave, int totalGold, int totalSoul, int totalStat)
    {
        HideWaveClearPanel();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverTitleText != null)
        {
            gameOverTitleText.text = "GAME OVER";
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text =
                $"Survived Wave: {survivedWave}\n\n" +
                $"Total Gold Earned: {totalGold}\n" +
                $"Total Soul Earned: {totalSoul}\n" +
                $"Total Stat Earned: {totalStat}";
        }

        if (restartButtonText != null)
        {
            restartButtonText.text = "Restart";
        }

        if (mainMenuButtonText != null)
        {
            mainMenuButtonText.text = "Main Menu";
        }
    }

    private void HandleNextWaveButtonClicked()
    {
        HideWaveClearPanel();

        if (waveManager != null)
        {
            waveManager.StartWave();
        }
    }

    private void HandleRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void HandleMainMenuButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}