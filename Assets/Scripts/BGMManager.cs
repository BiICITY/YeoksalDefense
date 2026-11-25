using System.Collections;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgmClip;

    [Header("Volume")]
    [SerializeField] private float normalVolume = 0.45f;
    [SerializeField] private float duckedVolume = 0.18f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Settings")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = loop;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = normalVolume;
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayBGM();
        }
    }

    public void PlayBGM()
    {
        if (bgmClip == null)
        {
            Debug.LogWarning("BGM Clip is missing.");
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.loop = loop;
        bgmSource.volume = normalVolume;
        bgmSource.Play();

        Debug.Log("BGM started.");
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;

        bgmSource.Stop();
    }

    public void DuckBGM()
    {
        FadeToVolume(duckedVolume);
    }

    public void RestoreBGM()
    {
        FadeToVolume(normalVolume);
    }

    public void SetNormalVolume(float volume)
    {
        normalVolume = Mathf.Clamp01(volume);
        FadeToVolume(normalVolume);
    }

    private void FadeToVolume(float targetVolume)
    {
        if (bgmSource == null) return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeVolumeRoutine(targetVolume));
    }

    private IEnumerator FadeVolumeRoutine(float targetVolume)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, t);

            yield return null;
        }

        bgmSource.volume = targetVolume;
        fadeRoutine = null;
    }
}