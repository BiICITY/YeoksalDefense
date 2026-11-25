using System.Collections;
using TMPro;
using UnityEngine;

public class WaveStartUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject waveStartPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI waveTitleText;
    [SerializeField] private TextMeshProUGUI waveSubText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float showDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 0.35f;

    private void Start()
    {
        if (waveStartPanel != null)
        {
            waveStartPanel.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    public IEnumerator ShowWaveStart(int wave)
    {
        if (waveStartPanel == null)
        {
            yield break;
        }

        waveStartPanel.SetActive(true);

        if (waveTitleText != null)
        {
            waveTitleText.text = $"Wave {wave}";
        }

        if (waveSubText != null)
        {
            waveSubText.text = "Enemies Approaching...";
        }

        if (canvasGroup != null)
        {
            yield return FadeCanvasGroup(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(showDuration);
            yield return FadeCanvasGroup(1f, 0f, fadeOutDuration);
        }
        else
        {
            yield return new WaitForSeconds(showDuration);
        }

        waveStartPanel.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        canvasGroup.alpha = to;
    }
}