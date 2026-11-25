using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private Transform targetRoot;

    [Header("Target")]
    [SerializeField] private Enemy enemyTarget;
    [SerializeField] private DefenseStructure structureTarget;
    [SerializeField] private Altar altarTarget;

    [Header("Position")]
    [SerializeField] private bool followTarget = true;
    [SerializeField] private float yOffset = 1.4f;

    [Header("Billboard")]
    [SerializeField] private bool faceCamera = true;

    [Header("Color")]
    [SerializeField] private bool changeColorByHP = true;
    [SerializeField] private Color highHPColor = Color.green;
    [SerializeField] private Color midHPColor = Color.yellow;
    [SerializeField] private Color lowHPColor = Color.red;

    [Header("Visibility")]
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float fillMoveSpeed = 4f;

    [Header("Padding")]
    [SerializeField] private float leftPadding = 2f;
    [SerializeField] private float rightPadding = 2f;
    [SerializeField] private float topPadding = 2f;
    [SerializeField] private float bottomPadding = 2f;

    private Camera mainCamera;
    private float displayedRatio = 1f;

    private void Awake()
    {
        AutoFindTarget();

        if (fillImage != null && fillRect == null)
        {
            fillRect = fillImage.rectTransform;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;

        if (TryGetHealthRatio(out float ratio))
        {
            displayedRatio = ratio;
            ApplyFillRatio(displayedRatio);
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (targetRoot == null)
        {
            AutoFindTarget();
        }

        if (followTarget && targetRoot != null)
        {
            transform.position = targetRoot.position + Vector3.up * yOffset;
        }

        if (faceCamera && mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }

        UpdateHealthBar();
    }

    private void AutoFindTarget()
    {
        if (enemyTarget == null)
        {
            enemyTarget = GetComponentInParent<Enemy>();
        }

        if (structureTarget == null)
        {
            structureTarget = GetComponentInParent<DefenseStructure>();
        }

        if (altarTarget == null)
        {
            altarTarget = GetComponentInParent<Altar>();
        }

        if (targetRoot == null)
        {
            if (enemyTarget != null)
            {
                targetRoot = enemyTarget.transform;
            }
            else if (structureTarget != null)
            {
                targetRoot = structureTarget.transform;
            }
            else if (altarTarget != null)
            {
                targetRoot = altarTarget.transform;
            }
        }
    }

    private void UpdateHealthBar()
    {
        bool hasTarget = TryGetHealthRatio(out float targetRatio);

        if (!hasTarget)
        {
            return;
        }

        targetRatio = Mathf.Clamp01(targetRatio);

        if (smoothFill)
        {
            displayedRatio = Mathf.MoveTowards(
                displayedRatio,
                targetRatio,
                fillMoveSpeed * Time.deltaTime
            );
        }
        else
        {
            displayedRatio = targetRatio;
        }

        ApplyFillRatio(displayedRatio);
        ApplyColor(targetRatio);
        ApplyVisibility(targetRatio);
    }

    private void ApplyFillRatio(float ratio)
    {
        if (fillRect == null)
        {
            return;
        }

        ratio = Mathf.Clamp01(ratio);

        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(ratio, 1f);
        fillRect.offsetMin = new Vector2(leftPadding, bottomPadding);
        fillRect.offsetMax = new Vector2(-rightPadding, -topPadding);

        if (fillImage != null)
        {
            fillImage.enabled = ratio > 0.001f;
        }
    }

    private void ApplyColor(float ratio)
    {
        if (fillImage == null || !changeColorByHP)
        {
            return;
        }

        if (ratio > 0.6f)
        {
            fillImage.color = highHPColor;
        }
        else if (ratio > 0.3f)
        {
            fillImage.color = midHPColor;
        }
        else
        {
            fillImage.color = lowHPColor;
        }
    }

    private void ApplyVisibility(float ratio)
    {
        if (canvasGroup == null)
        {
            return;
        }

        if (hideWhenFull && ratio >= 0.999f)
        {
            canvasGroup.alpha = 0f;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }

    private bool TryGetHealthRatio(out float ratio)
    {
        ratio = 1f;

        if (enemyTarget != null)
        {
            if (enemyTarget.MaxHP <= 0)
            {
                return false;
            }

            ratio = (float)enemyTarget.CurrentHP / enemyTarget.MaxHP;
            return true;
        }

        if (structureTarget != null)
        {
            if (structureTarget.MaxHP <= 0)
            {
                return false;
            }

            ratio = (float)structureTarget.CurrentHP / structureTarget.MaxHP;
            return true;
        }

        if (altarTarget != null)
        {
            if (altarTarget.MaxHP <= 0)
            {
                return false;
            }

            ratio = (float)altarTarget.CurrentHP / altarTarget.MaxHP;
            return true;
        }

        return false;
    }
}