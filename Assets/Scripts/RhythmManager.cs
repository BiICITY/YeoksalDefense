using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum RhythmNoteType
{
    Kung,
    Gideok,
    Deoreoreoreo,
    Deok,

    // 이전 코드 호환용
    Ttak = Deok
}

public class RhythmManager : MonoBehaviour
{
    [Serializable]
    private class RhythmPattern
    {
        public string patternName;
        public RhythmNoteType[] notes;

        public RhythmPattern(string name, RhythmNoteType[] rhythmNotes)
        {
            patternName = name;
            notes = rhythmNotes;
        }
    }

    private class MarkerView
    {
        public GameObject root;
        public Image image;
        public TextMeshProUGUI text;
    }

    [Header("Panel")]
    [SerializeField] private GameObject rhythmPanel;

    [Header("Timeline UI")]
    [SerializeField] private Slider rhythmTimelineSlider;
    [SerializeField] private RectTransform noteMarkerContainer;
    [SerializeField] private GameObject noteMarkerPrefab;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI godNameText;
    [SerializeField] private TextMeshProUGUI rhythmGuideText;
    [SerializeField] private TextMeshProUGUI rhythmResultText;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Input Buttons")]
    [SerializeField] private Button kungButton;
    [SerializeField] private Button gideokButton;
    [SerializeField] private Button deoreoreoreoButton;
    [SerializeField] private Button deokButton;

    [Header("Timeline Timing")]
    [Tooltip("첫 번째 점이 시작 후 몇 초 뒤에 오는지")]
    [SerializeField] private float firstNoteDelay = 0.7f;

    [Tooltip("점과 점 사이의 시간 간격")]
    [SerializeField] private float noteInterval = 0.9f;

    [Tooltip("점 타이밍 전후로 입력을 허용하는 시간")]
    [SerializeField] private float hitWindow = 0.28f;

    [Tooltip("마지막 노트 이후 여유 시간")]
    [SerializeField] private float endGraceTime = 0.45f;

    [Header("Fail Rule")]
    [SerializeField] private int maxMissCount = 2;

    [Header("Colors")]
    [SerializeField] private Color kungColor = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private Color gideokColor = new Color(0.3f, 0.85f, 1f);
    [SerializeField] private Color deoreoreoreoColor = new Color(0.8f, 0.45f, 1f);
    [SerializeField] private Color deokColor = new Color(1f, 0.9f, 0.25f);
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip kungClip;
    [SerializeField] private AudioClip gideokClip;
    [SerializeField] private AudioClip deoreoreoreoClip;
    [SerializeField] private AudioClip deokClip;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;
    [SerializeField] private AudioClip missClip;

    [Header("Debug")]
    [SerializeField] private bool isTrialActive = false;
    [SerializeField] private bool isInputOpen = false;
    [SerializeField] private float elapsedTime = 0f;
    [SerializeField] private int currentNoteIndex = 0;
    [SerializeField] private int missCount = 0;

    private readonly List<RhythmPattern> patternLibrary = new List<RhythmPattern>();
    private readonly List<MarkerView> markerViews = new List<MarkerView>();
    private readonly List<float> noteTimes = new List<float>();

    private RhythmPattern currentPattern;
    private float totalDuration = 0f;

    private Action successCallback;
    private Action failCallback;

    private Coroutine trialRoutine;

    private void Awake()
    {
        BuildPatternLibrary();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (rhythmPanel != null)
        {
            rhythmPanel.SetActive(false);
        }

        if (rhythmTimelineSlider != null)
        {
            rhythmTimelineSlider.minValue = 0f;
            rhythmTimelineSlider.maxValue = 1f;
            rhythmTimelineSlider.value = 0f;
            rhythmTimelineSlider.interactable = false;
        }

        ConnectButtons();
    }

    private void Update()
    {
        if (!isTrialActive || !isInputOpen)
        {
            return;
        }

        HandleKeyboardInput();
    }

    private void ConnectButtons()
    {
        if (kungButton != null)
        {
            kungButton.onClick.RemoveAllListeners();
            kungButton.onClick.AddListener(() => HandleNoteInput(RhythmNoteType.Kung));
        }

        if (gideokButton != null)
        {
            gideokButton.onClick.RemoveAllListeners();
            gideokButton.onClick.AddListener(() => HandleNoteInput(RhythmNoteType.Gideok));
        }

        if (deoreoreoreoButton != null)
        {
            deoreoreoreoButton.onClick.RemoveAllListeners();
            deoreoreoreoButton.onClick.AddListener(() => HandleNoteInput(RhythmNoteType.Deoreoreoreo));
        }

        if (deokButton != null)
        {
            deokButton.onClick.RemoveAllListeners();
            deokButton.onClick.AddListener(() => HandleNoteInput(RhythmNoteType.Deok));
        }
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            HandleNoteInput(RhythmNoteType.Kung);
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            HandleNoteInput(RhythmNoteType.Gideok);
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            HandleNoteInput(RhythmNoteType.Deoreoreoreo);
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            HandleNoteInput(RhythmNoteType.Deok);
        }
    }

    public void StartRandomRhythmTrial(string godName, Action onSuccess, Action onFail)
    {
        if (patternLibrary.Count <= 0)
        {
            Debug.LogError("Rhythm pattern library is empty.");
            return;
        }

        StopTrial();

        successCallback = onSuccess;
        failCallback = onFail;

        currentPattern = patternLibrary[UnityEngine.Random.Range(0, patternLibrary.Count)];

        if (godNameText != null)
        {
            godNameText.text = godName;
        }

        trialRoutine = StartCoroutine(RhythmTrialRoutine());
    }

    private IEnumerator RhythmTrialRoutine()
    {
        isTrialActive = true;
        isInputOpen = false;
        elapsedTime = 0f;
        currentNoteIndex = 0;
        missCount = 0;

        PrepareTimeline();

        if (rhythmPanel != null)
        {
            rhythmPanel.SetActive(true);
        }

        if (rhythmResultText != null)
        {
            rhythmResultText.text = "";
        }

        if (rhythmGuideText != null)
        {
            rhythmGuideText.text = "F=KUNG  G=GIDEOK  H=DEOREOREO  J=DEOK";
        }

        yield return CountdownRoutine();

        isInputOpen = true;

        while (isTrialActive)
        {
            elapsedTime += Time.deltaTime;

            UpdateSlider();
            CheckMissedNotes();

            if (currentNoteIndex >= currentPattern.notes.Length)
            {
                EndTrial(true);
                yield break;
            }

            if (elapsedTime >= totalDuration)
            {
                EndTrial(false);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator CountdownRoutine()
    {
        if (countdownText == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(0.45f);

        countdownText.text = "2";
        yield return new WaitForSeconds(0.45f);

        countdownText.text = "1";
        yield return new WaitForSeconds(0.45f);

        countdownText.text = "START";
        yield return new WaitForSeconds(0.35f);

        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
    }

    private void PrepareTimeline()
    {
        ClearMarkers();
        noteTimes.Clear();

        if (currentPattern == null || currentPattern.notes == null)
        {
            return;
        }

        int noteCount = currentPattern.notes.Length;

        totalDuration = firstNoteDelay + (noteCount - 1) * noteInterval + hitWindow + endGraceTime;

        if (rhythmTimelineSlider != null)
        {
            rhythmTimelineSlider.value = 0f;
        }

        for (int i = 0; i < noteCount; i++)
        {
            float noteTime = firstNoteDelay + i * noteInterval;
            noteTimes.Add(noteTime);

            float normalizedPosition = noteTime / totalDuration;
            CreateMarker(i, normalizedPosition);
        }
    }

    private void CreateMarker(int noteIndex, float normalizedX)
    {
        if (noteMarkerContainer == null || noteMarkerPrefab == null)
        {
            return;
        }

        GameObject markerObject = Instantiate(noteMarkerPrefab, noteMarkerContainer);
        markerObject.SetActive(true);

        RectTransform markerRect = markerObject.GetComponent<RectTransform>();

        if (markerRect != null)
        {
            markerRect.anchorMin = new Vector2(normalizedX, 0.5f);
            markerRect.anchorMax = new Vector2(normalizedX, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.anchoredPosition = Vector2.zero;
        }

        Image markerImage = markerObject.GetComponent<Image>();
        TextMeshProUGUI markerText = markerObject.GetComponentInChildren<TextMeshProUGUI>();

        RhythmNoteType noteType = currentPattern.notes[noteIndex];

        if (markerImage != null)
        {
            markerImage.color = GetNoteColor(noteType);
        }

        if (markerText != null)
        {
            markerText.text = GetKeyLabel(noteType);
        }

        markerViews.Add(new MarkerView
        {
            root = markerObject,
            image = markerImage,
            text = markerText
        });
    }

    private void UpdateSlider()
    {
        if (rhythmTimelineSlider == null)
        {
            return;
        }

        float t = elapsedTime / totalDuration;
        rhythmTimelineSlider.value = Mathf.Clamp01(t);
    }

    private void HandleNoteInput(RhythmNoteType inputType)
    {
        if (!isTrialActive || !isInputOpen)
        {
            return;
        }

        PlayNoteSound(inputType);

        if (currentPattern == null)
        {
            return;
        }

        if (currentNoteIndex >= currentPattern.notes.Length)
        {
            return;
        }

        RhythmNoteType expectedType = currentPattern.notes[currentNoteIndex];
        float targetTime = noteTimes[currentNoteIndex];
        float timeDifference = Mathf.Abs(elapsedTime - targetTime);

        if (timeDifference <= hitWindow)
        {
            if (inputType == expectedType)
            {
                HitCurrentNote();
            }
            else
            {
                MissCurrentNote("WRONG");
            }
        }
        else
        {
            if (rhythmResultText != null)
            {
                if (elapsedTime < targetTime)
                {
                    rhythmResultText.text = "TOO EARLY";
                }
                else
                {
                    rhythmResultText.text = "TOO LATE";
                }
            }
        }
    }

    private void HitCurrentNote()
    {
        SetMarkerColor(currentNoteIndex, hitColor);

        if (rhythmResultText != null)
        {
            rhythmResultText.text = "GOOD";
        }

        currentNoteIndex++;
    }

    private void MissCurrentNote(string reason)
    {
        SetMarkerColor(currentNoteIndex, missColor);

        missCount++;

        if (missClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(missClip);
        }

        if (rhythmResultText != null)
        {
            rhythmResultText.text = $"{reason}  MISS {missCount}/{maxMissCount}";
        }

        currentNoteIndex++;

        if (missCount > maxMissCount)
        {
            EndTrial(false);
        }
    }

    private void CheckMissedNotes()
    {
        if (currentPattern == null)
        {
            return;
        }

        while (currentNoteIndex < currentPattern.notes.Length)
        {
            float targetTime = noteTimes[currentNoteIndex];

            if (elapsedTime <= targetTime + hitWindow)
            {
                break;
            }

            MissCurrentNote("MISS");
        }
    }

    private void EndTrial(bool success)
    {
        if (!isTrialActive)
        {
            return;
        }

        isTrialActive = false;
        isInputOpen = false;

        if (rhythmTimelineSlider != null)
        {
            rhythmTimelineSlider.value = 1f;
        }

        if (success)
        {
            if (rhythmResultText != null)
            {
                rhythmResultText.text = "RITUAL SUCCESS!";
            }

            if (successClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(successClip);
            }

            StartCoroutine(CloseAfterDelayRoutine(true));
        }
        else
        {
            if (rhythmResultText != null)
            {
                rhythmResultText.text = "RITUAL FAILED!";
            }

            if (failClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(failClip);
            }

            StartCoroutine(CloseAfterDelayRoutine(false));
        }
    }

    private IEnumerator CloseAfterDelayRoutine(bool success)
    {
        yield return new WaitForSeconds(0.45f);

        if (rhythmPanel != null)
        {
            rhythmPanel.SetActive(false);
        }

        ClearMarkers();

        Action callback = success ? successCallback : failCallback;

        successCallback = null;
        failCallback = null;

        callback?.Invoke();
    }

    public void StopTrial()
    {
        if (trialRoutine != null)
        {
            StopCoroutine(trialRoutine);
            trialRoutine = null;
        }

        isTrialActive = false;
        isInputOpen = false;
        elapsedTime = 0f;
        currentNoteIndex = 0;
        missCount = 0;

        if (rhythmTimelineSlider != null)
        {
            rhythmTimelineSlider.value = 0f;
        }

        if (rhythmPanel != null)
        {
            rhythmPanel.SetActive(false);
        }

        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.gameObject.SetActive(false);
        }

        ClearMarkers();

        successCallback = null;
        failCallback = null;
    }

    private void ClearMarkers()
    {
        foreach (MarkerView marker in markerViews)
        {
            if (marker != null && marker.root != null)
            {
                Destroy(marker.root);
            }
        }

        markerViews.Clear();
    }

    private void SetMarkerColor(int index, Color color)
    {
        if (index < 0 || index >= markerViews.Count)
        {
            return;
        }

        if (markerViews[index].image != null)
        {
            markerViews[index].image.color = color;
        }
    }

    private Color GetNoteColor(RhythmNoteType noteType)
    {
        switch (noteType)
        {
            case RhythmNoteType.Kung:
                return kungColor;
            case RhythmNoteType.Gideok:
                return gideokColor;
            case RhythmNoteType.Deoreoreoreo:
                return deoreoreoreoColor;
            case RhythmNoteType.Deok:
                return deokColor;
            default:
                return Color.white;
        }
    }

    private string GetKeyLabel(RhythmNoteType noteType)
    {
        switch (noteType)
        {
            case RhythmNoteType.Kung:
                return "F";
            case RhythmNoteType.Gideok:
                return "G";
            case RhythmNoteType.Deoreoreoreo:
                return "H";
            case RhythmNoteType.Deok:
                return "J";
            default:
                return "?";
        }
    }

    private void PlayNoteSound(RhythmNoteType noteType)
    {
        if (audioSource == null)
        {
            return;
        }

        AudioClip clip = null;

        switch (noteType)
        {
            case RhythmNoteType.Kung:
                clip = kungClip;
                break;
            case RhythmNoteType.Gideok:
                clip = gideokClip;
                break;
            case RhythmNoteType.Deoreoreoreo:
                clip = deoreoreoreoClip;
                break;
            case RhythmNoteType.Deok:
                clip = deokClip;
                break;
        }

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void BuildPatternLibrary()
    {
        patternLibrary.Clear();

        // Easy - 3 notes
        patternLibrary.Add(new RhythmPattern("Easy 1", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Easy 2", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Deok,
            RhythmNoteType.Kung
        }));

        patternLibrary.Add(new RhythmPattern("Easy 3", new[]
        {
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Easy 4", new[]
        {
            RhythmNoteType.Deok,
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok
        }));

        patternLibrary.Add(new RhythmPattern("Easy 5", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Deok
        }));

        // Normal - 4 notes
        patternLibrary.Add(new RhythmPattern("Normal 1", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Normal 2", new[]
        {
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Normal 3", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Normal 4", new[]
        {
            RhythmNoteType.Deok,
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Normal 5", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deok
        }));

        // Hard - 5 notes
        patternLibrary.Add(new RhythmPattern("Hard 1", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Hard 2", new[]
        {
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deoreoreoreo
        }));

        patternLibrary.Add(new RhythmPattern("Hard 3", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Hard 4", new[]
        {
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok,
            RhythmNoteType.Kung
        }));

        patternLibrary.Add(new RhythmPattern("Hard 5", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Deok,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deok,
            RhythmNoteType.Kung
        }));

        // Very Hard - 6 notes
        patternLibrary.Add(new RhythmPattern("Very Hard 1", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Deok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Very Hard 2", new[]
        {
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Deok,
            RhythmNoteType.Kung
        }));

        patternLibrary.Add(new RhythmPattern("Very Hard 3", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Deok,
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Deok
        }));

        patternLibrary.Add(new RhythmPattern("Very Hard 4", new[]
        {
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok,
            RhythmNoteType.Gideok
        }));

        patternLibrary.Add(new RhythmPattern("Very Hard 5", new[]
        {
            RhythmNoteType.Kung,
            RhythmNoteType.Gideok,
            RhythmNoteType.Kung,
            RhythmNoteType.Deok,
            RhythmNoteType.Deoreoreoreo,
            RhythmNoteType.Deok
        }));
    }
}